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

        #region Public Methods

        public override async Task Enable() {

            if (IsEnabled) {
                return;
            }
            await Validate();
            if (IsValid) {
                MpClipTrayViewModel.Instance.RegisterTrigger(this);
                IsEnabled = true;
            }
            await base.Enable();
        }

        public override async Task Disable() {
            if (!IsEnabled) {
                return;
            }

            MpClipTrayViewModel.Instance.UnregisterTrigger(this);

            IsEnabled = false;

            await base.Disable();
        }
        #endregion
    }
}
