using Avalonia.Threading;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutTriggerViewModel :
        MpAvTriggerActionViewModelBase,
        //MpIShortcutCommandViewModel,
        //MpAvIKeyGestureViewModel,
        MpIApplicationCommandCollectionViewModel {

        #region Constants

        public const string SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID = "ShortcutTriggerKeyString";

        #endregion

        #region Interfaces

        //#region MpAvIKeyGestureViewModel Implementation

        //public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
        //    new ObservableCollection<MpAvShortcutKeyGroupViewModel>(KeyString.ToKeyItems());
        //#endregion

        //#region MpIShortcutCommandViewModel Implementation
        //public MpShortcutType ShortcutType =>
        //    MpShortcutType.InvokeTrigger;

        //public string KeyString =>
        //    MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);

        //public object ShortcutCommandParameter =>
        //    ActionId;
        //ICommand MpIShortcutCommandViewModel.ShortcutCommand =>
        //    InvokeThisActionCommand;
        //#endregion


        #region MpAvIApplicationCommandViewModel Implementation

        public IEnumerable<MpApplicationCommand> Commands =>
            new MpApplicationCommand[] {
                new MpApplicationCommand() {
                    Command = MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand,
                    CommandParameter = ActionId,
                    Tag = SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID
                }
            };

        #endregion


        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Shortcut Trigger",
                                controlType = MpParameterControlType.ShortcutRecorder,
                                unitType = MpParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID,
                                description = "Triggered when the recorded shortcut is pressed at anytime with the current clipboard"
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region View Models

        //public MpAvShortcutViewModel ShortcutViewModel =>
        //    MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);

        private MpAvShortcutViewModel _shortcutViewModel;
        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if (_shortcutViewModel == null &&
                    ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    _shortcutViewModel = MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcut(scrpvm);
                }
                return _shortcutViewModel;
            }
        }
        #endregion

        #region State

        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

        //public string KeyString {
        //    get {
        //        if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var param_vm)) {
        //            return param_vm.CurrentValue;
        //        }
        //        return string.Empty;
        //    }
        //    set {
        //        if (KeyString != value) {
        //            ArgLookup[SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID].CurrentValue = value;
        //            HasModelChanged = true;
        //            OnPropertyChanged(nameof(KeyString));
        //        }
        //    }
        //}

        //public int ShortcutId {
        //    get {
        //        if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var param_vm) &&
        //            param_vm.IntValue is int curVal) {
        //            return curVal;
        //        }
        //        return 0;
        //    }
        //    set {
        //        if (ShortcutId != value) {
        //            ArgLookup[SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID].IntValue = value;
        //            HasModelChanged = true;
        //            OnPropertyChanged(nameof(ShortcutId));
        //        }
        //    }
        //}
        //public int ShortcutId { get; private set; }
        public int ShortcutId =>
            ShortcutViewModel == null ? 0 : ShortcutViewModel.ShortcutId;
        #endregion

        #endregion

        #region Constructors

        public MpAvShortcutTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvShortcutTriggerViewModel_PropertyChanged;
            ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut s &&
                //ShortcutId == 0 && 
                s.ShortcutType == MpShortcutType.InvokeTrigger && s.CommandParameter == ActionId.ToString()) {
                //ShortcutId = s.Id;
                OnPropertyChanged(nameof(ShortcutViewModel));

                if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    Dispatcher.UIThread.Post(async () => {
                        await scrpvm.InitializeAsync(scrpvm.PresetValueModel);
                    });
                }
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut s && s.Id == ShortcutId) {
                if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    Dispatcher.UIThread.Post(async () => {
                        await scrpvm.InitializeAsync(scrpvm.PresetValueModel);
                    });
                }
            }
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut s && s.Id == ShortcutId) {
                // should only be able to occur from settings menu? maybe shouldn't allow
                //ShortcutId = 0;
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            //if (!IsValid) {
            //    return;
            //}
            //if (ShortcutViewModel == null) {
            //    ValidationText = $"Shortcut for Trigger '{FullName}' not found";
            //} else {

            //    ValidationText = string.Empty;
            //}
            //if (!IsValid) {
            //    ShowValidationNotification();
            //}
        }

        protected override void EnableTrigger() {
            if (ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.RegisterActionComponent(this);

            // NOTE nothing to enable
            // invoke is handled by shortcut listener and invokeCmd.CanExec
        }

        protected override void DisableTrigger() {
            if (ShortcutViewModel == null) {
                return;
            }
            ShortcutViewModel.UnregisterActionComponent(this);
        }

        protected override bool ValidateStartAction(object arg) {
            if (!base.ValidateStartAction(arg)) {
                return false;
            }
            if (ShortcutViewModel == null) {
                return false;
            }
            // NOTE only perform action if trigger is attachd to shortcut if not its disabled
            return ShortcutViewModel.HasInvoker;
        }

        #endregion

        #region Private Methods


        private void MpAvShortcutTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                        //ShortcutId = MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutId(scrpvm);
                        scrpvm.OnPropertyChanged(nameof(scrpvm.KeyGroups));
                        scrpvm.OnPropertyChanged(nameof(scrpvm.KeyString));
                    }
                    OnPropertyChanged(nameof(ShortcutViewModel));
                    break;
            }
        }

        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvParameterViewModelBase pvm in e.NewItems) {
                    if (pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                        scrpvm.ShortcutCommand = Parent.InvokeActionCommand;
                        scrpvm.ShortcutCommandParameter = ActionId;
                        scrpvm.ShortcutType = MpShortcutType.InvokeTrigger;

                        scrpvm.OnPropertyChanged(nameof(scrpvm.KeyString));
                        scrpvm.OnPropertyChanged(nameof(scrpvm.KeyGroups));
                    }
                }
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}
