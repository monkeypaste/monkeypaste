using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpTagCollectionViewModel : MpViewModelBase {
        public ObservableCollection<MpTagViewModel> TagViewModels { get; set; }

        public MpTagCollectionViewModel() : base() {
            Task.Run(Initialize);
        }
        public MpTagViewModel CreateTagViewModel(MpTag tag) {
            var tagViewModel = new MpTagViewModel(tag);           
            
            tagViewModel.PropertyChanged += TagViewModel_PropertyChanged;
            return tagViewModel;
        }

        private async Task Initialize() {
            IsBusy = true;
            var tagItems = await MpDb.Instance.GetItems<MpTag>();
            TagViewModels = new ObservableCollection<MpTagViewModel>(tagItems.Select(x=>CreateTagViewModel(x)));
            OnPropertyChanged(nameof(TagViewModels));
            TagViewModels.CollectionChanged += TagViewModels_CollectionChanged;
            await Task.Delay(3000);
            IsBusy = false;
        }

        private void TagViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count > 0) {
                IsBusy = false;
                TagViewModels.CollectionChanged -= TagViewModels_CollectionChanged;
            }
        }

        private void TagViewModel_PropertyChanged(object sender, EventArgs e) {
            if (sender is MpTagViewModel tvm) {
                Task.Run(async () => await MpDb.Instance.UpdateItem(tvm.Tag));
            }
        }
    }
}
