using MonkeyPaste;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentTaggedTriggerViewModel : MpAvTriggerActionViewModelBase {
        #region Properties

        #region View Models

        public MpAvTagTileViewModel SelectedTag {
            get {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            }
            set {
                if (SelectedTag != value) {
                    TagId = value.TagId;
                    OnPropertyChanged(nameof(SelectedTag));
                }
            }
        }

        #endregion

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

        public MpAvContentTaggedTriggerViewModel(MpAvActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(Validate);
            }
        }
        protected override async Task<bool> Validate() {
            await base.Validate();

            if (!IsValid) {
                return IsValid;
            }

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm == null) {
                ValidationText = $"Tag for Classifier '{RootTriggerActionViewModel.Label}/{Label}' not found";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected override async Task Enable() {
            await base.Enable();
            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.RegisterActionComponent(this);
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.UnregisterActionComponent(this);
            }
        }
        #endregion
    }
}
