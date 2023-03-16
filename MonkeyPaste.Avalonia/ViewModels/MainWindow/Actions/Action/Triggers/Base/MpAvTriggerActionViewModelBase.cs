using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvTriggerActionViewModelBase :
        MpAvActionViewModelBase,
        MpITriggerPluginComponent {
        #region Private Variables

        private bool _isShowEnabledChangedNotification = false;

        #endregion


        #region MpIParameterHost Overrides

        private MpHeadlessPluginFormat _actionComponentFormat;
        public override MpHeadlessPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                label = "Directory",
                                controlType = MpParameterControlType.DirectoryChooser,
                                unitType = MpParameterValueUnitType.FileSystemPath,
                                isRequired = true,
                                paramId = "1",
                                description = "The directory where input content will be written."
                            },
                            new MpParameterFormat() {
                                label = "Custom Name",
                                controlType = MpParameterControlType.TextBox,
                                unitType = MpParameterValueUnitType.PlainTextContentQuery,
                                isRequired = false,
                                paramId = "2",
                                description = "When left blank, the content will use its title as the file name."
                            },
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Interfaces

        #region MpITriggerPluginComponent Implementation

        //ICommand MpITriggerPluginComponent.InvokeActionCommand =>
        //    new MpAsyncCommand<object>(
        //        async (args) => {
        //            await PerformActionAsync(args);
        //        },
        //        (args) => {
        //            return CanPerformAction(args);
        //        });

        void MpITriggerPluginComponent.EnableTrigger() => EnableTrigger();

        void MpITriggerPluginComponent.DisableTrigger() => DisableTrigger();

        bool? MpITriggerPluginComponent.IsEnabled => IsEnabled;

        #endregion

        #endregion

        #region Properties

        #region View Models

        public override MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Add",
                            IconResourceKey = "AddImage",
                            SubItems =
                                typeof(MpActionType)
                                .EnumerateEnum<MpActionType>()
                                .Where(x=>x != MpActionType.None && x != MpActionType.Trigger)
                                .Select(x =>
                                    new MpMenuItemViewModel() {
                                        Header = x.EnumToLabel(),
                                        IconResourceKey = GetDefaultActionIconResourceKey(x),
                                        Command = AddChildActionCommand,
                                        CommandParameter = x
                                    }).ToList()
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = ToggleEnableOrDisableLabel,
                            IconBorderHexColor = MpSystemColors.Transparent,
                            IconHexStr = IsAllValid ? ToggleEnableOrDisableResourceKey : null,
                            IconResourceKey = IsAllValid ? null : ToggleEnableOrDisableResourceKey,
                            IconCornerRadius = IsAllValid ? 20 : 0,
                            Command = ToggleTriggerEnabledCommand,
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = "Remove",
                            IconResourceKey = "DeleteImage",
                            Command = DeleteThisActionCommand
                        }
                    }
                };
            }
        }
        #endregion

        #region Appearance
        public string EnabledHexColor => MpSystemColors.limegreen; //(MpPlatform.Services.PlatformResource.GetResource("EnabledHighlightBrush") as SolidColorBrush).Color.ToPortableColor().ToHex();
        public string DisabledHexColor => MpSystemColors.Red; //(MpPlatform.Services.PlatformResource.GetResource("DisabledHighlightBrush") as SolidColorBrush).Color.ToPortableColor().ToHex();

        public string CurEnableOrDisableHexColor => IsEnabled.HasValue ? IsEnabled.IsTrue() ? EnabledHexColor : DisabledHexColor : MpSystemColors.Transparent;
        public string ToggleEnableOrDisableResourceKey => IsEnabled.HasValue ? IsEnabled.IsTrue() ? DisabledHexColor : EnabledHexColor : "WarningImage";

        public string ToggleEnableOrDisableLabel => IsEnabled.HasValue ? IsEnabled.IsTrue() ? "Disable" : "Enable" : "Fix Errors";

        public override object IconResourceObj {
            get {
                string resourceKey;
                if (IsValid) {
                    resourceKey = GetDefaultActionIconResourceKey(TriggerType);
                } else {
                    resourceKey = "WarningImage";
                }

                return Mp.Services.PlatformResource.GetResource(resourceKey) as string;
            }
        }

        #endregion

        #region Layout
        #endregion

        #region State

        public bool IsAllValid => IsValid && AllDescendants.All(x => x.IsValid);
        #endregion

        #region Model       


        // Arg1 (DesignerProps in Parent)

        // Arg2 
        public bool? IsEnabled {
            get => string.IsNullOrEmpty(Arg2) ? null : bool.Parse(Arg2);
            set {
                var newVal = value == null ? null : value.ToString();
                if (Arg2 != newVal) {
                    Arg2 = newVal;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }

        }

        public MpTriggerType TriggerType {
            get {
                if (Action == null || string.IsNullOrEmpty(Arg3)) {
                    return 0;
                }
                return (MpTriggerType)int.Parse(Arg3);
            }
            set {
                if (TriggerType != value) {
                    Arg3 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TriggerType));
                }
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvTriggerActionViewModelBase(MpAvTriggerCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvTriggerActionViewModelBase_PropertyChanged;
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected abstract void EnableTrigger();
        protected abstract void DisableTrigger();


        protected virtual async Task ShowUserEnableChangeNotification() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (_isShowEnabledChangedNotification) {
                    // BUG not sure why but IsEnable change gets fired twice so this avoids 2 notifications
                    // maybe cause it depends on base Arg2? dunno tried to fix, sorry
                    return;
                }

                _isShowEnabledChangedNotification = true;

                string enabledText = IsEnabled.IsTrue() ? "ENABLED" : "DISABLED";
                string typeStr = ParentActionId == 0 ? "Trigger" : "Action";
                string notificationText = $"{typeStr} '{FullName}' is now  {enabledText}";

                await MpNotificationBuilder.ShowMessageAsync(
                    iconSourceObj: IconResourceObj.ToString(),
                    title: $"{typeStr.ToUpper()} STATUS",
                    body: notificationText);

                _isShowEnabledChangedNotification = false;

            });
        }
        #endregion

        #region Private Methods

        private void MpAvTriggerActionViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsEnabled):
                    ShowUserEnableChangeNotification().FireAndForgetSafeAsync(this);
                    SelfAndAllDescendants.ForEach(x => x.OnPropertyChanged(nameof(x.IsTriggerEnabled)));

                    OnPropertyChanged(nameof(CurEnableOrDisableHexColor));
                    OnPropertyChanged(nameof(ToggleEnableOrDisableResourceKey));
                    OnPropertyChanged(nameof(ToggleEnableOrDisableLabel));
                    OnPropertyChanged(nameof(IsTriggerEnabled));
                    break;
                case nameof(IsAllValid):
                    if (IsAllValid && IsEnabled.IsNull()) {
                        // when trigger is all valid set to disabled state 
                        // to allow enabling
                        IsEnabled = false;
                    } else if (!IsAllValid) {
                        // IsEnabled null forces trigger to be valid before enabling
                        IsEnabled = null;
                    }
                    break;
                case nameof(Children):
                    OnPropertyChanged(nameof(SelfAndAllDescendants));
                    break;
                case nameof(IsBusy):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
            }
        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(SelfAndAllDescendants));
        }

        private void EnableOrDisableTrigger(bool isEnabling) {
            if (isEnabling) {
                EnableTrigger();
            } else {
                DisableTrigger();
            }
            IsEnabled = isEnabling;
        }

        #endregion

        #region Commands

        public ICommand EnableTriggerCommand => new MpAsyncCommand(
            async () => {
                await ValidateActionAsync();
                if (!IsValid) {
                    return;
                }
                EnableOrDisableTrigger(true);
            }, () => {
                return IsEnabled.IsFalse();
            });

        public ICommand DisableTriggerCommand => new MpAsyncCommand(
            async () => {
                while (IsAnyBusy) {
                    await Task.Delay(100);
                }
                EnableOrDisableTrigger(false);
            }, () => {
                return IsEnabled.IsTrue();
            });

        public ICommand ToggleTriggerEnabledCommand => new MpCommand(
             () => {
                 if (IsEnabled.IsTrue()) {
                     DisableTriggerCommand.Execute(null);
                     return;
                 }
                 if (IsEnabled.IsFalse()) {
                     EnableTriggerCommand.Execute(null);
                     return;
                 }
             }, () => {
                 if (IsEnabled.IsTrue()) {
                     return DisableTriggerCommand.CanExecute(null);
                 }
                 if (IsEnabled.IsFalse()) {
                     return EnableTriggerCommand.CanExecute(null);
                 }
                 return false;
             });

        public ICommand DeleteThisTriggerCommand => new MpAsyncCommand<object>(
            async (args) => {
                bool confirmed =
                    await Mp.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync(
                        title: $"Confirm",
                        message: $"Are you sure you want to delete '{Label}' and all associated actions?",
                        iconResourceObj: "WarningImage",
                        anchor: args as Control);
                if (!confirmed) {
                    return;
                }

                Parent.DeleteActionCommand.Execute(this);
            }, (args) => {
                return Parent != null;
            });

        public ICommand DuplicateTriggerCommand => new MpAsyncCommand(
            async () => {
                var dup_trigger = await Action.CloneDbModelAsync();
                dup_trigger.Label = Parent.GetUniqueTriggerName("(Copy) " + dup_trigger.Label);
                await dup_trigger.WriteToDatabaseAsync();
                await Parent.CreateTriggerViewModel(dup_trigger);
            }, () => {
                return Action != null && Parent != null;
            });
        #endregion
    }
}
