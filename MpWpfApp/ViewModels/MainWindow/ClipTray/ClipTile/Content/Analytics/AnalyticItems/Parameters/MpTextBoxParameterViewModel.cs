namespace MpWpfApp {
    public class MpTextBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        #region State
                
        public override bool IsValid {
            get {
                if(!IsRequired) {
                    return true;
                }
                return !string.IsNullOrEmpty(InputValue);
            }
        }
        #endregion

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

        public MpTextBoxParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }


        #endregion
    }
}
