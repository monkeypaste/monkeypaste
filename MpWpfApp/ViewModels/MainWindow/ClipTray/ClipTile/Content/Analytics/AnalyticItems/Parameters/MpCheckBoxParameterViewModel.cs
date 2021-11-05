namespace MpWpfApp {
    public class MpCheckBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public bool IsChecked {
            get {
                if (Parameter == null) {
                    return false;
                }
                if (string.IsNullOrEmpty(Parameter.ValueCsv) &&
                   !string.IsNullOrEmpty(Parameter.DefaultValue)) {
                    Parameter.ValueCsv = Parameter.DefaultValue;
                }
                if(string.IsNullOrEmpty(Parameter.ValueCsv)) {
                    Parameter.ValueCsv = "0";
                }
                return Parameter.ValueCsv == "1";
            }
            set {
                if (Parameter != null && IsChecked != value) {
                    Parameter.ValueCsv = value ? "1":"0";
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        #endregion

        #region Constructors

        public MpCheckBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
