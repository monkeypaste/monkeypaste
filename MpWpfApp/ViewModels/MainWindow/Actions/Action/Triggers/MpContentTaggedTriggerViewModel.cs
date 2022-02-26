using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public override async Task Enable() {

            if (IsEnabled) {
                return;
            }
            await Validate();
            if (IsValid) {
                var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
                if (ttvm != null) {
                    ttvm.RegisterTrigger(this);
                    IsEnabled = true;
                }
            }
            await base.Enable();
        }

        public override async Task Disable() {
            if (!IsEnabled) {
                return;
            }

            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.UnregisterTrigger(this);
            }

            IsEnabled = false;
            await base.Disable();
        }
        #endregion


    }
}
