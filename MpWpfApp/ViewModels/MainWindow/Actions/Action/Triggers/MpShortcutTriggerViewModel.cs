using MonkeyPaste;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcutTriggerViewModel : 
        MpTriggerActionViewModelBase,
        MpIShortcutCommand {
        #region Properties

        #region MpIShortcutCommand Implementation

        public ICommand AssignCommand => AssignHotkeyCommand;
        public MpShortcutType ShortcutType => MpShortcutType.TriggerAction;
        public MpShortcutViewModel ShortcutViewModel {
            get {
                if (Action == null) {
                    return null;
                }
                var scvm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(
                    x => x.CommandId == ActionId && x.ShortcutType == ShortcutType);

                if (scvm == null) {
                    scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }
        public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        #endregion

        #region Model

        public int ShortcutId {
            get {
                if (Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if (ShortcutId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpShortcutTriggerViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override async Task<bool> Validate() {
            await base.Validate();

            if (!IsValid) {
                return IsValid;
            }

            var scvm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm == null) {
                ValidationText = $"Shortcut for Trigger Action '{FullName}' not found";
                await ShowValidationNotification();
            } else if (IsPerformingActionFromCommand) {
                if(MpClipTrayViewModel.Instance.SelectedModels == null ||
                   MpClipTrayViewModel.Instance.SelectedModels.Count == 0) {
                    ValidationText = $"No content selected, cannot execute '{FullName}' ";
                    await ShowValidationNotification();
                }
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected override async Task Enable() {
            await base.Enable();
            var scvm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.RegisterActionComponent(this);
            }
        }

        protected override async Task Disable() {
            await base.Disable();
            var scvm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.UnregisterActionComponent(this);
            }
        }

        #endregion

        #region Commands

        public MpIAsyncCommand AssignHotkeyCommand => new MpAsyncCommand(
            async () => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    $"Trigger {Label} Action",
                    PerformActionOnSelectedContentCommand,
                    MpShortcutType.TriggerAction,
                    ActionId,
                    ShortcutKeyString);

                OnPropertyChanged(nameof(ShortcutViewModel));

                OnPropertyChanged(nameof(ShortcutKeyString));


                if (ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });

        #endregion
    }
}
