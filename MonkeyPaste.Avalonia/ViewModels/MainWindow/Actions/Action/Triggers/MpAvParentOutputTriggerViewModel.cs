using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvParentOutputTriggerViewModel : MpAvTriggerActionViewModelBase {
        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvParentOutputTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            if (ParentTreeItem != null) {
                ParentTreeItem.RegisterTrigger(this);
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            if (ParentTreeItem == null) {
                ParentTreeItem.UnregisterTrigger(this);
            }
        }
        #endregion
    }
}
