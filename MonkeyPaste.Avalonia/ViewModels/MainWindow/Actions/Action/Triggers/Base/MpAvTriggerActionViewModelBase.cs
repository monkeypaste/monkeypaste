
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia.Threading;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvTriggerActionViewModelBase : MpAvActionViewModelBase {
        #region Private Variables

        private bool _isShowEnabledChangedNotification = false;

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
                                        IconResourceKey = GetDefaultActionIconResourceKey(x,null),
                                        Command = AddChildActionCommand,
                                        CommandParameter = x
                                    }).ToList()
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = ToggleEnableOrDisableLabel,
                            BorderHexColor = MpSystemColors.Transparent,
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
        public string EnabledHexColor => (MpPlatformWrapper.Services.PlatformResource.GetResource("EnabledHighlightBrush") as SolidColorBrush).Color.ToPortableColor().ToHex();
        public string DisabledHexColor => (MpPlatformWrapper.Services.PlatformResource.GetResource("DisabledHighlightBrush") as SolidColorBrush).Color.ToPortableColor().ToHex();

        public string CurEnableOrDisableHexColor => IsEnabled.HasValue ? IsEnabled.IsTrue() ? EnabledHexColor : DisabledHexColor : MpSystemColors.Transparent;
        public string ToggleEnableOrDisableResourceKey => IsEnabled.HasValue ? IsEnabled.IsTrue() ? DisabledHexColor : EnabledHexColor : "WarningImage";

        public string ToggleEnableOrDisableLabel => IsEnabled.HasValue ? IsEnabled.IsTrue() ? "Disable" : "Enable" : "Fix Errors";

        public override object IconResourceObj {
            get {
                string resourceKey;
                if (IsValid) {
                    resourceKey = GetDefaultActionIconResourceKey(ActionType, TriggerType);
                } else {
                    resourceKey = "WarningImage";
                }

                return MpPlatformWrapper.Services.PlatformResource.GetResource(resourceKey) as string;
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
                if(Arg2 != newVal) {
                    Arg2 = newVal;
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
                case nameof(IsSelected):
                    //Parent.OnPropertyChanged(nameof(Parent.IsAnySelected));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItemIdx));
                    //OnPropertyChanged(nameof(SelectedItem));
                    break;
                case nameof(IsEnabled):
                    //if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    //    return;
                    //}                    
                    //Dispatcher.UIThread.Post(async () => {
                    //    await ShowUserEnableChangeNotification();
                    //});
                    ShowUserEnableChangeNotification().FireAndForgetSafeAsync(this);
                    SelfAndAllDescendants.ForEach(x => x.OnPropertyChanged(nameof(x.IsTriggerEnabled)));

                    OnPropertyChanged(nameof(CurEnableOrDisableHexColor));
                    OnPropertyChanged(nameof(ToggleEnableOrDisableResourceKey));
                    OnPropertyChanged(nameof(ToggleEnableOrDisableLabel));
                    OnPropertyChanged(nameof(IsTriggerEnabled));
                    break;
                case nameof(IsAllValid):
                    if(IsAllValid && IsEnabled.IsNull()) {
                        IsEnabled = false;
                    } else if(!IsAllValid) {
                        IsEnabled = null;
                    }
                    break;
                case nameof(Children):
                    OnPropertyChanged(nameof(SelfAndAllDescendants));
                    break;
                case nameof(IsBusy):
                    if(Parent != null) {
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

        public ICommand EnableTriggerCommand => new MpCommand(
            () => {
                EnableOrDisableTrigger(true);
            }, () => {
                return IsEnabled.IsFalse();
            });

        public ICommand DisableTriggerCommand => new MpCommand(
            () => {
                EnableOrDisableTrigger(false);
            }, () => {
                return IsEnabled.IsTrue();
            });

        public ICommand ToggleTriggerEnabledCommand => new MpCommand(
             () => {
                IsBusy = true;

                EnableOrDisableTrigger(!IsEnabled.IsTrue());

                IsBusy = false;
            }, () => {
                return IsEnabled.HasValue;
            });

        #endregion
    }
}
