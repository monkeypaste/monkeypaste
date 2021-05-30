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
        #region Properties
        public MpCopyItemCollectionViewModel CopyItemCollectionViewModel { get; set; }

        public ObservableCollection<MpTagItemViewModel> TagViewModels { get; set; } = new ObservableCollection<MpTagItemViewModel>();
        
        public MpTagItemViewModel SelectedTagViewModel { get; set; }

        public MpTagItemViewModel RecentTagViewModel
        {
            get
            {
                return TagViewModels.Where(x => x.Tag.Id == MpTag.RecentTag.Id).FirstOrDefault();
            }
        }
        #endregion

        #region Public Methods
        public MpTagCollectionViewModel() : base() {
            CopyItemCollectionViewModel = new MpCopyItemCollectionViewModel();
            PropertyChanged += MpTagCollectionViewModel_PropertyChanged;
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            Task.Run(Initialize);
        }       

        public MpTagItemViewModel CreateTagViewModel(MpTag tag) {
            var tagViewModel = new MpTagItemViewModel(tag);
            tagViewModel.PropertyChanged += TagViewModel_PropertyChanged1;
            return tagViewModel;
        }        

        public void ClearSelection() {
            SelectedTagViewModel = null;
            foreach(var tivm in TagViewModels) {
                tivm.IsSelected = false;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            IsBusy = true;
            await MpDb.Instance.Init();
            var tagItems = await MpTag.GetAllTags();
            TagViewModels = new ObservableCollection<MpTagItemViewModel>(tagItems.Select(x => CreateTagViewModel(x)));
            OnPropertyChanged(nameof(TagViewModels));
            SelectedTagViewModel = RecentTagViewModel;
            await Task.Delay(300);
            IsBusy = false;
        }

        //private async Task OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //    switch(e.PropertyName) {
        //        case nameof(SelectedTagViewModel):
        //            OnSelectedTagItemChanged?.Invoke(this, SelectedTagViewModel);
        //            break;
        //    }
        //}

        private async void MpTagCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
           //switch (e.PropertyName) {
           //     case nameof(SelectedTagViewModel):
           //         if (SelectedTagViewModel != null) {
           //             await CopyItemCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
           //         }
           //         break;
           // }
        }
        #endregion

        #region Event Handlers
        private void Db_OnItemAdded(object sender, MpDbObject e) {
            if(e is MpCopyItem) {

            }
        }
        private void TagViewModel_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpTagItemViewModel tvm) {
                Task.Run(async () => await MpDb.Instance.UpdateItem<MpTag>(tvm.Tag));
            }
        }
        #endregion

        #region Commands
        public ICommand SelectTagCommand => new Command<object>(async (args) => {
            if(args != null && args is MpTagItemViewModel stivm && stivm != SelectedTagViewModel) {
                SelectedTagViewModel.IsSelected = false;
                stivm.IsSelected = true;
                SelectedTagViewModel = stivm;
                await CopyItemCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
            } else if(args == null) {
                ClearSelection();
            }
        });

        public ICommand DeleteTagCommand => new Command<object>(async (args) => {
            MpConsole.WriteLine("Delete Tag" + (args as MpTagItemViewModel).Tag.TagName);
        });
        #endregion
    }
}
