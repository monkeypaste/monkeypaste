using MonkeyPaste.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvMonkeyCopyTriggerViewModel :
        MpAvShortcutTriggerViewModel {

        #region Constants
        #endregion

        #region Interfaces
        #endregion

        #region MpIParameterHost Overrides
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionMonkeyCopyTriggerHint;

        #endregion
        #region State

        #endregion

        #region Model
        #endregion

        #endregion

        #region Constructors

        public MpAvMonkeyCopyTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods
        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }
            if (arg is not MpAvActionOutput ao || ao.CopyItem != null) {
                // arg should only be non-null from a dnd drop
                await base.PerformActionAsync(arg);
                return;
            }
            string copy_cmd = MpAvShortcutCollectionViewModel.Instance.ActiveCopyKeystring ?? Mp.Services.PlatformShorcuts.CopyKeys;
            if (string.IsNullOrEmpty(copy_cmd)) {
                await FinishActionAsync(arg);
                return;
            }

            MpCopyItem input_ci = null;
            void OnContentAdd(object sender, MpCopyItem ci) {
                MpAvClipTrayViewModel.Instance.OnCopyItemAdd -= OnContentAdd;
                input_ci = ci;
            }
            MpAvClipTrayViewModel.Instance.OnCopyItemAdd += OnContentAdd;

            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(copy_cmd);

            var sw = Stopwatch.StartNew();
            while (input_ci == null) {
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 10_000) {
                    // nothing added
                    MpDebug.Break($"MonkeyCopy trigger timeout", silent: true);
                    break;
                }
            }
            await FinishActionAsync(new MpAvMonkeyCopyOutput() {
                Previous = ao,
                CopyItem = input_ci
            });
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands
        #endregion
    }
}
