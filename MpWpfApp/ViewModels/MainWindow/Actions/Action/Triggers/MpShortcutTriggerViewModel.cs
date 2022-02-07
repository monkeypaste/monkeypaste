using System.Linq;

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

        public override void Enable() {
            if(!IsEnabled) {
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
                if (scvm != null) {
                    scvm.RegisterTrigger(this);
                }
            }
            base.Enable();
        }

        public override void Disable() {
            if (IsEnabled) {
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
                if (scvm != null) {
                    scvm.UnregisterTrigger(this);
                }
            }
            base.Disable();
        }

        #endregion
    }
}
