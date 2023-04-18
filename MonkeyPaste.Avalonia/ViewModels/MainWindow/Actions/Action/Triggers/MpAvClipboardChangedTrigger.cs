using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardChangedTrigger :
        MpAvTriggerActionViewModelBase {
        #region Constants

        //public const string EXCLUDED_APPS_PARAM_ID = "ExcludedAppsParam";
        //public const string POLLING_INTERVAL_MS_PARAM_ID = "PollingIntervalMs";

        //public const int DEFAULT_POLLING_INTERVAL_MS = 300;
        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat();
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvClipboardChangedTrigger(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods

        protected override void EnableTrigger() {
            Mp.Services.ClipboardMonitor.RegisterActionComponent(this);
            Mp.Services.ClipboardMonitor.StartMonitor(false);
        }

        protected override void DisableTrigger() {
            Mp.Services.ClipboardMonitor.UnregisterActionComponent(this);
            Mp.Services.ClipboardMonitor.StopMonitor();
        }

        protected override bool ValidateStartAction(object arg) {
            bool can_start = base.ValidateStartAction(arg);
            if (can_start) {
                can_start = !MpAvClipTrayViewModel.Instance.IsAppPaused;
            }
            IsPerformingAction = can_start;
            return IsPerformingAction;
        }

        public override async Task PerformActionAsync(object arg) {
            if (!base.ValidateStartAction(arg)) {
                return;
            }
            if (arg is MpPortableDataObject mpdo) {
                await base.PerformActionAsync(
                        new MpAvClipboardChangedTriggerOutput() {
                            Previous = null,
                            ClipboardDataObject = mpdo
                        });
            }
        }

        #endregion
    }
}
