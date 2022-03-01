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

        public override string CurrentValue => DefaultValue;

        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpContentParameterViewModel() : base(null) { }

        public MpContentParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;

            _defaultValue = aipv.Value;

            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(CurrentValue));

            OnValidate += MpContentParameterViewModel_OnValidate;
            await Task.Delay(1);

            IsBusy = false;
        }

        private void MpContentParameterViewModel_OnValidate(object sender, EventArgs e) {
            ValidationMessage = string.Empty;

            OnPropertyChanged(nameof(IsValid));
        }

        #endregion

        #region Protected Methods

        #endregion
    }
}
