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
        private int itemsAdded;
        private int currentStartIndex = 0;
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
            return itemViewModel;
        }
        #endregion

        #region Private Methods
        private async Task Initialize()
        {
            IsBusy = true;
            var copyItems = await _copyItemImporter.Get(1, 0, 20);
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(copyItems.Select(x => CreateCopyItemViewModel(x)).ToList());
            OnPropertyChanged(nameof(CopyItemViewModels));
            CopyItemViewModels.CollectionChanged += CopyItemViewModels_CollectionChanged;
            await Task.Delay(3000);
            IsBusy = false;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            foreach (MpCopyItem copyItem in args.NewItems)
            {
                itemsAdded++;
                CopyItemViewModels.Add(CreateCopyItemViewModel(copyItem));
            }
            if (itemsAdded == 20)
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

        private async Task NavigateToItem(MpCopyItemViewModel item) {
            if (item == null) {
                return;
            }
            var itemView = MpResolver.Resolve<MpCopyItemView>();
            var vm = itemView.BindingContext as MpCopyItemViewModel;
            vm.CopyItem = item.CopyItem;
            //await Navigation.PushAsync(itemView);
        }

        private async Task LoadData() {
            var items = await MpDb.Instance.GetItems<MpCopyItem>();
            var itemViewModels = items.Select(i => CreateCopyItemViewModel(i));
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(itemViewModels);            
            MpConsole.Instance.WriteLine(@"CopyItems Loaded: " + CopyItemViewModels.Count);
        }
        #endregion

        #region Commands


        public ICommand LoadMore => new Command(async () =>
        {
            currentStartIndex += 20;
            itemsAdded = 0;
            var collection = await _copyItemImporter.Get(1, currentStartIndex, 20);
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

        private Command<object> _addItemFromClipboardCommand = null;
        public ICommand AddItemFromClipboardCommand {
            get {
                if (_addItemFromClipboardCommand == null) {
                    _addItemFromClipboardCommand = new Command<object>(AddItemFromClipboard);
                }
                return _addItemFromClipboardCommand;
            }
        }
        private async void AddItemFromClipboard(object args) {
            if(args == null) {
                return;
            }
            string sourceHostInfo = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var newCopyItem = new MpCopyItem() { 
                CopyDateTime = DateTime.Now, 
                Title = "Text", 
                ItemText = itemPlainText,
                Host = sourceHostInfo
            };
            await MpDb.Instance.AddOrUpdate(newCopyItem);
            //await _repository.AddOrUpdate(newCopyItem);
        }
        #endregion
    }
}
