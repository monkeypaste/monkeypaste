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
    public class MpCopyItemCollectionViewModel : MpViewModelBase {
        #region Private Variables
        private readonly MpICopyItemImporter _copyItemImporter;
        private int _itemsAdded = 0;
        private int _currentStartIndex = 0;
        private int _pageSize = 20;
        #endregion

        #region Properties
        //public ObservableCollection<MpCopyItem> CopyItems { get; set; }

        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; }

        public MpCopyItemViewModel SelectedCopyItemViewModel { get; set; }
        #endregion

        #region Public Methods
        //public MpCopyItemCollectionViewModel() : base() {
        //    Task.Run(async () => await LoadData());
        //}

        public MpCopyItemCollectionViewModel(MpICopyItemImporter copyItemImporter) : base()
        {
            _copyItemImporter = copyItemImporter;
            Task.Run(Initialize);
        }

        public MpCopyItemViewModel CreateCopyItemViewModel(MpCopyItem item) {
            var itemViewModel = new MpCopyItemViewModel(item);
            itemViewModel.PropertyChanged += CopyItemViewModels_PropertyChanged;
            Routing.RegisterRoute(@"copyitem/" + itemViewModel,typeof(MpCopyItemDetailPageView));
            return itemViewModel;
        }
        #endregion

        #region Private Methods
        private async Task Initialize()
        {
            IsBusy = true;
            var copyItems = await _copyItemImporter.Get(1, 0, 20);
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>();
            foreach (var ci in copyItems) {
                CopyItemViewModels.Add(CreateCopyItemViewModel(ci));
            }
            OnPropertyChanged(nameof(CopyItemViewModels));
            CopyItemViewModels.CollectionChanged += CopyItemViewModels_CollectionChanged;
            await Task.Delay(3000);
            IsBusy = false;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            foreach (MpCopyItem copyItem in args.NewItems)
            {
                _itemsAdded++;
                CopyItemViewModels.Add(CreateCopyItemViewModel(copyItem));
            }
            if (_itemsAdded == 20)
            {
                var collection = (ObservableCollection<MpCopyItem>)sender;
                collection.CollectionChanged -= Collection_CollectionChanged;
            }
        }


        private void CopyItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                IsBusy = false;
                CopyItemViewModels.CollectionChanged -= CopyItemViewModels_CollectionChanged;
            }
        }

        private void CopyItemViewModels_PropertyChanged(object sender, EventArgs e) {
            if (sender is MpCopyItemViewModel item) {
                Task.Run(async () => await MpDb.Instance.UpdateItem(item.CopyItem));
            }
        }
        #endregion

        #region Commands
        public ICommand DeleteItemCommand => new Command<object>(async (args) => {
            if(args == null || args is not MpCopyItemViewModel civm) {
                return;
            }
            CopyItemViewModels.Remove(civm);
            await MpDb.Instance.DeleteItem(civm.CopyItem);
        });

        public ICommand ItemSelected => new Command(async (selectedCopyItemViewModel) => {
            if(selectedCopyItemViewModel == null) {
                return;
            }
            var scivm = selectedCopyItemViewModel as MpCopyItemViewModel;
            var selectedCopyItemDetailPageView = new MpCopyItemDetailPageView(scivm);
            await Navigation.NavigateTo(@"copyitem/" + scivm.CopyItem.Id);// PushModal(selectedCopyItemDetailPageView);
        });

        public ICommand LoadMore => new Command(async () =>
        {
            _currentStartIndex += 20;
            _itemsAdded = 0;
            var collection = await _copyItemImporter.Get(1, _currentStartIndex, 20);
            collection.CollectionChanged += Collection_CollectionChanged;
        });

        public ICommand AddFavorites => new Command<List<MpCopyItem>>((copyItems) =>
        {
            foreach (var copyItem in copyItems)
            {
                //localStorage.Store(photo.Filename);
            }
            MessagingCenter.Send(this, "FavoritesAdded");
        });

        public ICommand AddItemFromClipboardCommand => new Command<object>(async (args) => {
            if (args == null) {
                return;
            }
            //create CopyItem
            string hostPackageName = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var hostAppName = (args as object[])[2] as string;
            var hostAppImage = (args as object[])[3] as byte[];

            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Text",
                ItemText = itemPlainText,
                Host = hostPackageName,
                ItemImage = hostAppImage
            };

            await MpDb.Instance.AddOrUpdate(newCopyItem);

            //add copyitem to default tags
            var defaultTagList = await MpDb.Instance.Query<MpTag>("select * from MpTag where TagName=? or TagName=?", "All", "Recent");

            if (defaultTagList != null) {
                foreach (var tag in defaultTagList) {
                    var copyItemTag = new MpCopyItemTag() {
                        CopyItemId = newCopyItem.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItem<MpCopyItemTag>(copyItemTag);
                }
            }


            if(!string.IsNullOrEmpty(hostPackageName)) {
                //add or update copyitem's source app
                var appFromHostList = await MpDb.Instance.Query<MpApp>("select * from MpApp where AppPath=?", hostPackageName);
                if (appFromHostList != null && appFromHostList.Count >= 1) {
                    var app = appFromHostList[0];

                    newCopyItem.AppId = app.Id;
                    await MpDb.Instance.UpdateItem<MpCopyItem>(newCopyItem);
                } else {
                    var newIcon = new MpIcon() {
                       IconImage = hostAppImage 
                    };
                    await MpDb.Instance.AddItem<MpIcon>(newIcon);

                    var newApp = new MpApp() {
                        AppPath = hostPackageName,
                        AppName = hostAppName,
                        IconId = newIcon.Id
                    };

                    await MpDb.Instance.AddItem<MpApp>(newApp);

                    newCopyItem.AppId = newApp.Id;
                    await MpDb.Instance.UpdateItem<MpCopyItem>(newCopyItem);
                }
            }
        });
        #endregion
    }
}
