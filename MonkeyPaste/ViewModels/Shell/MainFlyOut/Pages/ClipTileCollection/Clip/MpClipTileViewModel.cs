using FFImageLoading.Forms;
using System;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        #endregion

        #region Properties
        public bool IsSelected { get; set; } = false;

        public MpClip Clip { get; set; }

        public string StatusText { get; set; } = "Status";

        public bool IsFavorite {
            get {
                if(Clip == null) {
                    return false;
                }
                var favTagList = MpDb.Instance.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites").Result;

                if (favTagList != null && favTagList.Count > 0) {
                    var result = MpDb.Instance.QueryAsync<MpClipTag>("select * from MpClipTag where ClipId=? and TagId=?", Clip.Id, favTagList[0].Id).Result;
                    return result != null && result.Count > 0;
                }
                return false;
            }
        }

        public ImageSource IconImageSource {
            get {
                if(Clip == null) {
                    return null;
                }
                return (StreamImageSource)new MpImageConverter().Convert(Clip.App.Icon.IconImage.ImageBytes, typeof(ImageSource));
            }
        }

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpClipTileViewModel() { }

        public MpClipTileViewModel(MpClip item) {
            PropertyChanged += MpClipViewModel_PropertyChanged;
            MpDb.Instance.OnItemUpdated += MpDb_OnItemUpdated;
            Clip = item;
            Routing.RegisterRoute("Clipdetails", typeof(MpClipDetailPageView));
        }
        #endregion

        #region Private Methods

        #region Event Handlers
        private void MpClipViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Clip):
                    OnPropertyChanged(nameof(IconImageSource));
                    break;
                case nameof(IsSelected):
                    if(IsSelected) {
                        //Device.InvokeOnMainThreadAsync(async () => {
                        //    await Shell.Current.GoToAsync($"Clipdetails?ClipId={Clip.Id}");
                        //});
                    }
                    break;
            }
        }

        private void MpDb_OnItemUpdated(object sender, MpDbModelBase e) {
            if(e is MpClip ci) {
                if(ci.Id == Clip.Id) {
                    Clip = ci;
                }
            }
        }
        #endregion

        #endregion

        #region Commands
        public ICommand AddToFavoritesCommand => new Command(async () => {
            await (Application.Current.MainPage.BindingContext as MpMainShellViewModel).TagCollectionViewModel.FavoritesTagViewModel.Tag.LinkWithClipAsync(Clip.Id);
        });

        public ICommand ClipTileTappedCommand => new Command(async () => {
            if(IsSelected) {
                await Shell.Current.GoToAsync($"Clipdetails?ClipId={Clip.Id}");
            }
        });

        public ICommand Save => new Command(async () => {
            await MpDb.Instance.AddOrUpdate<MpClip>(Clip);
            //wait Navigation.PopAsync();
        });

        private Command _setClipboardToItemCommand = null;
        public ICommand SetClipboardToItemCommand {
            get {
                if (_setClipboardToItemCommand == null) {
                    _setClipboardToItemCommand = new Command(SetClipboardToItem);
                }
                return _setClipboardToItemCommand;
            }
        }
        private void SetClipboardToItem() {
            Clipboard.SetTextAsync(Clip.ItemPlainText);
            ItemStatusChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}

