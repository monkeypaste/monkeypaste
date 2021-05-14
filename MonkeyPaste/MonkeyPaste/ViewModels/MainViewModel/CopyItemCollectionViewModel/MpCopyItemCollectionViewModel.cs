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

        #endregion

        #region Properties
        public ObservableCollection<MpCopyItemViewModel> CopyItemViewModels { get; set; } = new ObservableCollection<MpCopyItemViewModel>();

        public MpCopyItemViewModel SelectedCopyItemViewModel { get; set; }
        #endregion

        #region Public Methods
        public MpCopyItemCollectionViewModel() : base() {
            Task.Run(async () => await LoadData());
        }

        public MpCopyItemViewModel CreateCopyItemViewModel(MpCopyItem item) {
            var itemViewModel = new MpCopyItemViewModel(item);
            itemViewModel.ItemStatusChanged += ItemStatusChanged;

            return itemViewModel;
        }
        #endregion

        #region Private Methods
        private void ItemStatusChanged(object sender, EventArgs e) {
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
            await Navigation.PushAsync(itemView);
        }

        private async Task LoadData() {
            var items = await MpDb.Instance.GetItems<MpCopyItem>();
            var itemViewModels = items.Select(i => CreateCopyItemViewModel(i));
            CopyItemViewModels = new ObservableCollection<MpCopyItemViewModel>(itemViewModels);
        }
        #endregion

        #region Commands
        private Command _addNewItemCommand = null;
        public ICommand AddNewItemCommand {
            get {
                if (_addNewItemCommand == null) {
                    _addNewItemCommand = new Command(AddNewItem);
                }
                return _addNewItemCommand;
            }
        }
        private async void AddNewItem() {
            var itemView = MpResolver.Resolve<MpCopyItemView>();
            await Navigation.PushAsync(itemView);
        }

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
                CopyItemText = itemPlainText };
            await MpDb.Instance.AddOrUpdate(newCopyItem);
            //await _repository.AddOrUpdate(newCopyItem);
        }
        #endregion
    }
}
