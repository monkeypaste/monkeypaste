using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpTagTileCollectionViewModel : MpViewModelBase {
        #region Properties
        public MpClipTileCollectionPageViewModel ClipCollectionViewModel { get; set; }

        public ObservableCollection<MpTagTileViewModel> TagViewModels { get; set; } = new ObservableCollection<MpTagTileViewModel>();

        public MpTagTileViewModel SelectedTagViewModel { get; set; }

        public MpTagTileViewModel RecentTagViewModel {
            get {
                return TagViewModels.Where(x => x.Tag.Id == 1).FirstOrDefault();
            }
        }

        public MpTagTileViewModel FavoritesTagViewModel {
            get {
                return TagViewModels.Where(x => x.Tag.Id == 3).FirstOrDefault();
            }
        }
        #endregion

        #region Public Methods
        public MpTagTileCollectionViewModel() : base() {
            PropertyChanged += MpTagCollectionViewModel_PropertyChanged;
            MpDb.Instance.OnItemAdded += Db_OnItemAdded;
            Task.Run(Initialize);
        }

        public MpTagTileViewModel CreateTagViewModel(MpTag tag) {
            var tagViewModel = new MpTagTileViewModel(tag);
            tagViewModel.PropertyChanged += TagViewModel_PropertyChanged;
            return tagViewModel;
        }

        public void ClearSelection() {
            foreach (var tivm in TagViewModels) {
                tivm.IsSelected = false;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            IsBusy = true;
            var tags = await MpDb.Instance.GetItems<MpTag>();
            TagViewModels = new ObservableCollection<MpTagTileViewModel>(tags.Select(x => CreateTagViewModel(x)));
            OnPropertyChanged(nameof(TagViewModels));
            SelectedTagViewModel = RecentTagViewModel;
            ClipCollectionViewModel = new MpClipTileCollectionPageViewModel();
            await Task.Delay(300);
            IsBusy = false;

            //var tags = await MpDb.Instance.GetItems<MpTag>();
            var dbol = new List<MpISyncableDbObject>();
            foreach (var t in TagViewModels) {
                dbol.Add(t.Tag as MpISyncableDbObject);
            }
            var dbMsgStr = MpDbMessage.Create(dbol);
            MpConsole.WriteLine(dbMsgStr);

            var dbMsg = await MpDbMessage.Parse(dbMsgStr, new MpStringToDbModelTypeConverter());
            foreach (var jdbo in dbMsg.JsonDbObjects) {
                MpConsole.WriteLine(@"Type: " + jdbo.DbObjectType.ToString());
            }
            return;
        }

        private async void MpTagCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedTagViewModel):
                    if (SelectedTagViewModel != null && ClipCollectionViewModel != null) {
                        ClearSelection();
                        SelectedTagViewModel.IsSelected = true;
                        await ClipCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
                    }
                    break;
            }
        }
        #endregion

        #region Event Handlers
        private void Db_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpClip) {

            }
        }

        private async void TagViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (sender is MpTagTileViewModel ttvm) {
                switch (e.PropertyName) {
                    case nameof(ttvm.IsSelected):
                        //if (ttvm.IsSelected) {
                        //    if (SelectedTagViewModel == ttvm) {
                        //        //implies selection came from ui do nothing
                        //    } else {
                        //        if (SelectedTagViewModel != null) {
                        //            SelectedTagViewModel.IsSelected = false;
                        //        }
                        //        SelectedTagViewModel = ttvm;
                        //    }
                        //} else {
                        //    if (SelectedTagViewModel == ttvm) {
                        //        SelectedTagViewModel = null;
                        //    }
                        //}
                        break;
                }
                await MpDb.Instance.UpdateItem<MpTag>(ttvm.Tag);
            }
        }
        #endregion

        #region Commands       

        public ICommand SelectTagCommand => new Command<object>(async (args) => {
            //if (args != null && args is MpTagTileViewModel stivm && stivm != SelectedTagViewModel) {
            //    if(SelectedTagViewModel != null) {
            //        SelectedTagViewModel.IsSelected = false;
            //    }
            //    stivm.IsSelected = true;
            //    SelectedTagViewModel = stivm;
            //    await ClipCollectionViewModel.SetTag(SelectedTagViewModel.Tag.Id);
            //} else if (args == null) {
            //    ClearSelection();
            //}
        });

        public ICommand DeleteTagCommand => new Command<object>(async (args) => {
            MpConsole.WriteLine("Delete Tag" + (args as MpTagTileViewModel).Tag.TagName);
        });
        #endregion
    }
}
