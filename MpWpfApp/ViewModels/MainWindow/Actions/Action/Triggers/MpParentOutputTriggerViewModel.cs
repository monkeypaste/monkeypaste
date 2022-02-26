using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpParentOutputTriggerViewModel : MpTriggerActionViewModelBase {
        #region Properties

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpParentOutputTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Public Methods

        public override async Task Enable() {

            if (IsEnabled) {
                return;
            }
            await Validate();
            if (IsValid) {
                if (ParentActionViewModel == null) {
                    throw new System.Exception("Parent should be found in init");
                }
                ParentActionViewModel.RegisterTrigger(this);
                IsEnabled = true;
            }
            await base.Enable();
        }

        public override async Task Disable() {
            if (!IsEnabled) {
                return;
            }

            MpFileSystemWatcherViewModel.Instance.UnregisterTrigger(this);

            IsEnabled = false;
            await base.Disable();
        }
        #endregion
    }
}
