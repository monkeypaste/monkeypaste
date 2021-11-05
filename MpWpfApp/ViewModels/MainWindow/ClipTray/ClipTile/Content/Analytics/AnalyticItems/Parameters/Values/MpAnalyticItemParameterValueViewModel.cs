using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemParameterValueViewModel : MpViewModelBase<MpAnalyticItemParameterViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Model

        public int ValueIdx { get; set; } = 0;

        public string Value { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemParameterValueViewModel() : base(null) { }

        public MpAnalyticItemParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx,string value) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            Value = value;

            IsBusy = false;
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

            }
        }
        #endregion
    }
}
