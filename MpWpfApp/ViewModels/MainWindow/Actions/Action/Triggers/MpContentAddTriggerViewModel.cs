using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentAddTriggerViewModel : 
        MpTriggerActionViewModelBase {
        #region Constructors

        public MpContentAddTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            MpClipTrayViewModel.Instance.RegisterTrigger(this);
        }

        protected override async Task Disable() {
            await base.Disable();
            MpClipTrayViewModel.Instance.UnregisterTrigger(this);
        }
        #endregion
    }
}
