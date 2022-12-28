using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Atk;

namespace MonkeyPaste.Avalonia {
    public class MpAvSliderParameterViewModel : MpAvPluginParameterViewModelBase, MpISliderViewModel {
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
                if (CurrentValue == null || ParameterFormat == null) {
                    return 0;
                }

                switch (UnitType) {
                    case MpPluginParameterValueUnitType.Integer:
                        return IntValue;
                    case MpPluginParameterValueUnitType.Decimal:
                        return Math.Round(DoubleValue, Precision);
                    default:
                        return DoubleValue;
                }
            }
            set {
                if(this is MpISliderViewModel svm &&
                    svm.SliderValue != value) {
                    CurrentValue = value.ToString();
                    HasModelChanged = true;
                    svm.OnPropertyChanged(nameof(svm.SliderValue));
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

        public MpAvSliderParameterViewModel(MpIPluginComponentViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            OnPropertyChanged(nameof(CurrentValue));     
            if(this is MpISliderViewModel svm) {
                svm.OnPropertyChanged(nameof(svm.MinValue));
                svm.OnPropertyChanged(nameof(svm.MaxValue));
                svm.OnPropertyChanged(nameof(svm.SliderValue));

            }
            
            await Task.Delay(1);

            IsBusy = false;
        }



        #endregion
    }
}
