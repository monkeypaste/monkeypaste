using MonkeyPaste;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentTaggedTriggerViewModel : 
        MpAvTriggerActionViewModelBase,
        MpIPopupSelectorMenu {


        #region MpIPopupSelectorMenu Implementation

        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        MpAvTagTrayViewModel.Instance.AllTagViewModel.GetTagMenu(SelectTagCommand,new int[] { TagId}, true)
                    }
                };
            }
        }
        public MpMenuItemViewModel SelectedMenuItem =>
            SelectedTag == null ? null : SelectedTag.GetTagMenu(null, new int[] { TagId }, false);
        public string EmptyText => "Select Tag...";
        public object EmptyIconResourceObj => MpAvActionViewModelBase.GetDefaultActionIconResourceKey(MpActionType.Classify, null);
        #endregion

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
                if (Action == null || string.IsNullOrEmpty(Arg1)) {
                    return 0;
                }
                return int.Parse(Arg1);
            }
            set {
                if (TagId != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvContentTaggedTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task<bool> ValidateActionAsync() {
            await base.ValidateActionAsync();

            if (!IsValid) {
                return IsValid;
            }

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm == null) {
                ValidationText = $"Tag for Classifier '{RootTriggerActionViewModel.Label}/{Label}' not found";
                ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected override async Task EnableAsync() {
            await base.EnableAsync();
            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.RegisterActionComponent(this);
            }
        }

        protected override async Task DisableAsync() {
            await base.DisableAsync();
            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm != null) {
                ttvm.UnregisterActionComponent(this);
            }
        }
        #endregion

        #region Commands

        public ICommand SelectTagCommand => new MpCommand<object>(
            (args) => {
                if (args is int tagId) {
                    TagId = tagId;
                    OnPropertyChanged(nameof(SelectedTag));
                    OnPropertyChanged(nameof(SelectedMenuItem));
                }
            });


        #endregion
    }
}
