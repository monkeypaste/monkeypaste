namespace MpWpfApp {
    public class MpResultParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public string ResultValue {
            get {
                if (Parameter == null) {
                    return string.Empty;
                }
                return Parameter.ValueCsv;
            }
            set {
                if(Parameter != null && Parameter.ValueCsv != value) {
                    Parameter.ValueCsv = value;
                    OnPropertyChanged(nameof(ResultValue));
                }
            }
        }

        #endregion

        #region Constructors

        public MpResultParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
