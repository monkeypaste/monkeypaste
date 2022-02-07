using System.Linq;

namespace MpWpfApp {
    public class MpContentTaggedTriggerViewModel : MpTriggerActionViewModelBase {
        #region Properties

        #region Model

        public int TagId {
            get {
                if (Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if (TagId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpContentTaggedTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Methods

        public override void Enable() {
            if(!IsEnabled) {
                var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
                if (ttvm != null) {
                    ttvm.RegisterTrigger(this);
                }
            }
            
            base.Enable();
        }

        public override void Disable() {
            if (IsEnabled) {
                var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
                if (ttvm != null) {
                    ttvm.UnregisterTrigger(this);
                }
            }

            base.Disable();
        }

        #endregion


    }
}
