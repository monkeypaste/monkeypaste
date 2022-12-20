using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentAddTriggerViewModel : 
        MpAvTriggerActionViewModelBase {
        #region Properties

        #region Model

        public MpCopyItemType AddedContentType {
            get {
                if (Action == null) {
                    return 0;
                }
                return (MpCopyItemType)ActionObjId;
            }
            set {
                if (AddedContentType != value) {
                    ActionObjId = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AddedContentType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvContentAddTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            if(IsEnabled.HasValue && IsEnabled.Value) {
                return;
            }
            await base.Enable();
            //MpAvClipTrayViewModel.Instance.RegisterActionComponent(this);
        }

        protected override async Task Disable() {
            if(IsEnabled.HasValue && !IsEnabled.Value) {
                return;
            }
            await base.Disable();
            //MpAvClipTrayViewModel.Instance.UnregisterActionComponent(this);
        }

        protected override bool CanPerformAction(object arg) {
            if(!base.CanPerformAction(arg)) {
                return false;
            }
            if(AddedContentType == MpCopyItemType.None) {
                // NOTE Default is treated as all types
                return true;
            }
            if(arg is MpCopyItem ci && ci.ItemType != AddedContentType) {
                return false;
            }
            return true;
        }
        #endregion
    }
}
