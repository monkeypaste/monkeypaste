using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpCheckBoxParameterViewModel : MpAnalyticItemParameterViewModel {
        #region Private Variables

        private string _defaultValue = "0";

        #endregion

        #region Properties

        #region Model

        public override string CurrentValue { get; set; }

        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpCheckBoxParameterViewModel() : base(null) { }

        public MpCheckBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aip) {
            IsBusy = true;

            Parameter = aip;

            if (Parameter == null || Parameter.Values == null) {
                ResetToDefault();
            } else {
                MpAnalyticItemParameterValue defVal = Parameter.Values.FirstOrDefault(x => x.IsDefault);
                if (defVal != null) {
                    _defaultValue = defVal.Value;
                } else {
                    _defaultValue = "0";
                }
            }

            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(BoolValue));

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion
    }
}
