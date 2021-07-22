using FFImageLoading.Forms;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        #endregion

        #region Properties
        public bool IsSelected { get; set; } = false;

        public MpCopyItem CopyItem { get; set; }

        public string StatusText { get; set; } = "Status";

        public bool IsFavorite {
            get {
                if(CopyItem == null) {
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
                if(CopyItem == null) {
                    return null;
                }
                return (StreamImageSource)new MpImageConverter().Convert(CopyItem.App.Icon.IconImage.ImageBase64, typeof(ImageSource));
            }
        }

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpCopyItemTileViewModel() { }

        public MpCopyItemTileViewModel(MpCopyItem item) {
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
            //CopyItem.App
        }
        #region Event Handlers
        private void MpCopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(CopyItem):
                    OnPropertyChanged(nameof(IconImageSource));
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        //Device.InvokeOnMainThreadAsync(async () => {
                        //    await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}");
                        //});
                    }
                    break;
            }
        }

        private void MpDb_OnItemUpdated(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                if(ci.Id == CopyItem.Id) {
                    CopyItem = ci;
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        public ICommand ShowTagAssociationsCommand => new Command(async () => {
            Device.InvokeOnMainThreadAsync(async () => {
                await Shell.Current.GoToAsync($"CopyItemTagAssociations?CopyItemId={CopyItem.Id}");
            });
            //await Navigation.PushModal(new MpCopyItemTagAssociationPageView(new MpCopyItemTagAssociationPageViewModel(CopyItem)));
            //await (Application.Current.MainPage.BindingContext as MpMainShellViewModel).TagCollectionViewModel.FavoritesTagViewModel.Tag.LinkWithCopyItemAsync(CopyItem.Id);
        });

        public ICommand CopyItemTileTappedCommand => new Command(async () => {
            if(IsSelected) {
                Device.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.GoToAsync($"CopyItemdetails?CopyItemId={CopyItem.Id}");
                });
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

