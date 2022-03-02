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
            if(IsEnabled.HasValue && IsEnabled.Value) {
                return;
            }
            await base.Enable();
            MpClipTrayViewModel.Instance.RegisterTrigger(this);
        }

        protected override async Task Disable() {
            if(IsEnabled.HasValue && !IsEnabled.Value) {
                return;
            }
            await base.Disable();
            MpClipTrayViewModel.Instance.UnregisterTrigger(this);
        }
        #endregion
    }
}
