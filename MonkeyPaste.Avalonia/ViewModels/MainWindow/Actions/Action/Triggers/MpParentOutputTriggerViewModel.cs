using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpParentOutputTriggerViewModel : MpAvTriggerActionViewModelBase {
        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpParentOutputTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            if (ParentActionViewModel != null) {
                ParentActionViewModel.RegisterTrigger(this);
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            if (ParentActionViewModel == null) {
                ParentActionViewModel.UnregisterTrigger(this);
            }
        }
        #endregion
    }
}
