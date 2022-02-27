using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpShortcutTriggerViewModel : MpTriggerActionViewModelBase {
        #region Properties

        #region Model

        public int ShortcutId {
            get {
                if (Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if (ShortcutId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpShortcutTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override async Task<bool> Validate() {
            await base.Validate();

            if (!IsValid) {
                return IsValid;
            }

            var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm == null) {
                ValidationText = $"Analyzer for Action '{RootTriggerActionViewModel.Label}/{Label}' not found";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected override async Task Enable() {
            await base.Enable();
            var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.RegisterTrigger(this);
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.UnregisterTrigger(this);
            }
        }

        #endregion
    }
}
