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

        #region Protected Methods

        protected override async Task Enable() {
            await base.Enable();
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.RegisterTrigger(this);
                IsEnabled = true;
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.UnregisterTrigger(this);
            }
        }
        #endregion


    }
}
