using System.Collections.Generic;

namespace MpWpfApp {
    public class MpContentAddTriggerViewModel : MpTriggerActionViewModelBase {
        #region Constructors

        public MpContentAddTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Methods

        public override void Enable() {
            MpClipTrayViewModel.Instance.RegisterTrigger(this);
            base.Enable();
        }
        #endregion
    }
}
