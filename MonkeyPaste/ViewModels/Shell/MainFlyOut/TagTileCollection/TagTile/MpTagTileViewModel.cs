using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Private Variables
        private string _orgTagName = string.Empty;
        #endregion

        #region Properties

        #region View Models
        public MpContextMenuViewModel ContextMenuViewModel { get; set; }
        #endregion

        #region Model
        public MpTag Tag { get; set; }
        #endregion

        #region State
        public bool IsSelected { get; set; } = false;

        public int CopyItemCount {
            get {
                if (Tag == null || Tag.CopyItems == null) {
                    return 0;
                }
                return Tag.CopyItems.Count;
            }
        }              

        public bool IsNameReadOnly { get; set; } = true;
        #endregion

        #region Drag & Drop
        public bool IsBeingDraggedOver { get; set; } = false;

        public bool IsBeingDragged { get; set; } = false;
        #endregion

        #region Business Logic
        public bool IsUserTag {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.Id > 4;
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTagTileViewModel() : this(null) { }

        public MpTagTileViewModel(MpTag tag) {
            PropertyChanged += MpTagViewModel_PropertyChanged;
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            Tag = tag;

            //CopyItemCollectionViewModel = new MpCopyItemCollectionViewModel(Tag.Id);
            Task.Run(Initialize);
        }

        #endregion

        #region Private Methods
        private async Task Initialize() {
            //Tag.CopyItemList = await MpCopyItem.GetAllCopyItemsByTagId(Tag.Id);
            //Tag.Color = await MpColor.GetColorByIdAsync(Tag.ColorId);
            await Task.Delay(10);
            ContextMenuViewModel = new MpContextMenuViewModel();
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Rename",
                Command = RenameTagCommand,
                IconImageResourceName = "EditIcon"
            });

            //ContextMenuViewModel.Items.Add(new MpColorChooserContextMenuItemViewModel());
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Change Color",
                Command = ChangeColorCommand,
                IconImageResourceName = "ColorIcon"
            });
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Delete",
                Command = DeleteTagCommand,
                IconImageResourceName = "DeleteIcon"
            });

            OnViewModelLoaded();
        }

        private async Task UpdateSortOrder(bool fromModel = false) {
            if(fromModel) {
                var citl = await MpDataModelProvider.Instance.GetCopyItemTagsForTagAsync(Tag.Id);
                citl = citl.OrderBy(x => x.CopyItemSortIdx).ToList();
                for (int i = 0; i < citl.Count; i++) {
                    citl[i].CopyItemSortIdx = i;
                    await citl[i].WriteToDatabaseAsync();
                }
            } else {
                // TODO add logic to update copy items by order in collection
            }
        }

        #region Event Handlers
        private void MpTagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Device.InvokeOnMainThreadAsync(async () => {
                switch (e.PropertyName) {
                    case nameof(IsNameReadOnly):
                        if(IsNameReadOnly && (Tag.TagName != _orgTagName || Tag.Id == 0)) {
                            _orgTagName = Tag.TagName;
                            await Tag.WriteToDatabaseAsync();
                        }
                        break;
                }
            });
        }

        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            Device.InvokeOnMainThreadAsync(async () => {
                if (e is MpCopyItemTag ncit) {
                    if (ncit.TagId == Tag.Id) {
                        //occurs when copy item is linked to tag
                        if(ncit.CopyItemSortIdx < 0) {
                            ncit.CopyItemSortIdx = Tag.CopyItems.Count;
                            await ncit.WriteToDatabaseAsync();
                        } else {
                            var nci = await MpDb.Instance.GetItemAsync<MpCopyItem>(ncit.CopyItemId);
                            if(!Tag.CopyItems.Contains(nci)) {
                                Tag.CopyItems.Add(nci);
                            }
                            await UpdateSortOrder(true);
                        }

                        OnPropertyChanged(nameof(CopyItemCount));
                    }
                } else if (e is MpCopyItem nci) {
                    //occurs for new/synced copy items
                    bool isLinked = await MpDataModelProvider.Instance.IsTagLinkedWithCopyItem(Tag.Id, nci.Id);
                    if (!isLinked) {
                        isLinked = Tag.Id == MpTag.RecentTagId || Tag.Id == MpTag.AllTagId;
                    } 
                    if (isLinked) {// && !Tag.CopyItemList.Any(x => x.CopyItemGuid == nci.CopyItemGuid)) {
                        Tag.CopyItems.Add(nci);
                        OnPropertyChanged(nameof(CopyItemCount));
                    }
                }
            });
        }
        

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            Device.InvokeOnMainThreadAsync(async () => {
                if (e is MpTag t) {
                    if (t.Id == Tag.Id) {
                        Tag = t;
                    }
                } else if (e is MpCopyItemTag dcit) {
                    if (dcit.TagId == Tag.Id) {
                        await UpdateSortOrder(true);
                    }
                }
            });
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            Device.BeginInvokeOnMainThread(async () => {
                if (e is MpCopyItemTag dcit) {
                    if (dcit.TagId == Tag.Id) {
                        //when CopyItem unlinked
                        var ci = Tag.CopyItems.Where(x => x.Id == dcit.CopyItemId).FirstOrDefault();
                        if (ci != null) {
                            Tag.CopyItems.Remove(ci);
                            OnPropertyChanged(nameof(CopyItemCount));
                        }
                        await UpdateSortOrder(true);
                    }
                } else if (e is MpCopyItem dci) {
                    //when copy item deleted
                    if (Tag.CopyItems.Any(x => x.Id == dci.Id)) {
                        Tag.CopyItems.Remove(dci);
                        OnPropertyChanged(nameof(CopyItemCount));
                        await UpdateSortOrder(true);
                    }
                }
            });
        }
        #endregion

        #endregion        

        #region Commands
        public ICommand RenameTagCommand => new Command(
            () => {
                _orgTagName = Tag.TagName;
                IsNameReadOnly = false;
            },
            () => {
                return Tag.Id > 4;
            });

        public ICommand ChangeColorCommand => new Command(async () => {
            Tag.HexColor = MpHelpers.Instance.GetRandomColor().ToHex();
            await Tag.WriteToDatabaseAsync();
            OnPropertyChanged(nameof(Tag));
        });

        public ICommand DeleteTagCommand => new Command(
            async () => {
                await Tag.DeleteFromDatabaseAsync();
            },
            () => {
                return Tag.Id > 4;
            });
        #endregion
    }
}
