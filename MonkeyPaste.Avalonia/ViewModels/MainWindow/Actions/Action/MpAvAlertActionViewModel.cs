using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAlertActionViewModel :
        MpAvActionViewModelBase {

        #region Private Variables
        private bool _isPreviewing = false;
        #endregion

        #region Constants

        public const string IS_AUDIBLE_ALERT_PARAM_ID = "IsAudibleAlert";
        public const string SOUND_TYPE_PARAM_ID = "SoundResourceKey";
        public const string SOUND_VOLUME_PARAM_ID = "SoundVolumeVal";
        public const string IS_TOAST_ALERT_PARAM_ID = "IsToastAlert";
        public const string TOAST_MSG_PARAM_ID = "ToastMsgStr";
        public const string TOAST_SHOW_TIME_SECONDS_PARAM_ID = "ToastShowTimeSeconds";
        public const string TEST_ALERT_PARAM_ID = "TestAlertCmd";

        #endregion

        #region MpIParameterHost Overrides

        private MpHeadlessComponent _actionComponentFormat;
        public override MpHeadlessComponent ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessComponent() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertShowLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = true,
                                paramId = IS_TOAST_ALERT_PARAM_ID,
                                values = new List<MpParameterValueFormat>() {
                                    new MpParameterValueFormat() {
                                        isDefault = true,
                                        value ="True"
                                    }
                                }
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertMessageLabel,
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                paramId = TOAST_MSG_PARAM_ID
                            },

                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertShowTimeLabel,
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                minimum = 0,
                                maximum = 10,
                                paramId = TOAST_SHOW_TIME_SECONDS_PARAM_ID,
                                description = UiStrings.ActionAlertShowTimeHint,
                                values = new List<MpParameterValueFormat>() {
                                    new MpParameterValueFormat() {
                                        isDefault = true,
                                        value ="3"
                                    }
                                }
                            },
                             new MpParameterFormat() {
                                label = UiStrings.ActionAlertSoundLabel,
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                isRequired = true,
                                paramId = IS_AUDIBLE_ALERT_PARAM_ID,
                                value = new MpParameterValueFormat(false.ToString(),true)
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertSoundTypeLabel,
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                paramId = SOUND_TYPE_PARAM_ID,
                                values = new List<MpParameterValueFormat>() {
                                    new MpParameterValueFormat() {
                                        isDefault = true,
                                        label = UiStrings.ActionAlertMonkeySound,
                                        value ="MonkeySound"
                                    },
                                    new MpParameterValueFormat() {
                                        label = UiStrings.ActionAlertTingSound,
                                        value ="TingSound"
                                    },
                                    new MpParameterValueFormat() {
                                        label = UiStrings.ActionAlertChimeSound,
                                        value ="ChimeSound"
                                    },
                                    new MpParameterValueFormat() {
                                        label = UiStrings.ActionAlertAlertSound,
                                        value ="AlertSound"
                                    },
                                    new MpParameterValueFormat() {
                                        label = UiStrings.ActionAlertBlipSound,
                                        value ="BlipSound"
                                    },
                                    new MpParameterValueFormat() {
                                        label = UiStrings.ActionAlertSonarSound,
                                        value ="SonarSound"
                                    },
                                }
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertVolumeLabel,
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Decimal,
                                paramId = SOUND_VOLUME_PARAM_ID,
                                values = new List<MpParameterValueFormat>() {
                                    new MpParameterValueFormat() {
                                        isDefault = true,
                                        value ="1.0"
                                    }
                                }
                            },
                            new MpParameterFormat() {
                                label = UiStrings.ActionAlertPreviewLabel,
                                controlType = MpParameterControlType.Button,
                                paramId = TEST_ALERT_PARAM_ID,
                                values = new List<MpParameterValueFormat>() {
                                    new MpParameterValueFormat() {
                                        isDefault = true,
                                        value = TEST_ALERT_PARAM_ID
                                    }
                                }
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionAlertHint;

        #endregion

        #region State
        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

        public bool IsAudible {
            get {
                if (ArgLookup.TryGetValue(IS_AUDIBLE_ALERT_PARAM_ID, out var param_vm)) {
                    return param_vm.BoolValue;
                }
                return false;
            }
            set {
                if (IsAudible != value) {
                    ArgLookup[IS_AUDIBLE_ALERT_PARAM_ID].BoolValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsAudible));
                }
            }
        }

        public string SoundResourceKey {
            get {
                if (ArgLookup.TryGetValue(SOUND_TYPE_PARAM_ID, out var param_vm)) {
                    return param_vm.CurrentValue;
                }
                return "MonkeySound";
            }
            set {
                if (SoundResourceKey != value) {
                    ArgLookup[SOUND_TYPE_PARAM_ID].CurrentValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SoundResourceKey));
                }
            }
        }

        public double SoundVolume {
            get {
                if (ArgLookup.TryGetValue(SOUND_VOLUME_PARAM_ID, out var param_vm)) {
                    return param_vm.DoubleValue;
                }
                return 1.0d;
            }
            set {
                if (SoundVolume != value) {
                    ArgLookup[SOUND_VOLUME_PARAM_ID].DoubleValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SoundVolume));
                }
            }
        }
        public bool IsToast {
            get {
                if (ArgLookup.TryGetValue(IS_TOAST_ALERT_PARAM_ID, out var param_vm)) {
                    return param_vm.BoolValue;
                }
                return false;
            }
            set {
                if (IsToast != value) {
                    ArgLookup[IS_TOAST_ALERT_PARAM_ID].BoolValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsToast));
                }
            }
        }

        public string ToastMsg {
            get {
                if (ArgLookup.TryGetValue(TOAST_MSG_PARAM_ID, out var param_vm)) {
                    return param_vm.CurrentValue;
                }
                return string.Empty;
            }
            set {
                if (ToastMsg != value) {
                    ArgLookup[TOAST_MSG_PARAM_ID].CurrentValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ToastMsg));
                }
            }
        }

        public int ToastDelayS {
            get {
                if (ArgLookup.TryGetValue(TOAST_SHOW_TIME_SECONDS_PARAM_ID, out var param_vm)) {
                    return param_vm.IntValue;
                }
                return 3;
            }
            set {
                if (ToastDelayS != value) {
                    ArgLookup[TOAST_SHOW_TIME_SECONDS_PARAM_ID].IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ToastDelayS));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvAlertActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
            ActionArgs.CollectionChanged += ActionArgs_CollectionChanged;
        }

        #endregion

        #region Public Overrides

        protected override async Task PerformActionAsync(object arg) {
            if (!_isPreviewing) {
                if (!ValidateStartAction(arg)) {
                    return;
                }
            }
            var actionInput = GetInput(arg);

            if (IsToast) {
                string evald_msg = ToastMsg;
                if (actionInput != null) {

                    evald_msg = await MpPluginParameterValueEvaluator
                        .GetParameterRequestValueAsync(
                            MpParameterControlType.TextBox,
                            MpParameterValueUnitType.PlainTextContentQuery,
                            ToastMsg,
                            actionInput.CopyItem,
                            new object[] { actionInput, null });
                }

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    title: Label,
                    body: evald_msg,
                    msgType: MpNotificationType.AlertAction,
                    iconSourceObj: IconResourceObj,
                    maxShowTimeMs: ToastDelayS * 1000).FireAndForgetSafeAsync(this);
            }
            if (IsAudible) {
                MpAvSoundPlayerViewModel.Instance
                    .PlayCustomSoundCommand.Execute(new object[] { SoundResourceKey, SoundVolume });
            }

            if (_isPreviewing) {
                return;
            }
            await FinishActionAsync(actionInput);
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void ActionArgs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvParameterViewModelBase pvm in e.NewItems) {
                    if (pvm is MpAvButtonParameterViewModel bpvm) {
                        bpvm.ClickCommand = PreviewAlertCommand;
                    }
                }
            }
        }
        #endregion

        #region Commands

        public ICommand PreviewAlertCommand => new MpCommand(() => {
            _isPreviewing = true;
            InvokeThisActionCommand.Execute(null);
            _isPreviewing = false;

        });
        #endregion
    }
}
