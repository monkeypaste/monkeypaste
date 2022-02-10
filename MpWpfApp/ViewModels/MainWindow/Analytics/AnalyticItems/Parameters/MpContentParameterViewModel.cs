using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public class MpContentParameterViewModel : MpAnalyticItemParameterViewModel  {
        #region Private Variables
        
        private string _defaultValue;

        #endregion

        #region Properties

        #region State

        
        #endregion

        #region Model

        public override string CurrentValue { get; set; }

        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpContentParameterViewModel() : base(null) { }

        public MpContentParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aip) {
            IsBusy = true;

            Parameter = aip;

            if(Parameter.Values.Count > 0) {
                var defParamVal = aip.Values.FirstOrDefault(x => x.IsDefault);
                if(defParamVal == null) {
                    _defaultValue = aip.Values[0].Value;
                } else {
                    _defaultValue = defParamVal.Value;
                }                
            } else {
                _defaultValue = string.Empty;
            }
            

            //if(Parameter == null || Parameter.Values == null) {
            //    ResetToDefault();
            //} else {
            //    MpAnalyticItemParameterValue defVal = Parameter.Values.FirstOrDefault(x => x.IsDefault);
            //    if (defVal != null) {
            //        _defaultValue = defVal.Value;
            //    } else {
            //        _defaultValue = string.Empty;
            //    }

            //    CurrentValue = _defaultValue;
            //}

            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(CurrentValue));

            OnValidate += MpContentParameterViewModel_OnValidate;
            await Task.Delay(1);

            IsBusy = false;
        }

        private void MpContentParameterViewModel_OnValidate(object sender, EventArgs e) {
            //if (!IsRequired) {
            //    return true;
            //}
            //if (Parameter == null || CurrentValue == null) {
            //    return false;
            //}

            var minCond = Parameter.Values.FirstOrDefault(x => x.IsMinimum);
            if (minCond != null) {
                int minLength = 0;
                try {
                    minLength = Convert.ToInt32(minCond.Value);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Minimum val: {minCond.Value} could not conver to int, exception: {ex}");
                }
                if (CurrentValue.Length < minLength) {
                    ValidationMessage = $"{Label} must be at least {minLength} characters";
                } else {
                    ValidationMessage = string.Empty;
                }
            }
            if(IsValid) {
                var maxCond = Parameter.Values.FirstOrDefault(x => x.IsMaximum);
                if (maxCond != null) {
                    // TODO should cap all input string but especially here
                    int maxLength = int.MaxValue;
                    try {
                        maxLength = Convert.ToInt32(maxCond.Value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Maximum val: {minCond.Value} could not conver to int, exception: {ex}");
                    }
                    if (CurrentValue.Length > maxLength) {
                        ValidationMessage = $"{Label} can be no more than {maxLength} characters";
                    } else {
                        ValidationMessage = string.Empty;
                    }
                }
            }

            if(IsValid) {
                if (!string.IsNullOrEmpty(FormatInfo)) {
                    if (CurrentValue.IndexOfAny(FormatInfo.ToCharArray()) != -1) {
                        ValidationMessage = $"{Label} cannot contain '{FormatInfo}' characters";
                    }
                }
            }

            OnPropertyChanged(nameof(IsValid));
        }

        #endregion

        #region Protected Methods

        #endregion
    }
}
