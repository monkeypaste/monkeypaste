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
                if (Action == null || string.IsNullOrEmpty(Arg4)) {
                    return 0;
                }
                return int.Parse(Arg4);
            }
            set {
                if (ShortcutId != value) {
                    Arg4 = value.ToString();
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

        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if (ShortcutId == 0) {
                ValidationText = $"No Shortcut selected for Shortcut Trigger '{FullName}'";
            } else {
                //while (MpAvShortcutCollectionViewModel.Instance.IsAnyBusy) {
                //    await Task.Delay(100);
                //}
                //var ttvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);

                if (ShortcutViewModel == null) {
                    ValidationText = $"Shortcut for Trigger '{FullName}' not found";
                } else {

                    //if (IsPerformingActionFromCommand) {
                    //    if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    //        if (MpAvClipTrayViewModel.Instance.SelectedModels == null ||
                    //       MpAvClipTrayViewModel.Instance.SelectedModels.Count == 0) {
                    //            ValidationText = $"No content selected, cannot execute '{FullName}' ";
                    //        }
                    //    }
                    //}
                    ValidationText = string.Empty;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }

        protected override void EnableTrigger() {            
            var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);
            if (scvm != null) {
                scvm.RegisterActionComponent(this);
            }
        }

        protected override void DisableTrigger() {
            
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
