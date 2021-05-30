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
        private int _itemsAdded = 0;
        private int _currentStartIndex = 0;
        private int _pageSize = 20;
        #endregion

        #region Properties
        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; }

        public MpCopyItemViewModel SelectedCopyItemViewModel { get; set; }
        #endregion

        #region Public Methods
        public MpCopyItemCollectionViewModel() : this(MpTag.RecentTag.Id) { }

        public MpCopyItemCollectionViewModel(int tagId) : base() {
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Db_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Db_OnItemDeleted;

            SetTag(tagId);
        }

        public async Task SetTag(int tagId)
        {
           await Device.InvokeOnMainThreadAsync(async () => await Initialize(tagId));
        }

        public MpCopyItemViewModel CreateCopyItemViewModel(MpCopyItem item) {
            var itemViewModel = new MpCopyItemViewModel(item);
            itemViewModel.PropertyChanged += CopyItemViewModel_PropertyChanged;
            Routing.RegisterRoute(@"copyitem/" + itemViewModel, typeof(MpCopyItemDetailPageView));
            return itemViewModel;
        }

        public void ClearSelection() {
            foreach (var civm in CopyItemViewModels) {
                civm.IsSelected = false;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize(int tagId) {
            IsBusy = true;
            await MpDb.Instance.Init();
            var copyItems = await MpDb.Instance.Get(tagId, 0, _pageSize);
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(copyItems.Select(x => CreateCopyItemViewModel(x)));
            CopyItemViewModels.CollectionChanged += CopyItemViewModels_CollectionChanged;
            OnPropertyChanged(nameof(CopyItemViewModels));
            await Task.Delay(300);
            IsBusy = false;
        }
        #endregion

        #region Event Handlers
        //private async Task OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //    switch (e.PropertyName) {

        //    }
        //}
        //private async void MpCopyItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //    //await OnPropertyChanged(sender, e);
        //}        

        private void Db_OnItemDeleted(object sender, MpDbObject e) {
            //throw new NotImplementedException();
        }

        private void Db_OnItemUpdated(object sender, MpDbObject e) {
            //throw new NotImplementedException();
        }

        private void Db_OnItemAdded(object sender, MpDbObject e) {
            if (e is MpCopyItem nci) {
                CopyItemViewModels.Add(CreateCopyItemViewModel(nci));
            }
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) {
            foreach (MpCopyItem copyItem in args.NewItems) {
                _itemsAdded++;
                CopyItemViewModels.Add(CreateCopyItemViewModel(copyItem));
            }
            if (_itemsAdded == _pageSize) {
                var collection = (ObservableCollection<MpCopyItem>)sender;
                collection.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void CopyItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count > 0) {
                IsBusy = false;
                CopyItemViewModels.CollectionChanged -= CopyItemViewModels_CollectionChanged;
            }
        }

        private async void CopyItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpCopyItemViewModel civm)
            {
                switch (e.PropertyName)
                {
                    case nameof(civm.IsSelected):
                        if (civm.IsSelected)
                        {
                            if (SelectedCopyItemViewModel == civm)
                            {
                                //implies selection came from ui do nothing
                            }
                            else
                            {
                                if (SelectedCopyItemViewModel != null)
                                {
                                    SelectedCopyItemViewModel.IsSelected = false;
                                }
                                SelectedCopyItemViewModel = civm;
                            }
                        }
                        else
                        {
                            if (SelectedCopyItemViewModel == civm)
                            {
                                SelectedCopyItemViewModel = null;
                            }
                        }
                        break;
                }

                await MpDb.Instance.UpdateItem<MpCopyItem>(civm.CopyItem);
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
            await MpCopyItemTag.DeleteAllCopyItemTagsForCopyItemId(civm.CopyItem.Id);
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
            _currentStartIndex += _pageSize;
            _itemsAdded = 0;
            var collection = await MpDb.Instance.Get(1, _currentStartIndex, _pageSize);
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

        #endregion
    }
}
