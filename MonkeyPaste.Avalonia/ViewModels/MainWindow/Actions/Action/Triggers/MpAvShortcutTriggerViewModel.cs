using MonkeyPaste;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutTriggerViewModel : 
        MpAvTriggerActionViewModelBase,
        MpAvIShortcutCommand {
        #region Properties

        #region MpAvIShortcutCommand Implementation

        public ICommand AssignCommand => AssignHotkeyCommand;
        public MpShortcutType ShortcutType => MpShortcutType.TriggerAction;
        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if (Action == null) {
                    return null;
                }
                var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(
                    x => x.CommandParameter == ActionId.ToString() && x.ShortcutType == ShortcutType);

                if (scvm == null) {
                    scvm = new MpAvShortcutViewModel(MpAvShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }
        public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        #endregion

        #region Model

        // Arg1

        public int ShortcutId {
            get {
                if (Action == null || string.IsNullOrEmpty(Arg1)) {
                    return 0;
                }
                return int.Parse(Arg1);
            }
            set {
                if (ShortcutId != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Methods

        protected override async Task<bool> ValidateActionAsync() {
            await base.ValidateActionAsync();

            if (!IsValid) {
                return IsValid;
            }

            var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm == null) {
                ValidationText = $"Shortcut for Trigger Action '{FullName}' not found";
                ShowValidationNotification();
            } else if (IsPerformingActionFromCommand) {
                if(MpAvClipTrayViewModel.Instance.SelectedModels == null ||
                   MpAvClipTrayViewModel.Instance.SelectedModels.Count == 0) {
                    ValidationText = $"No content selected, cannot execute '{FullName}' ";
                    ShowValidationNotification();
                }
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        protected override async Task EnableAsync() {
            await base.EnableAsync();
            var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.RegisterActionComponent(this);
            }
        }

        protected override async Task DisableAsync() {
            await base.DisableAsync();
            var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.UnregisterActionComponent(this);
            }
        }

        #endregion

        #region Commands

        public ICommand AssignHotkeyCommand => new MpAsyncCommand(
            async () => {
                await MpAvShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    $"Trigger {Label} Action",
                    PerformActionOnSelectedContentCommand,
                    MpShortcutType.TriggerAction,
                    ActionId.ToString(),
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
