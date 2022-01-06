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
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;

namespace MpWpfApp {

    public class MpTagTrayViewModel : MpSingletonViewModel<MpTagTrayViewModel>, MpITreeItemViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region MpITreeItemViewModel Implementation

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
        public bool IsExpanded { get; set; }

        public MpITreeItemViewModel ParentTreeItem => MpSideBarTreeCollectionViewModel.Instance;


        public ObservableCollection<MpITreeItemViewModel> Children {
            get {
                return new ObservableCollection<MpITreeItemViewModel>(RootTagTileViewModels.Cast<MpITreeItemViewModel>());
            }
            set {
                if(Children != value) {
                    RootTagTileViewModels = new ObservableCollection<MpTagTileViewModel>(value.Cast<MpTagTileViewModel>());
                    OnPropertyChanged(nameof(Children));
                }
            }
        }

        #endregion

        #region View Models
        public ObservableCollection<MpTagTileViewModel> RootTagTileViewModels { get; private set; } = new ObservableCollection<MpTagTileViewModel>();

        public ObservableCollection<MpTagTileViewModel> PinnedItems {
            get {
                return new ObservableCollection<MpTagTileViewModel>(TagTileViewModels.Where(x => x.IsPinned));
            }
        }

        //public MpTagTileViewModel RootTagTileViewModel { get; private set; }

        //public ObservableCollection<MpTagTileViewModel> TagTileViewModels { get; private set; } = new ObservableCollection<MpTagTileViewModel>();
        public ObservableCollection<MpTagTileViewModel> TagTileViewModels {
            get {
                return FindAllTagTileViewModels();
            }
        }

        public MpTagTileViewModel SelectedTagTile => TagTileViewModels.FirstOrDefault(x => x.IsSelected);

        public MpTagTileViewModel AllTagViewModel => TagTileViewModels.FirstOrDefault(tt => tt.Tag.Id == MpTag.AllTagId);

        public MpTagTileViewModel RecentTagViewModel => TagTileViewModels.FirstOrDefault(tt => tt.Tag.Id == MpTag.RecentTagId);

        #endregion

        #region State

        public bool IsVisible { get; set; }

        public bool IsEditingTagName {
            get {
                if(SelectedTagTile == null) {
                    return false;
                }
                return SelectedTagTile.IsEditing;
            }
        }

        #endregion

        #region Layout

        private double _TagTrayDefaultMaxWidth = MpMeasurements.Instance.TagTrayDefaultMaxWidth;
        public double TagTrayDefaultMaxWidth { 
            get {
                return _TagTrayDefaultMaxWidth;
            }
            set {
                if(_TagTrayDefaultMaxWidth != value) {
                    _TagTrayDefaultMaxWidth = value;
                    OnPropertyChanged(nameof(TagTrayDefaultMaxWidth));
                }
            }
        }

        #endregion

        #region Business Logic

        public int DefaultTagId {
            get {
                return MpTag.AllTagId;
            }
        }

        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MpTagTrayViewModel() : base() {
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }


        public async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                IsBusy = true;

                MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
                MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

                List<MpTag> allTags = await MpDb.Instance.GetItemsAsync<MpTag>();

                var rttvm = await CreateTagTileViewModel(allTags.FirstOrDefault(x => x.Id == MpTag.AllTagId));

                RootTagTileViewModels.Add(rttvm);
                //create tiles for all the tags
                //foreach (MpTag t in allTags) {
                //    var ttvm = await CreateTagTileViewModel(t);
                //    TagTileViewModels.Add(ttvm);
                //}

                //foreach(var srttvm in RootTagTileViewModels) {
                //    foreach (var t in allTags.Where(x => x.ParentTagId == srttvm.TagId)) {
                //        var ttvm = await CreateTagTileViewModel(t);

