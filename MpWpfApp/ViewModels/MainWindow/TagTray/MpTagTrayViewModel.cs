using FFImageLoading.Helpers.Exif;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpTagTrayViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpTagTrayViewModel> _Lazy = new Lazy<MpTagTrayViewModel>(() => new MpTagTrayViewModel());
        public static MpTagTrayViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion

        #region Private Variables
        #endregion

        #region View Models
        public ObservableCollection<MpTagTileViewModel> TagTileViewModels { get; private set; } = new ObservableCollection<MpTagTileViewModel>();

        public MpTagTileViewModel SelectedTagTile {
            get {
                return TagTileViewModels.Where(x => x.IsSelected).FirstOrDefault();
            }
        }
        #endregion

        #region Properties
        public bool IsEditingTagName {
            get {
                return SelectedTagTile.IsEditing;
            }
        }

        private double _maxTagTrayWidth = MpMeasurements.Instance.MaxTagTrayWidth;
        public double MaxTagTrayWidth { 
            get {
                return _maxTagTrayWidth;
            }
            set {
                if(_maxTagTrayWidth != value) {
                    _maxTagTrayWidth = value;
                    OnPropertyChanged(nameof(MaxTagTrayWidth));
                }
            }
        }

        #endregion

        #region Events
        public event EventHandler<int> OnTagSelectionChanged;
        #endregion

        #region Public Methods

        public MpTagTrayViewModel() : base(null) {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += MpTagTrayViewModel_PropertyChanged;

            var allTags = MpDb.Instance.GetItems<MpTag>();
            if(allTags.Count == 0) {
                //occurs on first load
                var t = new MpTag() {
                    TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                    TagName = "Recent",
                    HexColor = MpHelpers.Instance.ConvertColorToHex(Colors.Green),
                    TagSortIdx = 0
                };
                t.WriteToDatabase("", true, true);
                //allTags.Add(t);

                t = new MpTag() {
                    TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                    TagName = "All",
                    HexColor = MpHelpers.Instance.ConvertColorToHex(Colors.Blue),
                    TagSortIdx = 1
                };
                t.WriteToDatabase("", true, true);
                //allTags.Add(t);

                t = new MpTag() {
                    TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
                    TagName = "Favorites",
                    HexColor = MpHelpers.Instance.ConvertColorToHex(Colors.Yellow),
                    TagSortIdx = 2
                };
                t.WriteToDatabase("", true, true);
                //allTags.Add(t);

                t = new MpTag() {
                    TagGuid = Guid.Parse("a0567976-dba6-48fc-9a7d-cbd306a4eaf3"),
                    TagName = "Help",
                    HexColor = MpHelpers.Instance.ConvertColorToHex(Colors.Orange),
                    TagSortIdx = 3
                };
                t.WriteToDatabase("", true, true);

                //allTags.Add(t);
            }
            //create tiles for all the tags
            foreach (MpTag t in MpDb.Instance.GetItems<MpTag>()) {
                this.Add(new MpTagTileViewModel(this,t));
            }

            TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;

            MpClipTrayViewModel.Instance.OnTilesChanged += ClipTrayViewModels_OnTilesChanged;

            OnViewModelLoaded();
        }

        private void ClipTrayViewModels_OnTilesChanged(object sender, object e) {
            if (MainWindowViewModel.ClipTileSortViewModel.IsSorting) {
                return;
            }

            RefreshAllCounts();
            RefreshRecentTag();
        }

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateSortOrder();
        }

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedTagTile):
                    if(SelectedTagTile != null && SelectedTagTile.Tag == null) {
                        //MpClipTrayViewModel.Instance.RefreshClips();
                    }
                    break;
            }
        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {


            //leftButton.MouseDown += (s, e6) => {
            //    NavLeftCommand.Execute(null);
            //};
            //rightButton.MouseDown += (s, e6) => {
            //    NavRightCommand.Execute(null);
            //};
            RefreshAllCounts();

            UpdateSortOrder(true);

            GetRecentTagTileViewModel().IsSelected = true;
        }

        public void AddClipToSudoTags(MpClipTileViewModel ctvm) {
            foreach(var ivm in ctvm.ItemViewModels) {
                GetHistoryTagTileViewModel().AddClip(ivm);

            }
            RefreshAllCounts();
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                TagTileViewModels.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in TagTileViewModels) {
                    ttvm.TagSortIdx = TagTileViewModels.IndexOf(ttvm);
                }
            }
        }

        public void RefreshAllCounts() {
            var acil = MpDb.Instance.GetItems<MpCopyItem>();
            foreach (var ttvm in TagTileViewModels) {
                ttvm.TagClipCount = 0;
                foreach(var ctvm in acil) {
                    if(ttvm.IsLinked(ctvm)) { 
                        ttvm.TagClipCount++;
                    }
                }
            }
        }

        public void RefreshRecentTag() {
            return;
            if (GetRecentTagTileViewModel().TagClipCount >= MpMeasurements.Instance.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    if (rtvm.IsLinked(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.ItemViewModels.Max(y=>y.CopyItem.CopyDateTime)).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - MpMeasurements.Instance.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    foreach(var rivm in rctvml[i].ItemViewModels) {
                        rtvm.RemoveClip(rivm);
                    }
                }

                if(rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                //rtvm.TagClipCount = MpMeasurements.Instance.MaxRecentClipItems;
            } else if (GetRecentTagTileViewModel().TagClipCount < MpMeasurements.Instance.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    if (rtvm.IsLinked(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.ItemViewModels.Max(y => y.CopyItem.CopyDateTime)).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - MpMeasurements.Instance.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    foreach (var rivm in rctvml[i].ItemViewModels) {
                        rtvm.RemoveClip(rivm);
                    }
                }

                if (rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                //rtvm.TagClipCount = MpMeasurements.Instance.MaxRecentClipItems;
            }
        }
        public void Add(MpTagTileViewModel newTagTile) {
            newTagTile.PropertyChanged += NewTagTile_PropertyChanged;
            TagTileViewModels.Add(newTagTile);

            ////watches Tag IsSelected so recent is selected if none are
            //newTagTile.PropertyChanged += (s, e) => {
            //    MpHelpers.Instance.RunOnMainThread((Action)(() => {
            //        switch (e.PropertyName) {
            //        case nameof(newTagTile.IsSelected):
            //                if(newTagTile.IsSelected) {
            //                    foreach(var ttvm in TagTileViewModels) {
            //                        if(ttvm != newTagTile) {
            //                            ttvm.IsSelected = false;
            //                        }
            //                    }
            //                    if (!MainWindowViewModel.SearchBoxViewModel.HasText && !MainWindowViewModel.IsMainWindowLocked) {
            //                        //this else if prevents filtered out tiles from being shown while searching and an item is 
            //                        //added while main window is locked
            //                        //MpClipTrayViewModel.Instance.FilterByAppIcon = null;
            //                        MpClipTrayViewModel.Instance.IsFilteringByApp = false;
            //                        MpClipTrayViewModel.Instance.RefreshClips();
            //                    }
            //                } else if(SelectedTagTile == null) {
            //                    GetRecentTagTileViewModel().IsSelected = true;
            //                }
            //                break;
            ////ensure at least history is selected
            //if (newTagTile.IsSelected == false) {
            //    //find all selected tag tiles
            //    var selectedTagTiles = TagTileViewModels.Where(tt => tt.IsSelected == true).ToList();
            //    //if none selected select history tag
            //    if (selectedTagTiles == null || selectedTagTiles.Count == 0) {
            //        //GetHistoryTagTileViewModel().IsSelected = true;
            //        GetRecentTagTileViewModel().IsSelected = true;
            //    }
            //} else if (!MainWindowViewModel.SearchBoxViewModel.HasText && !MainWindowViewModel.IsMainWindowLocked) {
            //    //this else if prevents filtered out tiles from being shown while searching and an item is 
            //    //added while main window is locked
            //    //MpClipTrayViewModel.Instance.FilterByAppIcon = null;
            //    MpClipTrayViewModel.Instance.IsFilteringByApp = false;
            //        MpClipTrayViewModel.Instance.RefreshClips();

            //foreach (MpClipTileViewModel ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
            //    //this ensures when switching between tags the last selected tag in a list reset
            //    //ctvm.IsSelected = false;
            //    if (newTagTile.IsLinked(ctvm)) {
            //        ctvm.ItemVisibility = Visibility.Visible;
            //        foreach (var rtbvm in ctvm.ItemViewModels) {
            //            //if composite parent is linked show all children
            //            rtbvm.ItemVisibility = Visibility.Visible;
            //        }
            //    } //below was for composite but fixing just to compile right now
            //        if (ctvm.IsTextItem) {
            //        bool hasSubLink = false;
            //        foreach (var rtbvm in ctvm.ItemViewModels) {
            //            if (newTagTile.IsLinked(rtbvm)) {
            //                rtbvm.Parent.ItemVisibility = Visibility.Visible;
            //                rtbvm.ItemVisibility = Visibility.Visible;
            //                hasSubLink = true;
            //            }
            //        }
            //        if (!hasSubLink) {
            //            ctvm.ItemVisibility = Visibility.Collapsed;
            //        }
            //    } else {
            //        ctvm.ItemVisibility = Visibility.Collapsed;
            //    }
            //}

            //if (MpClipTrayViewModel.Instance.VisibileClipTiles.Count > 0 &&
            //    !MpClipTrayViewModel.Instance.IsAnyContextMenuOpened) {
            //    MpClipTrayViewModel.Instance.ResetClipSelection();
            //}

            //            }

            //            break;
            //        }
            //    }));                
            //};
        }

        private void NewTagTile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ttvm = sender as MpTagTileViewModel;
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                switch (e.PropertyName) {
                    case nameof(ttvm.IsSelected):
                        if (ttvm.IsSelected) {
                            foreach (var t in TagTileViewModels) {
                                if (t != ttvm) {
                                    t.IsSelected = false;
                                }
                            }
                            if (!MainWindowViewModel.SearchBoxViewModel.HasText && !MainWindowViewModel.IsMainWindowLocked) {
                                //this else if prevents filtered out tiles from being shown while searching and an item is 
                                //added while main window is locked
                                //MpClipTrayViewModel.Instance.FilterByAppIcon = null;
                                MpClipTrayViewModel.Instance.IsFilteringByApp = false;
                                MpClipTrayViewModel.Instance.RefreshClips();
                            }
                        } else if (SelectedTagTile == null) {
                            GetRecentTagTileViewModel().IsSelected = true;
                        }
                        break;
                }
            }));
        }

        public void Remove(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            TagTileViewModels.Remove(tagTileToRemove);

            if(!tagTileToRemove.Tag.IsSyncing) {
                tagTileToRemove.Tag.DeleteFromDatabase();
            }

            //remove any shortcuts associated with clip
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.TagId == tagTileToRemove.TagId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }

            ResetTagSelection();
        }

        public void ClearTagEditing() {
            foreach(var ttvm in TagTileViewModels) {
                ttvm.IsEditing = false;
            }
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            foreach (var tagTile in TagTileViewModels) {
                tagTile.IsSelected = false;
            }
        }

        public void ResetTagSelection() {
            ClearTagSelection();
            GetRecentTagTileViewModel().IsSelected = true;
        }

        public void UpdateTagAssociation() {
            foreach (var ttvm in TagTileViewModels) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }

                bool isTagLinkedToAnySelectedClips = false;
                foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedClipTiles) {
                    if (ttvm.IsLinked(sctvm)) {
                        isTagLinkedToAnySelectedClips = true;
                    }
                    foreach(var srtbvm in sctvm.ItemViewModels) {
                        if(ttvm.IsLinked(srtbvm)) {
                            isTagLinkedToAnySelectedClips = true;
                        }
                    }
                }
                ttvm.IsAssociated = isTagLinkedToAnySelectedClips && MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 0;

            }
        }

        public MpTagTileViewModel GetHistoryTagTileViewModel() {
            return TagTileViewModels.Where(tt => tt.Tag.Id == MpTag.AllTagId).ToList()[0];
        }

        public MpTagTileViewModel GetRecentTagTileViewModel() {
            return TagTileViewModels.Where(tt => tt.Tag.Id == MpTag.RecentTagId).ToList()[0];
        }
        #endregion

        #region Private Methods

        #region Model Sync Events
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpTag t) {                    
                    var ttvmToRemove = TagTileViewModels.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (ttvmToRemove != null) {
                        ttvmToRemove.Tag.StartSync(e.SourceGuid);
                        this.Remove(ttvmToRemove);
                        ttvmToRemove.Tag.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpTag t) {
                    t.StartSync(e.SourceGuid);
                    var dupCheck = TagTileViewModels.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (dupCheck == null) {
                        this.Add(new MpTagTileViewModel(this,t));
                    } else {
                        MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
                        dupCheck.Tag = t;
                    }
                    t.EndSync();
                }
            }));
        }

        #endregion

        #endregion

        #region Commands
        private RelayCommand _deleteTagCommand;
        public ICommand DeleteTagCommand {
            get {
                if (_deleteTagCommand == null) {
                    _deleteTagCommand = new RelayCommand(DeleteTag, CanDeleteTag);
                }
                return _deleteTagCommand;
            }
        }
        private bool CanDeleteTag() {
            //allow delete if any tag besides history tag is selected, delete method will ignore history\
            if(SelectedTagTile == null) {
                return false;
            }
            return !SelectedTagTile.IsTagReadOnly;
        }
        private void DeleteTag() {
            this.Remove(SelectedTagTile);
        }

        private RelayCommand _createTagCommand;
        public ICommand CreateTagCommand {
            get {
                if (_createTagCommand == null) {
                    _createTagCommand = new RelayCommand(CreateTag);
                }
                return _createTagCommand;
            }
        }
        private void CreateTag() {
            //add tag to datastore so TagTile collection will automatically add the tile
            MpTag newTag = new MpTag() {
                TagName = "Untitled",
                HexColor = MpHelpers.Instance.GetRandomColor().ToString(),
                TagSortIdx = TagTileViewModels.Count
            };
            this.Add(new MpTagTileViewModel(this,newTag));
        }
        #endregion
    }
}
