using System;

namespace MpWpfApp {
    public class MpSliderParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Properties

        public double Min {
            get {
                if (Parameter == null) {
                    return 0;
                }
                var valueParts = Parameter.ValueCsv.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
                return Convert.ToDouble(valueParts[0]);
            }
        }

        public double Max {
            get {
                if (Parameter == null) {
                    return 0;
                }
                var valueParts = Parameter.ValueCsv.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
                return Convert.ToDouble(valueParts[valueParts.Length-1]);
            }
        }

        public double TickFrequency => Max - Min >= 1.0 ? 1.0 : 0.01;

        public double Value {
            get {
                if(Parameter == null) {
                    return 0;
                }
                if(!string.IsNullOrWhiteSpace(Parameter.InputValue)) {
                    return Convert.ToDouble(Parameter.InputValue);
                }
                return Convert.ToDouble(Parameter.DefaultValue);
            }
            set {
                if(Parameter != null && Parameter.InputValue.ToString() != value.ToString()) {
                    Parameter.InputValue = value.ToString();
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public override MpAnalyticItemParameterValueViewModel SelectedValue => new MpAnalyticItemParameterValueViewModel() { Value = this.Value.ToString() };

        public override bool IsValid => true;

        #endregion

        #region Constructors

        public MpSliderParameterViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion
    }
}
