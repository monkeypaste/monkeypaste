using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpSingletonViewModel<MpAnalyticItemCollectionViewModel,object> {

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items = new ObservableCollection<MpAnalyticItemViewModel>();

        #endregion

        #endregion

        #region Constructors

        public override async Task Init() {
            IsBusy = true;

            var ail = await MpDb.Instance.GetItemsAsync<MpAnalyticItem>();

            Items.Clear();
            foreach(var ai in ail) {
                var naivm = await CreateAnalyticItemViewModel(ai);
                Items.Add(naivm);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpAnalyticItem ai) {
            var naivm = new MpAnalyticItemViewModel(this);
            await naivm.InitializeAsync(ai);
            return naivm;
        }
        #endregion
    }
}
