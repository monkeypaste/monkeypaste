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
        public int TagId { get; set; } = 1;
        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; }

        public MpCopyItemViewModel SelectedCopyItemViewModel { get; set; }
        #endregion

        #region Public Methods

        public MpCopyItemCollectionViewModel(MpICopyItemImporter copyItemImporter) : base()
        {
            PropertyChanged += (s,e)=> {
                switch(e.PropertyName) {
                    case nameof(TagId):
                        Device.BeginInvokeOnMainThread(async ()=> { await Initialize(); });
                        break;
                }
            };
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
            var copyItems = await _copyItemImporter.Get(TagId, 0, 20);
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(copyItems.Select(x=>CreateCopyItemViewModel(x)));
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
                Task.Run(async () => await MpDb.Instance.UpdateWithChildren(item.CopyItem));
            }
        }
        #endregion

        #region Commands
        public ICommand DeleteItemCommand => new Command<object>(async (args) => {
            if(args == null || args is not MpCopyItemViewModel civm) {
                return;
            }
            CopyItemViewModels.Remove(civm);
            
            //await MpDb.Instance.DeleteItem(civm.CopyItem);
        });

        public ICommand ItemSelected => new Command(async (selectedCopyItemViewModel) => {
            if(selectedCopyItemViewModel == null) {
                return;
            }
            var scivm = selectedCopyItemViewModel as MpCopyItemViewModel;
            var selectedCopyItemDetailPageView = new MpCopyItemDetailPageView(scivm);
            //await Navigation.NavigateTo(@"copyitem/" + scivm.CopyItem.Id);// PushModal(selectedCopyItemDetailPageView);
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
            await MpCopyItem.AddNewCopyItem(args);
        });
        #endregion
    }
}
