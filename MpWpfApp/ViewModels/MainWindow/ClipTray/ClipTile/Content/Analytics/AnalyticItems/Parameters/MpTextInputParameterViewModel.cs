namespace MpWpfApp {
    public class MpTextInputParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public string InputValue {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(Parameter.ValueCsv) && 
                   !string.IsNullOrEmpty(Parameter.DefaultValue)) {
                    Parameter.ValueCsv = Parameter.DefaultValue;
                }
                return Parameter.ValueCsv;
            }
            set {
                if (Parameter != null && Parameter.ValueCsv != value) {
                    Parameter.ValueCsv = value;
                    OnPropertyChanged(nameof(InputValue));
                }
            }
        }

        #endregion

        #region Constructors

        public MpTextInputParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
