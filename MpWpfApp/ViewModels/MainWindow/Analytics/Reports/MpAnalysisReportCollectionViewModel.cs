using MonkeyPaste;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalysisReportCollectionViewModel : 
        MpSelectorViewModelBase<MpContentItemViewModel,MpAnalysisReportViewModel> {
        #region Properties

        #region View Models

        #endregion

        #endregion

        #region Constructors

        public MpAnalysisReportCollectionViewModel() : base(null) { }
        public MpAnalysisReportCollectionViewModel(MpContentItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int copyItemId) {
            if(copyItemId == 0) {
                return;
            }

            IsBusy = true;

            var ciTransactions = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemId(copyItemId);
            foreach(var cit in ciTransactions) {
                var rvm = await CreateReportViewModel(cit.ResponseJson);
                Items.Add(rvm);
            }
            if(Items.Count > 0) {
                Items[0].IsSelected = true;
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedItem));

            IsBusy = false;
        }

        public async Task<MpAnalysisReportViewModel> CreateReportViewModel(string responseStr) {
            var rvm = new MpAnalysisReportViewModel(this);
            await rvm.InitializeAsync(responseStr);
            return rvm;
        }
        #endregion
    }
}
