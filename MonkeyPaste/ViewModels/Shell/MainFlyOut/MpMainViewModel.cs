
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {  

    public class MpMainViewModel : MpViewModelBase {

        //public ObservableCollection<Photo> Recent { get; set; }
        //public ObservableCollection<Photo> Favorites { get; set; }

        #region Private Variables

        //private readonly MpIPhotoImporter photoImporter;
        //private readonly MpILocalStorage localStorage;
        #endregion

        #region Properties
        public MpCopyItemCollectionViewModel CopyItemCollectionViewModel { get; set; }// = new MpCopyItemCollectionViewModel();
        #endregion

        #region Public Methods
        public MpMainViewModel() : base() {
            CopyItemCollectionViewModel = new MpCopyItemCollectionViewModel(1);
        }

        //public MpMainViewModel(MpIPhotoImporter photoImporter, MpILocalStorage localStorage)
        //{
        //    this.photoImporter = photoImporter;
        //    this.localStorage = localStorage;
        //}

        //public async Task Initialize()
        //{
        //    var photos = await photoImporter.Get(0, 20, Quality.Low);
        //    Recent = photos;
        //    RaisePropertyChanged(nameof(Recent));
        //    await LoadFavorites();
        //    MessagingCenter.Subscribe<MpGalleryViewModel>(this, "FavoritesAdded", (sender) =>
        //    {
        //        MainThread.BeginInvokeOnMainThread(async () =>
        //        {
        //            await LoadFavorites();
        //        });
        //    });
        //}
        #endregion

        #region Private Methods
        //private async Task LoadFavorites()
        //{
        //    var filenames = await localStorage.Get();
        //    var favorites = await photoImporter.Get(filenames, Quality.Low);
        //    Favorites = favorites;
        //    RaisePropertyChanged(nameof(Favorites));
        //}
        #endregion

        #region Commands

        #endregion
    }
}
