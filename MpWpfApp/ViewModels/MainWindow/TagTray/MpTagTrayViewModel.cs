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

    public class MpTagTrayViewModel : 
        MpSelectorViewModelBase<object,MpTagTileViewModel>, 
        MpISingletonViewModel<MpTagTrayViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        

        #region View Models

        public ObservableCollection<MpTagTileViewModel> PinnedItems {
            get {
                return new ObservableCollection<MpTagTileViewModel>(Items.Where(x => x.IsPinned));
            }
        }

        public override ObservableCollection<MpTagTileViewModel> Items {
            get {
                if(AllTagViewModel == null) {
                    return new ObservableCollection<MpTagTileViewModel>();
                }
                var ttvml = AllTagViewModel.FindAllChildren().ToList();
                ttvml.Insert(0, AllTagViewModel);
                return new ObservableCollection<MpTagTileViewModel>(ttvml);
            }
        }

        public MpTagTileViewModel SelectedTagTile => Items.FirstOrDefault(x => x.IsSelected);

        public MpTagTileViewModel AllTagViewModel { get; set; }

        public MpTagTileViewModel RecentTagViewModel => Items.FirstOrDefault(tt => tt.Tag.Id == MpTag.RecentTagId);

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

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

        private static MpTagTrayViewModel _instance;
        public static MpTagTrayViewModel Instance => _instance ?? (_instance = new MpTagTrayViewModel());


        public MpTagTrayViewModel() : base(null) {
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }


        public async Task Init() {
            await MpHelpers.RunOnMainThreadAsync(async () => {
                IsBusy = true;

                MpDb.SyncAdd += MpDbObject_SyncAdd;
                MpDb.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.SyncDelete += MpDbObject_SyncDelete;

                MpTag allTag = await MpDb.GetItemAsync<MpTag>(MpTag.AllTagId);

                AllTagViewModel = new MpTagTileViewModel(this);
                await AllTagViewModel.InitializeAsync(allTag);

                OnPropertyChanged(nameof(Items));

                Items.CollectionChanged += TagTileViewModels_CollectionChanged;

                //UpdateSortOrder(true);

                Items.FirstOrDefault(x => x.TagId == DefaultTagId).IsSelected = true;

                await RefreshAllCounts();

                AllTagViewModel.IsExpanded = true;

                //SelectTagCommand.Execute(DefaultTagId);

                IsBusy = false;
            });
        }
        #endregion

        #region Public Methods

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateSortOrder();
        }

        

        public void UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                //Items.Sort(x => x.TagSortIdx);
            } else {
                foreach (var ttvm in Items) {
                    ttvm.TagSortIdx = Items.IndexOf(ttvm);
                }
            }
        }

        public async Task RefreshAllCounts() {
            var countTasks = new Dictionary<int, Task<int>>();
            foreach (var ttvm in Items) {
                if (ttvm.IsAllTag && !MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.GetTotalCopyItemCountAsync());
                } else if (ttvm.IsRecentTag) {
                    //countTasks.Add(ttvm.TagId, MpDataModelProvider.GetRecentCopyItemCountAsync());
                } else {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.GetCopyItemCountForTagAsync(ttvm.TagId));
                }
            }

            await Task.WhenAll(countTasks.Values.ToArray());

            foreach (var ct in countTasks) {
                int count = await ct.Value;
                var ttvm = Items.FirstOrDefault(x => x.TagId == ct.Key);
                if (ttvm != null) {
                    ttvm.TagClipCount = count;
                }
            }           
        }

        public void ClearTagEditing() {
            foreach(var ttvm in Items) {
                ttvm.IsEditing = false;
            }
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            foreach (var tagTile in Items) {
                tagTile.IsSelected = false;
            }
        }

        public void ResetTagSelection() {
            if(SelectedTagTile.TagId != DefaultTagId) {
                ClearTagSelection();
                Items.Where(x => x.TagId == DefaultTagId).FirstOrDefault().IsSelected = true;
            }            
        }

        public async Task UpdateTagAssociation() {
            foreach (var ttvm in Items) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }
                var ciidl = await MpDataModelProvider.GetCopyItemIdsForTagAsync(ttvm.TagId);

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
                        MpActionCollectionViewModel.Instance.IsVisible = false;
                    }
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
            MpHelpers.RunOnMainThread(() => {
                if (sender is MpTag t) {                    
                    var ttvmToRemove = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (ttvmToRemove != null) {
                        ttvmToRemove.Tag.StartSync(e.SourceGuid);
                        DeleteTagCommand.Execute(t.Id);
                        ttvmToRemove.Tag.EndSync();
                    }
                }
            });
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread(() => {
            });
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread(async() => {
                //if (sender is MpTag t) {
                //    t.StartSync(e.SourceGuid);
                //    var dupCheck = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                //    if (dupCheck == null) {
                //        var ttvm = await CreateTagTileViewModel(t);
                //        Items.Add(ttvm);
                //    } else {
                //        MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
                //        dupCheck.Tag = t;
                //    }
                //    t.EndSync();
                //}
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

                var ttvm = Items.Where(x => x.TagId == (int)tagId).FirstOrDefault();
                ttvm.ParentTreeItem.Children.Remove(ttvm);

                var ctl = await MpDataModelProvider.GetChildTagsAsync((int)tagId);
                ctl.ForEach(x => DeleteTagCommand.Execute(x.Id));

                if (!ttvm.Tag.IsSyncing) {
                    await ttvm.Tag.DeleteFromDatabaseAsync();
                }

                OnPropertyChanged(nameof(Items));
                ResetTagSelection();
            },
            (tagId) => {
                //allow delete if any tag besides history tag is selected, delete method will ignore history
                if (tagId == null) {
                    return false;
                }
                var ttvm = Items.Where(x => x.TagId == (int)tagId).FirstOrDefault();
                if(ttvm == null) {
                    return false;
                }
                return !ttvm.IsTagReadOnly;
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

                //if (MpDataModelProvider.QueryInfo.TagId == tagId) {
                //    return;
                //}

                foreach (var ttvm in Items) {
                    ttvm.IsSelected = ttvm.TagId == tagId;
                }

                if(MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual) {
                    MpClipTileSortViewModel.Instance.ResetToDefault();
                } else if (MpDataModelProvider.QueryInfo.TagId != tagId) {
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                }                
            },
            (args)=>args != null);

        
        #endregion
    }
}