                //        ttvm.ParentTagViewModel = srttvm;
                //        srttvm.ChildTagViewModels.Add(ttvm);
                //        srttvm.OnPropertyChanged(nameof(srttvm.ChildTagViewModels));
                //    }

                //}

                OnPropertyChanged(nameof(RootTagTileViewModels));
                OnPropertyChanged(nameof(TagTileViewModels));

                TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;

                //UpdateSortOrder(true);

                TagTileViewModels.FirstOrDefault(x => x.TagId == DefaultTagId).IsSelected = true;

                await RefreshAllCounts();

                RootTagTileViewModels[0].IsExpanded = true;

                //SelectTagCommand.Execute(DefaultTagId);

                IsBusy = false;
            });
        }
        #endregion

        #region Public Methods

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateSortOrder();
        }

        public async Task<MpTagTileViewModel> CreateTagTileViewModel(MpTag tag) {
            MpTagTileViewModel ttvm = new MpTagTileViewModel(this);
            await ttvm.InitializeAsync(tag);

            var ctl = await MpDataModelProvider.Instance.GetChildTagsAsync(ttvm.TagId);
            foreach(var ct in ctl) {
                var cttvm = await CreateTagTileViewModel(ct);
                cttvm.ParentTreeItem = ttvm;
                ttvm.Children.Add(cttvm);
            }
            return ttvm;
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                //TagTileViewModels.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in TagTileViewModels) {
                    ttvm.TagSortIdx = TagTileViewModels.IndexOf(ttvm);
                }
            }
        }

        public ObservableCollection<MpTagTileViewModel> FindAllTagTileViewModels() {
            var ttvml = new List<MpTagTileViewModel>();
            if (RootTagTileViewModels != null) {
                foreach (var rttvm in RootTagTileViewModels) {
                    ttvml.Add(rttvm);
                    ttvml.AddRange(rttvm.FindChildren());
                }
            }
            return new ObservableCollection<MpTagTileViewModel>(ttvml);
        }

        public async Task RefreshAllCounts() {
            var countTasks = new Dictionary<int, Task<int>>();
            foreach (var ttvm in TagTileViewModels) {
                if (ttvm.IsAllTag) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetTotalCopyItemCountAsync());
                } else if (ttvm.IsRecentTag) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetRecentCopyItemCountAsync());
                } else {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetCopyItemCountForTagAsync(ttvm.TagId));
                }
            }

            await Task.WhenAll(countTasks.Values.ToArray());

            foreach (var ct in countTasks) {
                int count = await ct.Value;
                var ttvm = TagTileViewModels.FirstOrDefault(x => x.TagId == ct.Key);
                if (ttvm != null) {
                    ttvm.TagClipCount = count;
                }
            }           
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
            if(SelectedTagTile.TagId != DefaultTagId) {
                ClearTagSelection();
                TagTileViewModels.Where(x => x.TagId == DefaultTagId).FirstOrDefault().IsSelected = true;
            }            
        }

        public async Task UpdateTagAssociation() {
            foreach (var ttvm in TagTileViewModels) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }
                var ciidl = await MpDataModelProvider.Instance.GetCopyItemIdsForTagAsync(ttvm.TagId);

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

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsVisible):
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.IsGridSplitterEnabled));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeButtonGridMinWidth));
                    
                    if(IsVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsVisible = false;
                        MpMatcherCollectionViewModel.Instance.IsVisible = false;
                    }
                    break;
                case nameof(TagTileViewModels):
                case nameof(RootTagTileViewModels):
                    OnPropertyChanged(nameof(Children));
                    break;
            }
        }

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
            MpHelpers.Instance.RunOnMainThread(() => {
                if (sender is MpTag t) {                    
                    var ttvmToRemove = TagTileViewModels.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (ttvmToRemove != null) {
                        ttvmToRemove.Tag.StartSync(e.SourceGuid);
                        DeleteTagCommand.Execute(t.Id);
                        ttvmToRemove.Tag.EndSync();
                    }
                }
            });
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(() => {
            });
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(async() => {
                if (sender is MpTag t) {
                    t.StartSync(e.SourceGuid);
                    var dupCheck = TagTileViewModels.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (dupCheck == null) {
                        var ttvm = await CreateTagTileViewModel(t);
                        TagTileViewModels.Add(ttvm);
                    } else {
                        MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
                        dupCheck.Tag = t;
                    }
                    t.EndSync();
                }
            });
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ToggleTileIsPinnedCommand => new RelayCommand<object>(
            (args) => {
                var pctvm = args as MpTagTileViewModel;
                pctvm.IsPinned = !pctvm.IsPinned;

                OnPropertyChanged(nameof(PinnedItems));
                pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
            },
            (args) => args != null &&
                      (args is MpTagTileViewModel ||
                       args is List<MpTagTileViewModel>));

        public ICommand DeleteTagCommand => new RelayCommand<object>(
            async (tagId) => {
                //when removing a tag auto-select the history tag

                var ttvm = TagTileViewModels.Where(x => x.TagId == (int)tagId).FirstOrDefault();
                ttvm.ParentTreeItem.Children.Remove(ttvm);

                var ctl = await MpDataModelProvider.Instance.GetChildTagsAsync((int)tagId);
                ctl.ForEach(x => DeleteTagCommand.Execute(x.Id));

                if (!ttvm.Tag.IsSyncing) {
                    await ttvm.Tag.DeleteFromDatabaseAsync();
                }

                OnPropertyChanged(nameof(TagTileViewModels));
                OnPropertyChanged(nameof(RootTagTileViewModels));
                ResetTagSelection();
            },
            (tagId) => {
                //allow delete if any tag besides history tag is selected, delete method will ignore history
                if (tagId == null) {
                    return false;
                }
                var ttvm = TagTileViewModels.Where(x => x.TagId == (int)tagId).FirstOrDefault();
                if(ttvm == null) {
                    return false;
                }
                return !ttvm.IsTagReadOnly;
            });

        public ICommand CreateTagCommand => new RelayCommand(
            async() => {
                //add tag to datastore so TagTile collection will automatically add the tile
                MpTag newTag = new MpTag() {
                    TagName = "Untitled",
                    HexColor = MpHelpers.Instance.GetRandomColor().ToString(),
                    TagSortIdx = TagTileViewModels.Count,
                    ParentTagId = SelectedTagTile.TagId
                };
                await newTag.WriteToDatabaseAsync();
                var ttvm = await CreateTagTileViewModel(newTag);
                ttvm.ParentTreeItem = SelectedTagTile;
                SelectedTagTile.Children.Add(ttvm);

                OnPropertyChanged(nameof(TagTileViewModels));
                OnPropertyChanged(nameof(RootTagTileViewModels));

                await Task.Delay(300);

                SelectTagCommand.Execute(ttvm);

                await Task.Delay(500);

                ttvm.RenameTagCommand.Execute(null);
            });

        public ICommand SelectTagCommand => new RelayCommand<object>(
            (args) => {
                int tagId;
                if(args == null) {
                    tagId = DefaultTagId;
                } else if(args is MpTagTileViewModel ttvm) {
                    tagId = ttvm.TagId;
                } else {
                    tagId = (int)args;
                }

                //if (MpDataModelProvider.Instance.QueryInfo.TagId == tagId) {
                //    return;
                //}

                foreach (var ttvm in TagTileViewModels) {
                    ttvm.IsSelected = ttvm.TagId == tagId;
                }

                if(MpClipTileSortViewModel.Instance.SelectedSortType.SortType == MpContentSortType.Manual) {
                    MpClipTileSortViewModel.Instance.ResetToDefault();
                } else if (MpDataModelProvider.Instance.QueryInfo.TagId != tagId) {
                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged();
                }                
            },
            (args)=>args != null);

        
        #endregion
    }
}
