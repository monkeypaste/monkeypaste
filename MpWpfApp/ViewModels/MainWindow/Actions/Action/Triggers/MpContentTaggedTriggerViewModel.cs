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
            var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.RegisterTrigger(this);
            }
            base.Enable();
        }

        #endregion


    }
}
