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

        private MpActionPluginFormat _actionComponentFormat;
        public override MpActionPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpActionPluginFormat();
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
            Mp.Services.ClipboardMonitor.StartMonitor();
        }

        protected override void DisableTrigger() {
            Mp.Services.ClipboardMonitor.UnregisterActionComponent(this);
            Mp.Services.ClipboardMonitor.StopMonitor();
        }

        protected override bool CanPerformAction(object arg) {
            return !MpAvClipTrayViewModel.Instance.IsAppPaused;
        }

        public override async Task PerformActionAsync(object arg) {
            if (!base.CanPerformAction(arg)) {
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
