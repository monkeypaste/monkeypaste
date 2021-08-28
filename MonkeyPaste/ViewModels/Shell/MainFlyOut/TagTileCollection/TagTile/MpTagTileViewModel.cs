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
                if (Tag == null || Tag.CopyItemList == null) {
                    return 0;
                }
                return Tag.CopyItemList.Count;
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

        #region Event Handlers
        private void MpTagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            Device.InvokeOnMainThreadAsync(async () => {
                switch (e.PropertyName) {
                    case nameof(IsNameReadOnly):
                        if(IsNameReadOnly && (Tag.TagName != _orgTagName || Tag.Id == 0)) {
                            _orgTagName = Tag.TagName;
                            await MpDb.Instance.AddOrUpdateAsync<MpTag>(Tag);
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
                        var nci = await MpCopyItem.GetCopyItemByIdAsync(ncit.CopyItemId);
                        if (!Tag.CopyItemList.Contains(nci)) {
                            Tag.CopyItemList.Add(nci);

                            OnPropertyChanged(nameof(CopyItemCount));
                        }
                    }
                } else if (e is MpCopyItem nci) {
                    //occurs for new/synced copy items
                    bool isLinked = await Tag.IsLinkedWithCopyItemAsync(nci);
                    if(!isLinked && (Tag.Id == MpTag.RecentTagId || Tag.Id == MpTag.AllTagId)) {
                        isLinked = true;
                    }
                    if (isLinked && !Tag.CopyItemList.Any(x=>x.CopyItemGuid == nci.CopyItemGuid)) {
                        Tag.CopyItemList.Add(nci);
                        OnPropertyChanged(nameof(CopyItemCount));
                    }
                }
            });
        }
        

        private void Db_OnItemUpdated(object sender, MpDbModelBase e) {
            Device.InvokeOnMainThreadAsync(async () => {
                if (e is MpTag t) {
                    if (t.Guid == Tag.Guid) {
                        Tag = t;
                    }
                } 
            });
        }

        private void Db_OnItemDeleted(object sender, MpDbModelBase e) {
            Device.BeginInvokeOnMainThread(() => {
                if (e is MpCopyItemTag dcit) {
                    if (dcit.TagId == Tag.Id) {
                        //when CopyItem unlinked
                        var ci = Tag.CopyItemList.Where(x => x.Id == dcit.CopyItemId).FirstOrDefault();
                        if (ci != null) {
                            Tag.CopyItemList.Remove(ci);
                            OnPropertyChanged(nameof(CopyItemCount));
                        }
                    }
                } else if (e is MpCopyItem dci) {
                    //when copy item deleted
                    if (Tag.CopyItemList.Any(x => x.Id == dci.Id)) {
                        Tag.CopyItemList.Remove(dci);
                        OnPropertyChanged(nameof(CopyItemCount));
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
            await MpDb.Instance.AddOrUpdateAsync<MpTag>(Tag);
            OnPropertyChanged(nameof(Tag));
        });

        public ICommand DeleteTagCommand => new Command(
            async () => {
                await MpDb.Instance.DeleteItemAsync<MpTag>(Tag);
            },
            () => {
                return Tag.Id > 4;
            });
        #endregion
    }
}
