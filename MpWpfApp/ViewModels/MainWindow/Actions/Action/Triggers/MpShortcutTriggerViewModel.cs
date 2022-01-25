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
            var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.RegisterTrigger(this);
            }
            base.Enable();
        }

        #endregion
    }
}
