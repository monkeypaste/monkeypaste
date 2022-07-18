using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Diagnostics;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTrayViewModel : 
        MpSelectorViewModelBase<object,MpAvTagTileViewModel>, 
        MpIAsyncSingletonViewModel<MpAvTagTrayViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpISidebarItemViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvTagTileViewModel> PinnedItems {
            get {
                return new ObservableCollection<MpAvTagTileViewModel>(Items.Where(x => x.IsPinned));
            }
        }

        public override ObservableCollection<MpAvTagTileViewModel> Items {
            get {
                if(AllTagViewModel == null) {
                    return new ObservableCollection<MpAvTagTileViewModel>();
                }
                var ttvml = AllTagViewModel.FindAllChildren().ToList();
                ttvml.Insert(0, AllTagViewModel);
                return new ObservableCollection<MpAvTagTileViewModel>(ttvml.Cast<MpAvTagTileViewModel>());
            }
        }

        //public ObservableCollection<MpITreeItemViewModel> TreeItems => new ObservableCollection<MpITreeItemViewModel>(Items.Cast<MpITreeItemViewModel>());

        public MpAvTagTileViewModel SelectedTagTile => Items.FirstOrDefault(x => x.IsSelected);

        public MpAvTagTileViewModel AllTagViewModel { get; set; }

        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;// MpMeasurements.Instance.DefaultTagTreePanelWidth;

        public double DefaultSidebarWidth => 300;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
        public bool IsSidebarVisible { get; set; }

        public MpISidebarItemViewModel NextSidebarItem { get; }
        public MpISidebarItemViewModel PreviousSidebarItem { get; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #region State

        public bool IsNavButtonsVisible => true;// Items.Where(x => x.IsPinned).Sum(x => x.TagTileTrayWidth) > MpMeasurements.Instance.TagTrayDefaultMaxWidth;
        public bool IsEditingTagName {
            get {
                if(SelectedTagTile == null) {
                    return false;
                }
                return SelectedTagTile.IsEditing;
            }
        }

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #region Appearance

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

        private static MpAvTagTrayViewModel _instance;
        public static MpAvTagTrayViewModel Instance => _instance ?? (_instance = new MpAvTagTrayViewModel());


        public MpAvTagTrayViewModel() : base(null) {
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }


        public async Task InitAsync() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                IsBusy = true;

                MpDb.SyncAdd += MpDbObject_SyncAdd;
                MpDb.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.SyncDelete += MpDbObject_SyncDelete;

                MpTag allTag = await MpDb.GetItemAsync<MpTag>(MpTag.AllTagId);

                AllTagViewModel = new MpAvTagTileViewModel(this);
                await AllTagViewModel.InitializeAsync(allTag);

                while(AllTagViewModel.IsBusy) {
                    await Task.Delay(100);
                }

                //OnPropertyChanged(nameof(Items));
                //OnPropertyChanged(nameof(TreeItems));

                Items.CollectionChanged += TagTileViewModels_CollectionChanged;

                //UpdateSortOrder(true);

                Items.FirstOrDefault(x => x.TagId == DefaultTagId).IsSelected = true;

                await RefreshAllCounts();

                AllTagViewModel.IsExpanded = true;

                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(PinnedItems));

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
                if (ttvm.IsAllTag) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.GetTotalCopyItemCountAsync());
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
                ttvm.IsTagNameTrayReadOnly = ttvm.IsTagNameTreeReadOnly = true;
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
            // BUG after tray jump and scroll then resizing tile then resize to default (double click) 
            // there's a collection is modified exception
            // I think this is because of tray's load (since scroll changes from resizing) more and handling persistent selection so wait till tray's done

            while(MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                //await Task.Delay(100);
                return;
            }
            
            foreach (var ttvm in Items) {
                if (ttvm.IsSudoTag || ttvm.IsSelected) {
                    continue;
                }
                var ciidl = await MpDataModelProvider.GetCopyItemIdsForTagAsync(ttvm.TagId);

                ttvm.IsAssociated = MpAvClipTrayViewModel.Instance.SelectedItem != null &&
                                                     ciidl.Contains(MpAvClipTrayViewModel.Instance.SelectedItem.CopyItemId);

            }
        }

        #endregion

        #region Private Methods

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSidebarVisible):
                    if(IsSidebarVisible) {
                        MpAnalyticItemCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpActionCollectionViewModel.Instance.IsSidebarVisible = false;
                        MpClipboardHandlerCollectionViewModel.Instance.IsSidebarVisible = false;
                    }
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
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
            Dispatcher.UIThread.Post(() => {
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
            //Dispatcher.UIThread.Post(() => {
            //});
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
           // Dispatcher.UIThread.Post(async() => {
                //if (sender is MpTag t) {
                //    t.StartSync(e.SourceGuid);
                //    var dupCheck = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                //    if (dupCheck == null) {
                //        var ttvm = await CreateTagTileViewModel(t);
                //        Items.Add(ttvm);
                //    } else {
                //        MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
                //        dupCheck.Tag = t;
                //    }
                //    t.EndSync();
                //}
           // });
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                var pctvm = args as MpAvTagTileViewModel;
                pctvm.IsPinned = !pctvm.IsPinned;

                OnPropertyChanged(nameof(PinnedItems));
                pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
                OnPropertyChanged(nameof(IsNavButtonsVisible));
            },
            (args) => args != null &&
                      (args is MpAvTagTileViewModel ||
                       args is List<MpAvTagTileViewModel>));

        public ICommand DeleteTagCommand => new MpCommand<object>(
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


        public ICommand SelectTagCommand => new MpCommand<object>(
            (args) => {
                int tagId;
                if(args == null) {
                    tagId = DefaultTagId;
                } else if(args is MpAvTagTileViewModel ttvm) {
                    tagId = ttvm.TagId;
                } else if(args is int){
                    tagId = (int)args;
                } else {
                    Debugger.Break();
                    tagId = MpTag.AllTagId;
                }

                Items.ForEach(x => x.IsSelected = x.TagId == tagId);

                OnPropertyChanged(nameof(SelectedTagTile));
                
                if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    // last loaded tag is selected in ClipTray OnPostMainWindowLoaded 
                    // which notifies query changed so don't notify
                    return;
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
