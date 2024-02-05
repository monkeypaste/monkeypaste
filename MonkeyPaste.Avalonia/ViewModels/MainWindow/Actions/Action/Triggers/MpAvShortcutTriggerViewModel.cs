using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvShortcutTriggerViewModel :
        MpAvTriggerActionViewModelBase {

        #region Constants

        const string SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID = "ShortcutTriggerKeyString";

        #endregion

        #region Interfaces
        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionShortcutTriggerLabel,
                                controlType = MpParameterControlType.ShortcutRecorder,
                                unitType = MpParameterValueUnitType.PlainText,
                                paramId = SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID,
                                description = UiStrings.ActionShortcutTriggerTriggerHint
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

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionShortcutTriggerHint;

        #endregion
        #region State
        protected override MpIActionComponent TriggerComponent =>
            ShortcutViewModel;

        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

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

        #region Db Handlers
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut s &&
                //ShortcutId == 0 && 
                s.ShortcutType == MpShortcutType.InvokeTrigger && s.CommandParameter == ActionId.ToString()) {
                //ShortcutId = s.Id;
                OnPropertyChanged(nameof(ShortcutViewModel));

                if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    Dispatcher.UIThread.Post(async () => {
                        await scrpvm.InitializeAsync(scrpvm.ParameterValue);
                    });
                }
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut s && s.Id == ShortcutId) {
                if (ArgLookup.TryGetValue(SHORTCUT_TRIGGER_KEYSTRING_PARAM_ID, out var pvm) && pvm is MpAvShortcutRecorderParameterViewModel scrpvm) {
                    Dispatcher.UIThread.Post(async () => {
                        await scrpvm.InitializeAsync(scrpvm.ParameterValue);
                    });
                }
            }
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut s && s.Id == ShortcutId) {
                // should only be able to occur from settings menu? maybe shouldn't allow
                //ShortcutId = 0;
                Task.Run(ValidateActionAndDescendantsAsync);
            }
        }

        #endregion

        protected override void EnableTrigger() {
            if (Parent.IsRestoringEnabled &&
                !Mp.Services.StartupState.IsPlatformLoaded) {
                Dispatcher.UIThread.Post(async () => {
                    while (!Mp.Services.StartupState.IsPlatformLoaded) {
                        await Task.Delay(100);
                    }
                    EnableTrigger();
                });
                return;
            }
            if (ShortcutViewModel == null) {
                return;
            }
            TriggerComponent.RegisterActionComponent(this);

            // NOTE nothing to enable
            // invoke is handled by shortcut listener and invokeCmd.CanExec
        }

        //protected override void Param_vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        //    base.Param_vm_PropertyChanged(sender, e);
        //    if (sender is MpAvShortcutRecorderParameterViewModel scrpvm) {
        //        switch (e.PropertyName) {
        //            case nameof(scrpvm.KeyString):
        //                scrpvm.InitializeAsync(scrpvm.PresetValueModel).FireAndForgetSafeAsync();
        //                break;
        //        }
        //    }
        //}
        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                await FinishActionAsync(arg);
                return;
            }
            MpAvActionOutput input = GetInput(arg) ?? new MpAvTriggerInput();
            if (input.CopyItem == null) {
                MpCopyItem input_ci = null;
                var sctvm = MpAvClipTrayViewModel.Instance.SelectedItem;
                if (sctvm == null || !sctvm.IsFocusWithin) {
                    if (Mp.Services.ClipboardMonitor.LastClipboardDataObject is { } lcdo) {
                        input_ci = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(lcdo, false, MpDataObjectSourceType.ShortcutTrigger);

                    }
                } else if (sctvm != null) {
                    input_ci = sctvm.CopyItem;
                }

                input.CopyItem = input_ci;
            }
            await FinishActionAsync(input);
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
