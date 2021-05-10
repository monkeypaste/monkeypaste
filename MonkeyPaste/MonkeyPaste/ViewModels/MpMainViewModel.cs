using MonkeyPaste.Models;
using MonkeyPaste.Repositories;
using MonkeyPaste.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste.ViewModels {
    public class MpMainViewModel : Base.MpViewModelBase {
        private readonly MpCopyItemRepository _repository;

        public ObservableCollection<MpCopyItemViewModel> Items { get; set; }        

        public MpMainViewModel(MpCopyItemRepository repository) {
            _repository = repository;

            _repository.OnItemAdded += (sender, item) => Items.Add(CreateCopyItemViewModel(item));
            _repository.OnItemUpdated += (sender, item) => Task.Run(async () => await LoadData());

            Task.Run(async () => await LoadData());
        }

        public void AddSharedText(string newText) {
            Console.WriteLine(@"Shared Text: " + newText);
        }
        public MpCopyItemViewModel SelectedItem {
            get { 
                return null; 
            }
            set {
                Device.BeginInvokeOnMainThread(async () => await
                NavigateToItem(value));
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        private async Task NavigateToItem(MpCopyItemViewModel item) {
            if (item == null) {
                return;
            }
            var itemView = MpResolver.Resolve<MpItemView>();
            var vm = itemView.BindingContext as MpItemViewModel;
            vm.Item = item.CopyItem;
            await Navigation.PushAsync(itemView);
        }        

        private async Task LoadData() {
            var items = await _repository.GetItems();
            if (!ShowAll) {
                items = items.Where(x => Clipboard.GetTextAsync().Result == x.ItemPlainText).ToList();
            }
                
            var itemViewModels = items.Select(i => CreateCopyItemViewModel(i));
            Items = new ObservableCollection<MpCopyItemViewModel>(itemViewModels);            
        }

        private MpCopyItemViewModel CreateCopyItemViewModel(MpCopyItem item) {
            var itemViewModel = new MpCopyItemViewModel(item);
            itemViewModel.ItemStatusChanged += ItemStatusChanged;

            return itemViewModel;
        }

        private void ItemStatusChanged(object sender, EventArgs e) {
            if (sender is MpCopyItemViewModel item) {
                if (!ShowAll && 
                    Clipboard.GetTextAsync().Result == item.CopyItem.ItemPlainText) {
                    Items.Remove(item);
                }
                Task.Run(async () => await _repository.UpdateItem(item.CopyItem));
            }
        }
        public bool ShowAll { get; set; }

              

        private Command _addNewItemCommand = null;
        public ICommand AddNewItemCommand {
            get {
                if(_addNewItemCommand == null) {
                    _addNewItemCommand = new Command(AddNewItem);
                }
                return _addNewItemCommand;
            }
        }
        private async void AddNewItem() {
            var itemView = MpResolver.Resolve<MpItemView>();
            await Navigation.PushAsync(itemView);
        }

        private Command<string> _addItemFromClipboardCommand = null;
        public ICommand AddItemFromClipboardCommand {
            get {
                if (_addItemFromClipboardCommand == null) {
                    _addItemFromClipboardCommand = new Command<string>(AddItemFromClipboard);
                }
                return _addItemFromClipboardCommand;
            }
        }
        private async void AddItemFromClipboard(string itemPlainText) {
            var newCopyItem = new MpCopyItem() { CopyDateTime = DateTime.Now, Title = "Text", ItemPlainText = itemPlainText };
            await _repository.AddOrUpdate(newCopyItem);
        }

        //public ICommand AddItem => new Command(async () => {
        //    var itemView = MpResolver.Resolve<MpItemView>();
        //    await Navigation.PushAsync(itemView);
        //});


    }
}
