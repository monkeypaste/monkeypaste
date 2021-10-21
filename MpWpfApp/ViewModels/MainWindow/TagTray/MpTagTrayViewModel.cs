using FFImageLoading.Helpers.Exif;
using Microsoft.Toolkit.Mvvm.Input;
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

        public MpTagTileViewModel SelectedTagTile => TagTileViewModels.Where(x => x.IsSelected).FirstOrDefault();

        public MpTagTileViewModel AllTagViewModel => TagTileViewModels.Where(tt => tt.Tag.Id == MpTag.AllTagId).FirstOrDefault();

        public MpTagTileViewModel RecentTagViewModel => TagTileViewModels.Where(tt => tt.Tag.Id == MpTag.RecentTagId).FirstOrDefault();
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
                this.Add(CreateTagTileViewModel(t));
            }

            TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;
        }


        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateSortOrder();
        }

        public MpTagTileViewModel CreateTagTileViewModel(MpTag tag) {
            var ttvm = new MpTagTileViewModel(this, tag);
            ttvm.PropertyChanged += Ttvm_PropertyChanged;
            return ttvm;
        }

        private void Ttvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ttvm = sender as MpTagTileViewModel;
            switch(e.PropertyName) {
                case nameof(ttvm.IsSelected):
                    if(ttvm.IsSelected) {
                        MpMessenger.Instance.Send<bool>(true);

                        OnTagSelectionChanged?.Invoke(this, ttvm.TagId);
                    }
                    break;
            }
        }

        public void TagTray_Loaded(object sender, RoutedEventArgs e) {
            RefreshAllCounts();

            UpdateSortOrder(true);

            RecentTagViewModel.IsSelected = true;
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
            Task.Run(async () => {
                //var acil = MpDb.Instance.GetItems<MpCopyItem>();
                var countTasks = new Dictionary<int,Task<int>>();
                foreach (var ttvm in TagTileViewModels) {
                    if (ttvm.IsAllTag) {
                        countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetTotalCopyItemCountAsync());
                        //ttvm.TagClipCount = await MpCopyItemProvider.Instance.GetTotalCopyItemCountAsync();
                    } else if (ttvm.IsRecentTag) {
                        countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetRecentCopyItemCountAsync());
                        //ttvm.TagClipCount = await MpCopyItemProvider.Instance.GetRecentCopyItemCountAsync();
                    } else {
                        countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetTagItemCountAsync(ttvm.TagId));
                        //ttvm.TagClipCount = await MpCopyItemProvider.Instance.GetTagItemCountAsync(ttvm.TagId);
                    }
                    //ttvm.TagClipCount = 0;
                    //foreach(var ci in acil) {
                    //    if(ttvm.IsLinked(ci)) { 
                    //        ttvm.TagClipCount++;
                    //    }
                    //}
                }

                await Task.WhenAll(countTasks.Values.ToArray());

                foreach(var ct in countTasks) {
                    int count = await ct.Value;
                    var ttvm = TagTileViewModels.Where(x => x.TagId == ct.Key).FirstOrDefault();
                    if(ttvm != null) {
                        ttvm.TagClipCount = count;
                    }
                }
            });
        }

        public void Add(MpTagTileViewModel newTagTile) {
            newTagTile.PropertyChanged += NewTagTile_PropertyChanged;
            TagTileViewModels.Add(newTagTile);
        }

        private void NewTagTile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ttvm = sender as MpTagTileViewModel;
            MpHelpers.Instance.RunOnMainThreadAsync(() => {
                switch (e.PropertyName) {
                    case nameof(ttvm.IsSelected):
                        if (ttvm.IsSelected) {
                            foreach (var t in TagTileViewModels) {
                                if (t != ttvm) {
                                    t.IsSelected = false;
                                }
                            }
                            //if (!MainWindowViewModel.SearchBoxViewModel.HasText && 
                            //    !MainWindowViewModel.IsMainWindowLocked &&
                            //    !MpMainWindowViewModel.IsMainWindowLoading) {
                            //    //this prevents filtered out tiles from being shown while searching and an item is 
                            //    //added while main window is locked
                            //    //MpClipTrayViewModel.Instance.FilterByAppIcon = null;
                            //    MpClipTrayViewModel.Instance.IsFilteringByApp = false;
                            //    MpClipTrayViewModel.Instance.RefreshTiles();
                            //}
                        } else if (SelectedTagTile == null && !MpMainWindowViewModel.IsMainWindowLoading) {
                            //RecentTagViewModel.IsSelected = true;
                        }
                        break;
                }
            },System.Windows.Threading.DispatcherPriority.Normal);
        }

        public void Remove(MpTagTileViewModel tagTileToRemove) {
            //when removing a tag auto-select the history tag
            TagTileViewModels.Remove(tagTileToRemove);

            if(!tagTileToRemove.Tag.IsSyncing) {
                tagTileToRemove.Tag.DeleteFromDatabase();
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
            if(SelectedTagTile != RecentTagViewModel) {
                ClearTagSelection();
                RecentTagViewModel.IsSelected = true;
            }            
        }

        public void UpdateTagAssociation() {
            foreach (var ttvm in TagTileViewModels) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }
                var ciidl = MpDataModelProvider.Instance.GetCopyItemIdsForTag(ttvm.TagId);

                bool isTagLinkedToAnySelectedClips = false;
                foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedItems) {
                    if(sctvm.ItemViewModels.Select(x=>x.CopyItemId).Any(x=>ciidl.Contains(x))) {
                        isTagLinkedToAnySelectedClips = true;
                        break;
                    }
                }
                ttvm.IsAssociated = isTagLinkedToAnySelectedClips && MpClipTrayViewModel.Instance.SelectedItems.Count > 0;

            }
        }

        #endregion

        #region Private Methods

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                
            }
        }

        #endregion

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
