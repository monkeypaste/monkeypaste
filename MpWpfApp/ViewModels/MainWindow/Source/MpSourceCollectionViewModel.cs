using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpSourceCollectionViewModel : MpSingletonViewModel2<MpSourceCollectionViewModel> {
        #region Properties

        #region View Models

        public ObservableCollection<MpSourceViewModel> Items { get; set; } = new ObservableCollection<MpSourceViewModel>();

        public MpSourceViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                Items.ForEach(x => x.IsSelected = false);
                if(value != null) {
                    value.IsSelected = true;
                }
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpSourceCollectionViewModel() : base() {
            Task.Run(Init);
        }

        public async Task Init() {
            IsBusy = true;

            //await MpIconCollectionViewModel.Instance.Init();
            //await MpAppCollectionViewModel.Instance.Init();
            //await MpUrlCollectionViewModel.Instance.Init();

            var sl = await MpDb.Instance.GetItemsAsync<MpSource>();
            foreach(var s in sl) {
                var svm = await CreateSourceViewModel(s);
                Items.Add(svm);
            }

            Application.Current.Resources["SourceCollectionViewModel"] = this;

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpSourceViewModel> CreateSourceViewModel(MpSource s) {
            var svm = new MpSourceViewModel(this);
            await svm.InitializeAsync(s);
            return svm;
        }

        public MpSourceViewModel GetSourceViewModelBySourceId(int sid) {
            return Items.FirstOrDefault(x => x.Source.Id == sid);
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpSource s) {
                MpHelpers.Instance.RunOnMainThread(async () => {
                    var svm = await CreateSourceViewModel(s);
                    Items.Add(svm);
                    OnPropertyChanged(nameof(Items));
                });
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpSource s) {
                MpHelpers.Instance.RunOnMainThread(async () => {
                    var svm = Items.FirstOrDefault(x => x.Source.Id == s.Id);
                    if(svm != null) {
                        await svm.InitializeAsync(s);
                    }
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpSource s) {
                var svm = Items.FirstOrDefault(x => x.Source.Id == s.Id);
                if (svm != null) {
                    Items.Remove(svm);
                }
            }
        }
        #endregion
        #endregion
    }
}