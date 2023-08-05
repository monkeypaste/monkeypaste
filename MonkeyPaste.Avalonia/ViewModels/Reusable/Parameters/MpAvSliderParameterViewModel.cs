using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSliderParameterViewModel : MpAvParameterViewModelBase, MpISliderViewModel {
        #region Private Variables
        #endregion

        #region Interfaces


        #region MpISliderViewModel Implementation

        public double SliderValue {
            get {
                object val = GetClampedValue(CurrentValue);
                if (val == null) {
                    return 0;
                }

                try {
                    return double.Parse(CurrentValue);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error converting slider current value '{CurrentValue}' ex: ", ex);
                    return 0;
                }
            }
            set {
                if (SliderValue != value) {
                    CurrentValue = value.ToStringOrDefault();
                    OnPropertyChanged(nameof(SliderValue));
                }
            }
        }
        public double MinValue => Minimum;
        public double MaxValue => Maximum;

        #endregion

        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Appearance
        #endregion

        #region State
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvSliderParameterViewModel() : base(null) { }

        public MpAvSliderParameterViewModel(MpAvViewModelBase parent) : base(parent) {
            PropertyChanged += MpAvSliderParameterViewModel_PropertyChanged;
        }



        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(CurrentTypedValue));
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
            OnPropertyChanged(nameof(SliderValue));

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpAvSliderParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SliderValue):
                    OnPropertyChanged(nameof(CurrentTypedValue));
                    break;
            }
        }
        private object GetClampedValue(string value) {
            try {
                switch (UnitType) {
                    case MpParameterValueUnitType.Integer:
                        return (int)double.Parse(value);
                    case MpParameterValueUnitType.Decimal:
                        return Math.Round(double.Parse(value), Precision);
                    default:
                        return double.Parse(value);
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting slider value '{value}' ex: ", ex);
                return null;
            }
        }
        #endregion
    }
}
