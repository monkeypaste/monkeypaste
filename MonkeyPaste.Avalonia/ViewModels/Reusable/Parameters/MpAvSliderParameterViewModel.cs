using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSliderParameterViewModel : MpAvParameterViewModelBase, MpISliderViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Appearance
        #endregion

        #region MpISliderViewModel Implementation

        public double SliderValue { 
            get {
                object val = GetClampedValue(CurrentValue);
                if(val == null) {
                    return 0;
                }

                try {
                    return double.Parse(CurrentValue);
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting slider current value '{CurrentValue}' ex: ", ex);
                    return 0;
                }
            }
        }
        public double MinValue => Minimum;
        public double MaxValue => Maximum;

        #endregion

        #region Model

        #endregion        

        #endregion

        #region Constructors

        public MpAvSliderParameterViewModel() : base(null) { }

        public MpAvSliderParameterViewModel(MpIParameterHostViewModel parent) : base(parent) {
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(SliderValue));

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private object GetClampedValue(string value) {
            try {
                switch (UnitType) {
                    case MpParameterValueUnitType.Integer:
                        return int.Parse(value);
                    case MpParameterValueUnitType.Decimal:
                        return Math.Round(double.Parse(value), Precision);
                    default:
                        return double.Parse(value);
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error converting slider value '{value}' ex: ", ex);
                return null;
            }
        }
        #endregion
    }
}
