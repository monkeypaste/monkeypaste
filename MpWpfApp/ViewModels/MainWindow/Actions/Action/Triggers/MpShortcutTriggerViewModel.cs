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

        #region Public Methods

        public override async Task<bool> Validate() {
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

        public override async Task Enable() {

            if (IsEnabled) {
                return;
            }
            await Validate();
            if(IsValid) {
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
                if (scvm != null) {
                    scvm.RegisterTrigger(this);
                    IsEnabled = true;
                }
            }
            await base.Enable();
        }

        public override async Task Disable() {
            if(!IsEnabled) {
                return;
            }

            var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.UnregisterTrigger(this);
            }

            IsEnabled = false;
            await base.Disable();
        }

        #endregion
    }
}
