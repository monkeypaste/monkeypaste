using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppCommandActionViewModel :
        MpAvActionViewModelBase {
        #region Private Variables

        #endregion

        #region Constants

        public const string SELECTED_SHORTCUT_ID_PARAM_ID = "SelectedShortcutId";
        public const string ALWAYS_CONTINUE_PARAM_ID = "AlwaysContinue";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionAppCommandLabel,
                                controlType = MpParameterControlType.ComponentPicker,
                                unitType = MpParameterValueUnitType.ApplicationCommandComponentId,
                                isRequired = true,
                                paramId = SELECTED_SHORTCUT_ID_PARAM_ID,
                                description = UiStrings.ActionAppCommandHint
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionAppCommandAlwaysContinueLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = true,
                                paramId = ALWAYS_CONTINUE_PARAM_ID,
                                description = UiStrings.ActionAppCommandAlwaysContinueHint,
                                value = new MpPluginParameterValueFormat(true.ToString(),true)
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

        public MpAvShortcutViewModel SelectedShortcut =>
            MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId);

        #endregion

        #region State
        public override bool AllowNullArg => true;
        #endregion

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionAppCommandActionHint;

        #endregion

        #region Model

        public int ShortcutId {
            get {
                if (ArgLookup.TryGetValue(SELECTED_SHORTCUT_ID_PARAM_ID, out var param_vm) &&
                    param_vm.IntValue is int curVal) {
                    return curVal;
                }
                return 0;
            }
            set {
                if (ShortcutId != value) {
                    ArgLookup[SELECTED_SHORTCUT_ID_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ShortcutId));
                }
            }
        }
        public bool AlwaysContinue {
            get {
                if (ArgLookup.TryGetValue(SELECTED_SHORTCUT_ID_PARAM_ID, out var param_vm) &&
                    param_vm.BoolValue is bool curVal) {
                    return curVal;
                }
                return true;
            }
            set {
                if (AlwaysContinue != value) {
                    ArgLookup[SELECTED_SHORTCUT_ID_PARAM_ID].BoolValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AlwaysContinue));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvAppCommandActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvAppCommandActionViewModel_PropertyChanged;
        }


        #endregion

        #region Protected Overrides
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is not MpShortcut) {
                return;
            }
            if (ArgLookup.TryGetValue(SELECTED_SHORTCUT_ID_PARAM_ID, out var pvmb)) {
                pvmb.InitializeAsync(pvmb.PresetValueModel).FireAndForgetSafeAsync();
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is not MpShortcut) {
            }
            if (ArgLookup.TryGetValue(SELECTED_SHORTCUT_ID_PARAM_ID, out var pvmb)) {
                pvmb.InitializeAsync(pvmb.PresetValueModel).FireAndForgetSafeAsync();
            }
        }
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut t && t.Id == ShortcutId) {
                Dispatcher.UIThread.Post(async () => {
                    await ValidateActionAsync();
                });
            }
        }
        protected override async Task ValidateActionAsync() {
            await base.ValidateActionAsync();
            if (!IsValid) {
                return;
            }
            if (ShortcutId == 0) {
                //ValidationText = $"No Collection selected for Classifier '{FullName}'";
                ValidationText = string.Format(UiStrings.ActionAppCommandValidation1, FullName);
            } else {
                if (SelectedShortcut == null) {
                    //ValidationText = $"Collection for Classifier '{FullName}' not found";
                    ValidationText = string.Format(UiStrings.ActionAppCommandValidation2, FullName);
                } else {
                    ValidationText = string.Empty;
                }
            }
            if (!IsValid) {
                ShowValidationNotification();
            }
        }
        protected override void Param_vm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.Param_vm_PropertyChanged(sender, e);
            if (sender is not MpAvComponentPickerParameterViewModel cppvm) {
                return;
            }
            switch (e.PropertyName) {
                case nameof(cppvm.ComponentId):
                    ShortcutId = cppvm.ComponentId;
                    break;
            }
        }

        protected override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            bool did_execute = false;
            if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ShortcutId) is { } scvm) {
                did_execute = scvm.PerformShortcutCommand.CanExecute(scvm.CommandParameter);
                if (scvm.PerformShortcutCommand is MpIAsyncCommand<object> async_cmd) {
                    await async_cmd.ExecuteAsync(scvm.CommandParameter);
                } else {
                    scvm.PerformShortcutCommand.Execute(scvm.CommandParameter);
                }
            }

            await FinishActionAsync(new MpAvAppCommandOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem,
                ShortcutId = ShortcutId,
                CanExecutionContinue = AlwaysContinue || did_execute

            });
        }

        #endregion

        #region Private Methods


        private void MpAvAppCommandActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ActionArgs):
                    OnPropertyChanged(nameof(SelectedShortcut));
                    break;
                case nameof(SelectedShortcut):
                    OnPropertyChanged(nameof(IconResourceObj));
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
