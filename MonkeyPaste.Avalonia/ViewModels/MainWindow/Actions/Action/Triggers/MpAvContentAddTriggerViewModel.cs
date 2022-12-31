using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentAddTriggerViewModel : 
        MpAvTriggerActionViewModelBase {
        #region Properties

        #region Model

        public MpCopyItemType AddedContentType {
            get {
                if (Action == null || string.IsNullOrEmpty(Arg4)) {
                    return MpCopyItemType.None;
                }
                return Arg4.ToEnum<MpCopyItemType>();
            }
            set {
                if (AddedContentType != value) {
                    Arg4 = value.ToString();
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

        protected override async Task ValidateActionAsync() {
            // is always valid
            await Task.Delay(1);
        }
        protected override void EnableTrigger() {
            MpAvClipTrayViewModel.Instance.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {
            MpAvClipTrayViewModel.Instance.UnregisterActionComponent(this);
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
