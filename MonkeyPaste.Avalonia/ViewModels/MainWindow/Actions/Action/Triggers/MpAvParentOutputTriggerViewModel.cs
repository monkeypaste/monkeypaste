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

        protected override async Task EnableAsync() {
            await base.EnableAsync();
            if (ParentActionViewModel != null) {
                ParentActionViewModel.RegisterTrigger(this);
            }
        }

        protected override async Task DisableAsync() {
            await base.DisableAsync();
            if (ParentActionViewModel == null) {
                ParentActionViewModel.UnregisterTrigger(this);
            }
        }
        #endregion
    }
}
