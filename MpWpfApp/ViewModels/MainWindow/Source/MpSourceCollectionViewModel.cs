using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpSourceCollectionViewModel : 
        MpSelectorViewModelBase<object,MpSourceViewModel>, 
        MpIAsyncSingletonViewModel<MpSourceCollectionViewModel> {
        #region Properties

        #region View Models


        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors


        private static MpSourceCollectionViewModel _instance;
        public static MpSourceCollectionViewModel Instance => _instance ?? (_instance = new MpSourceCollectionViewModel());

        public MpSourceCollectionViewModel() : base(null) { }

        public async Task InitAsync() {
            IsBusy = true;
            while (MpAppCollectionViewModel.Instance.IsAnyBusy ||
                  MpUrlCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var sl = await MpDb.GetItemsAsync<MpSource>();
            foreach(var s in sl) {
                var svm = await CreateSourceViewModel(s);
                Items.Add(svm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

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
                MpHelpers.RunOnMainThread(async () => {
                    var svm = await CreateSourceViewModel(s);
                    Items.Add(svm);
                    OnPropertyChanged(nameof(Items));
                });
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpSource s) {
                MpHelpers.RunOnMainThread(async () => {
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