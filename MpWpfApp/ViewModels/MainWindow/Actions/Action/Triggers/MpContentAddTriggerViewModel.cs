using System.Collections.Generic;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentAddTriggerViewModel : 
        MpTriggerActionViewModelBase {
        #region Constructors

        public MpContentAddTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Methods

        public override void Enable() {
            if(!IsEnabled) {
                MpClipTrayViewModel.Instance.RegisterTrigger(this);
            }
            base.Enable();
        }
        #endregion
    }
}
