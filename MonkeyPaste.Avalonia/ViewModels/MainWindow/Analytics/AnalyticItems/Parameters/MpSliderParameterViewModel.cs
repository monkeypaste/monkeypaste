using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpSliderParameterViewModel : MpPluginParameterViewModelBase, MpISliderViewModel {
        #region Private Variables
       // private string _defaultValue;
        #endregion

        #region Properties

        #region View Models
        //public override MpAnalyticItemParameterValueViewModel CurrentValueViewModel => new MpAnalyticItemParameterValueViewModel() { Value = this.Value.ToString() };
        #endregion

        #region MpISliderViewModel Implementation

        public double TotalWidth { get; set; }

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
                if (SliderValue != value) {
                    CurrentValue = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SliderValue));
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

        public MpSliderParameterViewModel() : base(null) { }

        public MpSliderParameterViewModel(MpIPluginComponentViewModel parent) : base(parent) { }

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
    }
}
