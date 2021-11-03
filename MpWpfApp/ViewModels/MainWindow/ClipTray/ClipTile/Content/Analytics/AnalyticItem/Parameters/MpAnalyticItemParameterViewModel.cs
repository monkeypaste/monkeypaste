using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemParameterViewModel : MpViewModelBase<MpAnalyticItemViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemParameterValueViewModel> Values { get; set; } = new ObservableCollection<MpAnalyticItemParameterValueViewModel>();
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Model
                

        public MpAnalyticItemParameter Parameter { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterViewModel() : base(null) { }

        public MpAnalyticItemParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemParameter aip) {
            IsBusy = true;

            Parameter = aip;

            Values.Clear();
            var valueParts = Parameter.ValueCsv.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < valueParts.Length; i++) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(i, valueParts[i]);
                Values.Add(naipvvm);
            }

            OnPropertyChanged(nameof(Values));

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(int idx, string value) {
            var naipvvm = new MpAnalyticItemParameterValueViewModel(this);
            await naipvvm.InitializeAsync(idx, value);
            return naipvvm;
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

            }
        }
        #endregion
    }
}
