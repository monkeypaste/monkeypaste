using Gtk;
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
        public MpShortcutType ShortcutType => MpShortcutType.InvokeAction;
        public MpAvShortcutViewModel ShortcutViewModel =>
            MpAvShortcutCollectionViewModel.Instance.Items
            .FirstOrDefault(x => x.CommandParameter == ActionId.ToString() && x.ShortcutType == MpShortcutType.InvokeAction);
        public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        #endregion

        #region Model

        public override string Description { 
            get => base.Description; 
            set => base.Description = value; 
        }

        // Arg1

        //public int ShortcutId {
        //    get {
        //        if (Action == null || string.IsNullOrEmpty(Arg4)) {
        //            return 0;
        //        }
        //        return int.Parse(Arg4);
        //    }
        //    set {
        //        if (ShortcutId != value) {
        //            Arg4 = value.ToString();
        //            HasModelChanged = true;
        //            OnPropertyChanged(nameof(ShortcutId));
        //        }
        //    }
        //}

        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvShortcutTriggerViewModel_PropertyChanged;
        }


        #endregion

        #region Protected Methods

        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
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
            if (!IsValid) {
                ShowValidationNotification();
            }
        }

        protected override void EnableTrigger() {            
            if(ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {
            if (ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.UnregisterActionComponent(this);
        }

        #endregion

        #region Private Methods


        private void MpAvShortcutTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(ShortcutViewModel):
                    //ShortcutId = ShortcutViewModel == null ? 
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand AssignHotkeyCommand => new MpAsyncCommand(
            async () => {
                await MpAvShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    shortcutType: MpShortcutType.InvokeAction,
                    title: $"Trigger {Label} Action",
                    command: InvokeThisActionCommand,
                    commandParameter: ActionId.ToString(),
                    keys: ShortcutKeyString);

                OnPropertyChanged(nameof(ShortcutViewModel));

                OnPropertyChanged(nameof(ShortcutKeyString));


                if (ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });


        

        #endregion
    }
}
