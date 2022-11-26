using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Avalonia.Threading;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {

    public class MpAvSourceCollectionViewModel : 
        MpAvSelectorViewModelBase<object,MpAvSourceViewModel>, 
        MpIAsyncSingletonViewModel<MpAvSourceCollectionViewModel> {
        #region Properties

        #region View Models


        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors


        private static MpAvSourceCollectionViewModel _instance;
        public static MpAvSourceCollectionViewModel Instance => _instance ?? (_instance = new MpAvSourceCollectionViewModel());

        public MpAvSourceCollectionViewModel() : base(null) { }

        public async Task InitAsync() {
            IsBusy = true;
            while (MpAvAppCollectionViewModel.Instance.IsAnyBusy ||
                  MpAvUrlCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var sl = await MpDataModelProvider.GetItemsAsync<MpSource>();
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

        public async Task<MpAvSourceViewModel> CreateSourceViewModel(MpSource s) {
            var svm = new MpAvSourceViewModel(this);
            await svm.InitializeAsync(s);
            return svm;
        }

        public MpAvSourceViewModel GetSourceViewModelBySourceId(int sid) {
            return Items.FirstOrDefault(x => x.Source.Id == sid);
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpSource s) {
                Dispatcher.UIThread.Post(async () => {
                    var svm = await CreateSourceViewModel(s);
                    Items.Add(svm);
                    OnPropertyChanged(nameof(Items));
                });
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpSource s) {
                Dispatcher.UIThread.Post(async () => {
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

        #region Commands
        #endregion
    }
}