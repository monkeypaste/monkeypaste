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
        public ObservableCollection<MpTagItemViewModel> TagViewModels { get; set; } = new ObservableCollection<MpTagItemViewModel>();
        
        public MpTagItemViewModel SelectedTagViewModel {
            get {
                return TagViewModels.Where(x => x.IsSelected).FirstOrDefault();
            }
        }

        public MpTagCollectionViewModel() : base() {
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            //Device.BeginInvokeOnMainThread(async () => await Initialize());
            Task.Run(Initialize);
        }

        private void Db_OnItemAdded(object sender, MpDbObject e) {
            if(e is MpCopyItem) {

            }
        }

        public MpTagItemViewModel CreateTagViewModel(MpTag tag) {
            var tagViewModel = new MpTagItemViewModel(tag);           
            
            tagViewModel.PropertyChanged += TagViewModel_PropertyChanged;
            return tagViewModel;
        }

        private async Task Initialize() {
            IsBusy = true;
            var tagItems = await MpTag.GetAllTags();
            TagViewModels = new ObservableCollection<MpTagItemViewModel>(tagItems.Select(x=>CreateTagViewModel(x)));
            OnPropertyChanged(nameof(TagViewModels));
            await Task.Delay(3000);
            IsBusy = false;
        }

        private void TagViewModel_PropertyChanged(object sender, EventArgs e) {
            if (sender is MpTagItemViewModel tvm) {
                Task.Run(async () => await MpDb.Instance.UpdateWithChildren(tvm.Tag));
            }
        }

        public ICommand DeleteTagCommand => new Command<object>(async (args) => {
            MpConsole.WriteLine("Delete Tag"+(args as MpTagItemViewModel).Tag.TagName);
        });
    }
}
