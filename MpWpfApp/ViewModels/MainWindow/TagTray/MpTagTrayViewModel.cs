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
    public class MpTagTrayViewModel : MpUndoableObservableCollectionViewModel<MpTagTrayViewModel,MpTagTileViewModel> {
        #region Private Variables
        #endregion

        #region View Models
        public MpTagTileViewModel SelectedTagTile {
            get {
                var sttl = this.Where(tt => tt.IsSelected)?.ToList();
                if(sttl.Count > 0) {
                    return sttl[0];
                }
                return null;
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

        public Visibility NavButtonVisibility {
            get {
                if(ListBox == null || ScrollViewer == null) {
                    return Visibility.Collapsed;
                }
                ListBox.UpdateLayout();
                ScrollViewer.UpdateLayout();
                if(ScrollViewer.ExtentWidth >= ListBox.MaxWidth) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Public Methods

        public MpTagTrayViewModel() : base() {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

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
                this.Add(new MpTagTileViewModel(t));
            }
        }        

        public MpTagTrayViewModel(MpClipTrayViewModel ctrvm) : this() {
            CollectionChanged += (s, e) => {
                UpdateSortOrder();
            };
            ctrvm.ClipTileViewModels.CollectionChanged += (s, e) => {
                if(MainWindowViewModel.ClipTileSortViewModel.IsSorting) {
                    return;
                }
                if (e.NewItems != null) {
                    foreach (MpClipTileViewModel ctvm in ctrvm.ClipTileViewModels) {
                        AddClipToSudoTags(ctvm);
                    }
                    RefreshRecentTag();
                }
                if (e.OldItems != null && e.NewItems == null) {
                    RefreshAllCounts();
                    RefreshRecentTag();
                }
            };

        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {
            var tagTrayGrid = (Grid)sender;
            var tagTray = (ListBox)tagTrayGrid.FindName("TagTray");
            var leftButton = (RepeatButton)tagTrayGrid.FindName("TagTrayNavLeftButton");
            var rightButton = (RepeatButton)tagTrayGrid.FindName("TagTrayNavRightButton");
            Console.WriteLine("Max Tag Tray Width: " + MaxTagTrayWidth);
            ListBox = tagTray;
            ScrollViewer = ListBox.GetVisualDescendent<ScrollViewer>();
            //_trayCanvas = ListBox.GetVisualAncestor<Canvas>();
            IsHorizontal = true;


            tagTrayGrid.PreviewMouseDown += (s, e10) => {
                MpClipTrayViewModel.Instance.ResetClipSelection();
            };
            tagTray.Drop += (s, e2) => {
                return;
            };

            //leftButton.MouseDown += (s, e6) => {
            //    NavLeftCommand.Execute(null);
            //};
            //rightButton.MouseDown += (s, e6) => {
            //    NavRightCommand.Execute(null);
            //};
            RefreshAllCounts();

            UpdateSortOrder(true);

            GetRecentTagTileViewModel().IsSelected = true;

            OnPropertyChanged(nameof(NavButtonVisibility));
        }

        public void AddClipToSudoTags(MpClipTileViewModel ctvm) {
            GetHistoryTagTileViewModel().AddClip(ctvm);

            RefreshAllCounts();
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                this.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in this) {
                    ttvm.TagSortIdx = this.IndexOf(ttvm);
                }
            }
        }

        public void RefreshAllCounts() {
            foreach(var ttvm in this) {
                ttvm.TagClipCount = 0;
                foreach(var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    if(ttvm.IsLinkedWithClipTile(ctvm) /*&& ctvm.CopyItemType != MpCopyItemType.Composite*/) {
                        ttvm.TagClipCount++;
                    }
                    //if(ctvm.CopyItemType == MpCopyItemType.Composite) {
                    //    foreach(var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    //        if(ttvm.IsLinkedWithRtbItem(rtbvm)) {
                    //            ttvm.TagClipCount++;
                    //        }
                    //    }
                    //}
                }
            }
            //if (GetRecentTagTileViewModel().IsSelected) {
            //    //will trigger reselection in Add's property change
            //    GetRecentTagTileViewModel().IsSelected = false;
            //}
        }

        public void RefreshRecentTag() {
            if (GetRecentTagTileViewModel().TagClipCount >= Properties.Settings.Default.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    if (rtvm.IsLinkedWithClipTile(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.CopyItemCreatedDateTime).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - Properties.Settings.Default.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    rtvm.RemoveClip(rctvml[i]);
                }

                if(rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                //rtvm.TagClipCount = Properties.Settings.Default.MaxRecentClipItems;
            } else if (GetRecentTagTileViewModel().TagClipCount < Properties.Settings.Default.MaxRecentClipItems) {
                var rtvm = GetRecentTagTileViewModel();
                var rctvml = new List<MpClipTileViewModel>();
                foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    if (rtvm.IsLinkedWithClipTile(ctvm)) {
                        rctvml.Add(ctvm);
                    }
                }
                rctvml = rctvml.OrderBy(x => x.CopyItemCreatedDateTime).ToList();
                int itemsToRemoveCount = rtvm.TagClipCount - Properties.Settings.Default.MaxRecentClipItems;
                for (int i = 0; i < itemsToRemoveCount; i++) {
                    rtvm.RemoveClip(rctvml[i]);
                }

                if (rtvm.IsSelected) {
                    //will trigger reselection in Add's property change
                    rtvm.IsSelected = false;
                }
                //rtvm.TagClipCount = Properties.Settings.Default.MaxRecentClipItems;
            }
        }
        public new void Add(MpTagTileViewModel newTagTile) {
            base.Add(newTagTile);

            //watches Tag IsSelected so recent is selected if none are
            newTagTile.PropertyChanged += (s, e) => {
                MpHelpers.Instance.RunOnMainThread((Action)(() => {
                    switch (e.PropertyName) {
                    case nameof(newTagTile.IsSelected):
                        //ensure at least history is selected
                        if (newTagTile.IsSelected == false) {
                            //find all selected tag tiles
                            var selectedTagTiles = this.Where(tt => tt.IsSelected == true).ToList();
                            //if none selected select history tag
                            if (selectedTagTiles == null || selectedTagTiles.Count == 0) {
                                //GetHistoryTagTileViewModel().IsSelected = true;
                                GetRecentTagTileViewModel().IsSelected = true;
                            }
                        } else if (!MainWindowViewModel.SearchBoxViewModel.HasText && !MainWindowViewModel.IsMainWindowLocked) {
                            //this else if prevents filtered out tiles from being shown while searching and an item is 
                            //added while main window is locked
                            //MpClipTrayViewModel.Instance.FilterByAppIcon = null;
                            MpClipTrayViewModel.Instance.IsFilteringByApp = false;

                            foreach (MpClipTileViewModel ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                                //this ensures when switching between tags the last selected tag in a list reset
                                //ctvm.IsSelected = false;
                                if (newTagTile.IsLinkedWithClipTile(ctvm)) {
                                    ctvm.TileVisibility = Visibility.Visible;
                                    foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                        //if composite parent is linked show all children
                                        rtbvm.SubItemVisibility = Visibility.Visible;
                                    }
                                } //below was for composite but fixing just to compile right now
                                    if (ctvm.CopyItemType == MpCopyItemType.RichText) {
                                    bool hasSubLink = false;
                                    foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                        if (newTagTile.IsLinkedWithRtbItem(rtbvm)) {
                                            rtbvm.HostClipTileViewModel.TileVisibility = Visibility.Visible;
                                            rtbvm.SubItemVisibility = Visibility.Visible;
                                            hasSubLink = true;
                                        }
                                    }
                                    if (!hasSubLink) {
                                        ctvm.TileVisibility = Visibility.Collapsed;
                                    }
                                } else {
                                    ctvm.TileVisibility = Visibility.Collapsed;
                                }
                            }
                            if (MpClipTrayViewModel.Instance.ClipTileViewModels.ListBox != null) {
                                //this ensures visibility takes affect if filtering by app
                                //MpClipTrayViewModel.Instance.GetTray().Items.Refresh();
                            }

                            if (MpClipTrayViewModel.Instance.VisibileClipTiles.Count > 0 &&
                                !MpClipTrayViewModel.Instance.IsAnyContextMenuOpened) {
                                MpClipTrayViewModel.Instance.ResetClipSelection();
                            }

                        }

                        base.OnPropertyChanged(nameof(NavButtonVisibility));
                        break;
                    }
                }));                
            };
        }        

        public new void Remove(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            base.Remove(tagTileToRemove);

            if(!tagTileToRemove.Tag.IsSyncing) {
                tagTileToRemove.Tag.DeleteFromDatabase();
            }

            //remove any shortcuts associated with clip
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.TagId == tagTileToRemove.TagId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }

            ResetTagSelection();
            OnPropertyChanged(nameof(NavButtonVisibility));
        }

        public void ClearTagEditing() {
            foreach(var ttvm in this) {
                ttvm.IsEditing = false;
            }
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            foreach (var tagTile in this) {
                tagTile.IsSelected = false;
            }
        }

        public void ResetTagSelection() {
            ClearTagSelection();
            GetRecentTagTileViewModel().IsSelected = true;
        }

        public void UpdateTagAssociation() {
            foreach (var ttvm in this) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }

                bool isTagLinkedToAnySelectedClips = false;
                foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedClipTiles) {
                    if (ttvm.IsLinkedWithClipTile(sctvm)) {
                        isTagLinkedToAnySelectedClips = true;
                    }
                    foreach(var srtbvm in sctvm.RichTextBoxViewModelCollection) {
                        if(ttvm.IsLinkedWithRtbItem(srtbvm)) {
                            isTagLinkedToAnySelectedClips = true;
                        }
                    }
                }
                ttvm.IsAssociated = isTagLinkedToAnySelectedClips && MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 0;

            }
        }

        public MpTagTileViewModel GetHistoryTagTileViewModel() {
            return this.Where(tt => tt.Tag.Id == MpTag.AllTagId).ToList()[0];
        }

        public MpTagTileViewModel GetRecentTagTileViewModel() {
            return this.Where(tt => tt.Tag.Id == MpTag.RecentTagId).ToList()[0];
        }
        #endregion

        #region Private Methods

        #region Model Sync Events
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpTag t) {                    
                    var ttvmToRemove = this.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
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
                    var dupCheck = this.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (dupCheck == null) {
                        this.Add(new MpTagTileViewModel(t));
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
                TagSortIdx = this.Count
            };
            this.Add(new MpTagTileViewModel(newTag));
        }

        private RelayCommand _navRightCommand = null;
        public ICommand NavRightCommand {
            get {
                if(_navRightCommand == null) {
                    _navRightCommand = new RelayCommand(NavRight, CanNavRight);
                }
                return _navRightCommand;
            }
        }
        private bool CanNavRight() {
            return true;
        }
        private void NavRight() {
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + 20);
        }

        private RelayCommand _navLeftCommand = null;
        public ICommand NavLeftCommand {
            get {
                if (_navLeftCommand == null) {
                    _navLeftCommand = new RelayCommand(NavLeft, CanNavLeft);
                }
                return _navLeftCommand;
            }
        }
        private bool CanNavLeft() {
            return true;
        }
        private void NavLeft() {
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - 20);
        }
        #endregion
    }
}
