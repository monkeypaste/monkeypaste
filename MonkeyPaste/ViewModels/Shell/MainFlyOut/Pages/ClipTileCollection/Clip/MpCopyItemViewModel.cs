using FFImageLoading.Forms;
using FFImageLoading.Helpers.Exif;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        private string _orgTitle = string.Empty;
        #endregion

        #region Properties

        #region View Models
        public MpContextMenuViewModel ContextMenuViewModel { get; set; }
        #endregion

        public bool IsSelected { get; set; } = false;

        public MpCopyItem CopyItem { get; set; }

        public string StatusText { get; set; } = "Status";

        public bool IsFavorite {
            get {
                if (CopyItem == null) {
                    return false;
                }
                var favTagList = MpDb.Instance.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites").Result;

                if (favTagList != null && favTagList.Count > 0) {
                    var result = MpDb.Instance.QueryAsync<MpCopyItemTag>("select * from MpCopyItemTag where CopyItemId=? and TagId=?", CopyItem.Id, favTagList[0].Id).Result;
                    return result != null && result.Count > 0;
                }
                return false;
            }
        }

        public ImageSource IconImageSource {
            get {
                if (CopyItem == null) {
                    return null;
                }
                return (StreamImageSource)new MpImageConverter().Convert(CopyItem.App.Icon.IconImage.ImageBase64, typeof(ImageSource));
            }
        }

        public bool IsTitleReadOnly { get; set; } = true;

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpCopyItemViewModel() { }

        public MpCopyItemViewModel(MpCopyItem item) {
            PropertyChanged += MpCopyItemViewModel_PropertyChanged;
            MpDb.Instance.OnItemUpdated += MpDb_OnItemUpdated;
            CopyItem = item;
            Routing.RegisterRoute("CopyItemdetails", typeof(MpCopyItemDetailPageView));
            Routing.RegisterRoute("CopyItemTagAssociations", typeof(MpCopyItemTagAssociationPageView));

            Task.Run(Initialize);
        }


        #endregion

        #region Private Methods
        private async Task Initialize() {
            ContextMenuViewModel = new MpContextMenuViewModel();
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Change Tags",
                Command = ShowTagAssociationsCommand,
                IconImageResourceName = "StarOutlineIcon"
            });
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Rename",
                Command = RenameCopyItemCommand,
                IconImageResourceName = "EditIcon"
            });
            ContextMenuViewModel.Items.Add(new MpContextMenuItemViewModel() {
                Title = "Delete",
                Command = DeleteCopyItemCommand,
                IconImageResourceName = "DeleteIcon"
            });
        }
        #region Event Handlers
        private void MpCopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CopyItem):
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        //Device.InvokeOnMainThreadAsync(async () => {
                        //    await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}");
                        //});
                    }
                    break;
                case nameof(IsTitleReadOnly):
                    if (IsTitleReadOnly && CopyItem.Title != _orgTitle) {
                        _orgTitle = CopyItem.Title;
                        MpDb.Instance.AddOrUpdate<MpCopyItem>(CopyItem);
                    }
                    break;
            }
        }

        private void MpDb_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (ci.Id == CopyItem.Id) {
                    CopyItem = ci;
                    //await Initialize();
                    //OnPropertyChanged(nameof(IconImageSource));
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        public ICommand DeleteCopyItemCommand => new Command(
            async () => {
                await MpDb.Instance.DeleteItemAsync<MpCopyItem>(CopyItem);
                await MpCopyItemTag.DeleteAllCopyItemTagsForCopyItemId(CopyItem.Id);
            });

        public ICommand ShowTagAssociationsCommand => new Command(async () => {
            await Device.InvokeOnMainThreadAsync(async () => {
                await Shell.Current.GoToAsync($"CopyItemTagAssociations?CopyItemId={CopyItem.Id}");
            });
            //await Navigation.PushModal(new MpCopyItemTagAssociationPageView(new MpCopyItemTagAssociationPageViewModel(CopyItem)));
            //await (Application.Current.MainPage.BindingContext as MpMainShellViewModel).TagCollectionViewModel.FavoritesTagViewModel.Tag.LinkWithCopyItemAsync(CopyItem.Id);
        });

        public ICommand RenameCopyItemCommand => new Command(async () => {
            _orgTitle = CopyItem.Title;

            var renamePopupPage = new MpRenamePopupPageView(_orgTitle);
            renamePopupPage.OnComplete += async (s, e) => {
                if (renamePopupPage.WasCanceled) {
                    CopyItem.Title = _orgTitle;
                } else if (e != _orgTitle) {
                    CopyItem.Title = e;
                    OnPropertyChanged(nameof(CopyItem));
                    MpDb.Instance.UpdateItem<MpCopyItem>(CopyItem);
                }
                await PopupNavigation.Instance.PopAllAsync();
            };
            await PopupNavigation.Instance.PushAsync(renamePopupPage,false);
            
        });

        public ICommand CopyItemTileTappedCommand => new Command(async () => {
            if(IsSelected) {
                await Device.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}");
                    return;
                });
            } else {
                IsSelected = true;
            }
        });

        public ICommand Save => new Command(async () => {
            await MpDb.Instance.AddOrUpdateAsync<MpCopyItem>(CopyItem);
            //wait Navigation.PopAsync();
        });

        private Command _setCopyItemboardToItemCommand = null;
        public ICommand SetCopyItemboardToItemCommand {
            get {
                if (_setCopyItemboardToItemCommand == null) {
                    _setCopyItemboardToItemCommand = new Command(SetCopyItemboardToItem);
                }
                return _setCopyItemboardToItemCommand;
            }
        }
        private void SetCopyItemboardToItem() {
            Clipboard.SetTextAsync(CopyItem.ItemText);
            ItemStatusChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}

