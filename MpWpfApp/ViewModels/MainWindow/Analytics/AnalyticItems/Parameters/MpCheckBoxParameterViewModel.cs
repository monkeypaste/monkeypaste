using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpCheckBoxParameterViewModel : MpPluginParameterViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpCheckBoxParameterViewModel() : base(null) { }

        public MpCheckBoxParameterViewModel(MpViewModelBase parent) : base(parent) { }

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            IsBusy = false;
        }

        #endregion
    }
}
