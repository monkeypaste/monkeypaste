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

        #region State

        public bool IsEditingTagName {
            get {
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
                TagTileViewModels.Add(CreateTagTileViewModel(t));
            }

            TagTileViewModels.CollectionChanged += TagTileViewModels_CollectionChanged;
        }


        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateSortOrder();
        }

        public MpTagTileViewModel CreateTagTileViewModel(MpTag tag) {
            var ttvm = new MpTagTileViewModel(this, tag);
            return ttvm;
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

        public async Task RefreshAllCounts() {
            var countTasks = new Dictionary<int, Task<int>>();
            foreach (var ttvm in TagTileViewModels) {
                if (ttvm.IsAllTag) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetTotalCopyItemCountAsync());
                } else if (ttvm.IsRecentTag) {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetRecentCopyItemCountAsync());
                } else {
                    countTasks.Add(ttvm.TagId, MpDataModelProvider.Instance.GetTagItemCountAsync(ttvm.TagId));
                }
            }

            await Task.WhenAll(countTasks.Values.ToArray());

            foreach (var ct in countTasks) {
                int count = await ct.Value;
                var ttvm = TagTileViewModels.Where(x => x.TagId == ct.Key).FirstOrDefault();
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
                        DeleteTagCommand.Execute(t.Id);
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
                        TagTileViewModels.Add(CreateTagTileViewModel(t));
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

        public ICommand DeleteTagCommand => new RelayCommand<object>(
            (tagId) => {
                //when removing a tag auto-select the history tag

                var ttvm = TagTileViewModels.Where(x => x.TagId == (int)tagId).FirstOrDefault();
                TagTileViewModels.Remove(ttvm);

                if (!ttvm.Tag.IsSyncing) {
                    ttvm.Tag.DeleteFromDatabase();
                }

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
            () => {
                //add tag to datastore so TagTile collection will automatically add the tile
                MpTag newTag = new MpTag() {
                    TagName = "Untitled",
                    HexColor = MpHelpers.Instance.GetRandomColor().ToString(),
                    TagSortIdx = TagTileViewModels.Count
                };
                newTag.WriteToDatabase();
                TagTileViewModels.Add(CreateTagTileViewModel(newTag));
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

                foreach(var ttvm in TagTileViewModels) {
                    ttvm.IsSelected = ttvm.TagId == tagId;
                }

                MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged();
            });
        #endregion
    }
}
