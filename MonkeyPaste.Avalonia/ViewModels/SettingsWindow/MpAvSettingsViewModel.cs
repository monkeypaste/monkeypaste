using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables

        private string[] _restartContentParams => new string[] {
            nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
            nameof(MpAvPrefViewModel.Instance.CurrentCultureCode),
            //nameof(MpAvPrefViewModel.Instance.ShowInTaskSwitcher),
            nameof(MpAvPrefViewModel.Instance.IsLoggingEnabled),
        };

        private string[] _reinitContentParams => new string[] {
            nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
            nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
            nameof(MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled),
            nameof(MpAvPrefViewModel.Instance.DefaultFontSize),
            nameof(MpAvPrefViewModel.Instance.IsSpellCheckEnabled),
            nameof(MpAvPrefViewModel.Instance.ThemeTypeName),
            nameof(MpAvPrefViewModel.Instance.ThemeColor),
            nameof(MpAvPrefViewModel.Instance.GlobalBgOpacity),
            nameof(MpAvPrefViewModel.Instance.CurrentCultureCode),
        };

        public string[] HiddenParamIds => new string[] {
            nameof(MpAvPrefViewModel.Instance.NotificationSoundGroupIdx),
            nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup),
            MpRuntimePrefParamType.ChangeRoutingType.ToString(),
            //nameof(MpAvPrefViewModel.Instance.UserDefinedFileExtensionsCsv)
            //nameof(MpAvPrefViewModel.Instance.CurrentCultureCode),
            //nameof(MpAvPrefViewModel.Instance.IsTextRightToLeft)

        };

        private Dictionary<object, Action<MpAvPluginParameterItemView>> _runtimeParamAttachActions;
        #endregion

        #region CONSTANTS

        #endregion

        #region Statics

        private static MpAvSettingsViewModel _instance;
        public static MpAvSettingsViewModel Instance => _instance ?? (_instance = new MpAvSettingsViewModel());


        #endregion

        #region Interfaces

        #region MpIWindowViewModel Implementatiosn
        public MpWindowType WindowType =>
            MpWindowType.Settings;

        public bool IsWindowOpen { get; set; }

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation 
        public bool WantsTopmost =>
            true;

        #endregion

        #region MpIActiveWindowViewModel Implementation
        public bool IsWindowActive { get; set; }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvSettingsFrameViewModel> Items =>
            TabLookup == null ? null : TabLookup.SelectMany(x => x.Value);

        public Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>> TabLookup { get; set; }
        public Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>> FilteredTabLookup =>
            TabLookup == null ? null : TabLookup.ToDictionary(x => x.Key, x => x.Value.Where(x => x.FilteredItems != null && x.FilteredItems.Any()));

        public IEnumerable<MpAvSettingsFrameViewModel> FilteredAccountFrames =>
            FilteredTabLookup == null ? null : FilteredTabLookup[MpSettingsTabType.Account];

        public IEnumerable<MpAvSettingsFrameViewModel> FilteredPreferenceFrames =>
             FilteredTabLookup == null ? null : FilteredTabLookup[MpSettingsTabType.Preferences];
        public MpAvSettingsFrameViewModel SelectedItem {
            get =>
                Items
                    .Where(x => x.IsSelected)
                    .OrderByDescending(x => x.LastSelectedDateTime)
                    .FirstOrDefault();
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsSelected = x == value);
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public MpAvParameterViewModelBase SelectedSetting {
            get => SelectedItem == null ? null : SelectedItem.SelectedItem;
            set {
                if (SelectedSetting != value) {
                    SelectedItem = Items.FirstOrDefault(x => x.Items.Contains(value));
                    if (SelectedItem != null) {
                        SelectedItem.SelectedItem = value;
                    }
                    OnPropertyChanged(nameof(SelectedSetting));
                }
            }
        }


        public IList<string> RecentSettingsSearches { get; private set; }
        #endregion

        #region State

        public MpSettingsTabType DefaultSelectedTab {
            get {
                if (string.IsNullOrEmpty(MpAvPrefViewModel.Instance.LastSelectedSettingsTabTypeStr)) {
                    // NOTE below should be added back in when sync implemented
                    //if (MpAvAccountViewModel.Instance.IsFree) {
                    //    return MpSettingsTabType.Account;
                    //}
                    return MpSettingsTabType.Preferences;
                }
                return MpAvPrefViewModel.Instance.LastSelectedSettingsTabTypeStr.ToEnum<MpSettingsTabType>();
            }
        }
        public bool IsTabButtonVisible0 { get; set; } = true;
        public bool IsTabButtonVisible1 { get; set; } = true;
        public bool IsTabButtonVisible2 { get; set; } = true;
        public bool IsTabButtonVisible3 { get; set; } = true;
        public bool IsPrefTabSelected =>
           (MpSettingsTabType)SelectedTabIdx == MpSettingsTabType.Preferences;

        public string FilterText { get; set; } = string.Empty;

        public bool IsBatchUpdate { get; set; }

        public int SelectedTabIdx {
            get {
                for (int i = 0; i < IsTabSelected.Count; i++) {
                    if (IsTabSelected[i]) {
                        return i;
                    }

                }
                return -1;
            }
            set {
                if (SelectedTabIdx != value) {
                    for (int i = 0; i < IsTabSelected.Count; i++) {
                        IsTabSelected[i] = i == value;
                    }
                    OnPropertyChanged(nameof(SelectedTabIdx));
                }
            }
        }

        public bool IsLoginOnly =>
            MpAvWelcomeNotificationViewModel.Instance.IsWindowOpen;

        public ObservableCollection<bool> IsTabSelected { get; set; }

        #endregion

        #region Appearance

        public GridLength AccountColumnWidth =>
            IsLoginOnly || !MpAvAccountViewModel.Instance.IsUserViewEnabled ?
                new GridLength(1, GridUnitType.Star) :
                new GridLength(0.5, GridUnitType.Star);

        public GridLength SubscriptionColumnWidth =>
            IsLoginOnly || !MpAvAccountViewModel.Instance.IsUserViewEnabled ?
                new GridLength(0) :
                new GridLength(0.5, GridUnitType.Star);
        #endregion

        #endregion

        #region Constructors

        public MpAvSettingsViewModel() : base() {
            MpAvPrefViewModel.Instance.PropertyChanged += MpPrefViewModel_Instance_PropertyChanged;
            PropertyChanged += MpAvSettingsWindowViewModel_PropertyChanged;

            IsTabSelected = new ObservableCollection<bool>(Enumerable.Repeat(false, 6));
            IsTabSelected.CollectionChanged += IsTabSelected_CollectionChanged;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }



        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await InitSettingFramesAsync();
            UpdateFilters();
            InitRuntimeParams();
        }
        #endregion

        #region Private Methods
        private async Task InitSettingFramesAsync() {
            TabLookup = new Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>>() {
                {
                    MpSettingsTabType.Account,
                    new [] {
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Status) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountEmail),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = UiStrings.AccountEmailLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountEmail)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountUsername),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = UiStrings.AccountUserNameLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountUsername)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountTypeStr),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = UiStrings.AccountSubscriptionLabel,
                                            value = new MpParameterValueFormat(
                                                val: MpAvPrefViewModel.Instance.AccountType.ToString(),
                                                label: MpAvPrefViewModel.Instance.AccountType.EnumToUiString())
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountBillingCycleTypeStr),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = UiStrings.AccountBillingCycleLabel,
                                            value = new MpParameterValueFormat(
                                                val: MpAvPrefViewModel.Instance.AccountBillingCycleType.ToString(),
                                                label: MpAvPrefViewModel.Instance.AccountBillingCycleType.EnumToUiString())
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountNextPaymentDateTime),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = UiStrings.AccountNextPaymentLabel,
                                            value = new MpParameterValueFormat(
                                                MpAvPrefViewModel.Instance.AccountBillingCycleType == MpBillingCycleType.Never ?
                                                    MpBillingCycleType.Never.EnumToUiString() :
                                                    MpAvPrefViewModel.Instance.AccountNextPaymentDateTime.ToString(MpAvCurrentCultureViewModel.Instance.DateFormat))
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountLogout.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            isVisible =
#if DEBUG
                                            true,
#else
                                            false,
#endif
                                            value = new MpParameterValueFormat(MpRuntimePrefParamType.AccountLogout.ToString(),UiStrings.AccountLogoutButtonText)
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Register) {
                            FrameHint = UiStrings.PrefRegistrationFrameHint,
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountShowLogin.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            value = new MpParameterValueFormat() {
                                                label = UiStrings.AccountExistingAccountButtonLabel}
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountEmail),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.AccountEmailLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountEmail)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountUsername),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.AccountUserNameLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountUsername)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountPassword),
                                            controlType = MpParameterControlType.PasswordBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.AccountPasswordLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountPassword)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountPassword2),
                                            controlType = MpParameterControlType.PasswordBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.CommonConfirmLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountPassword2)
                                        },

                                       new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountShowPrivacyPolicy.ToString(),
                                            controlType = MpParameterControlType.Hyperlink,
                                            value = new MpParameterValueFormat(MpRuntimePrefParamType.AccountShowPrivacyPolicy.ToString(),UiStrings.PrivacyPolicyLabel)
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountRegister.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            value = new MpParameterValueFormat() {
                                                label = UiStrings.AccountRegisterButtonText
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Login) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountShowRegister.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            value = new MpParameterValueFormat() {
                                                label = UiStrings.AccountSwitchToRegisterButtonLabel
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountUsername),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.AccountUserNameLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountUsername)
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountPassword),
                                            controlType = MpParameterControlType.PasswordBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.AccountPasswordLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.AccountPassword)
                                        },
                                       new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountResetPassword.ToString(),
                                            controlType = MpParameterControlType.Hyperlink,
                                            value = new MpParameterValueFormat(MpRuntimePrefParamType.AccountResetPassword.ToString(), UiStrings.AccountForgotPwdButtonLabel)
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.AccountLogin.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            value = new MpParameterValueFormat(MpRuntimePrefParamType.AccountLogin.ToString(),UiStrings.AccountLoginButtonLabel)
                                        }
                                    }
                                }
                            }
                        },
                    }

                },
                {
                    MpSettingsTabType.Preferences,
                    new[] {
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Theme) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ThemeTypeName),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefThemeStyleLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Light,
                                                    value = MpThemeType.Light.ToString(),
                                                    label = MpThemeType.Light.EnumToUiString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Dark,
                                                    value = MpThemeType.Dark.ToString(),
                                                    label = MpThemeType.Dark.EnumToUiString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ThemeHexColor.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefThemeColorLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.PrefThemeColorHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.CommonSelectLabel,
                                                    value = MpRuntimePrefParamType.ThemeHexColor.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.DefaultFonts) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefUiFontLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.CommonRequiresRestartHint,
                                            values =
                                                FontManager.Current.SystemFonts
                                                .Select(x=>x.Name)
                                                .Where(x=>!string.IsNullOrEmpty(x))
                                                .Union(MpAvThemeViewModel.Instance.CustomFontFamilyNames)
                                                .OrderBy(x=>x)
                                                .Select(x=>new MpParameterValueFormat() {
                                                    isDefault = MpAvThemeViewModel.Instance.DefaultReadOnlyFontFamily.ToLowerInvariant() == x.ToLowerInvariant(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefContentFontLabel,
                                            values =
                                                FontManager.Current.SystemFonts
                                                .Select(x=>x.Name)
                                                .Where(x=>!string.IsNullOrEmpty(x))
                                                .Union(MpAvThemeViewModel.Instance.CustomFontFamilyNames)
                                                .OrderBy(x=>x)
                                                .Select(x=>new MpParameterValueFormat() {
                                                    isDefault = MpAvThemeViewModel.Instance.DefaultEditableFontFamily.ToLowerInvariant() == x.ToLowerInvariant(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultFontSize),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = UiStrings.PrefContentFontSizeLabel,
                                            values =
                                                new int[]{ 8, 9, 10, 12, 14, 16, 20, 24, 32, 42, 54, 68, 84, 98 }
                                                .Select(x=>new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.DefaultFontSize == x,
                                                    value = x.ToString(),
                                                }).ToList()
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Sound) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.NotificationSoundGroupIdx),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = UiStrings.PrefSoundThemeLabel,
                                            values =
                                                new int[]{ 0, 1, 2, 3 }
                                                .Select(x=>new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.NotificationSoundGroupIdx == x,
                                                    label = ((MpSoundGroupType)x).ToString(),
                                                    value = x.ToString(),
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.NotificationSoundVolume),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = UiStrings.PrefNtfVolumeLabel,
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.NotificationSoundVolume.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Window) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MainWindowShowBehaviorTypeStr),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefMainWindowShowBehaviorLabel,
                                            values =
                                                Enum.GetNames(typeof(MpMainWindowShowBehaviorType))
                                                .Select(x=> new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MainWindowShowBehaviorTypeStr.ToLowerInvariant() == x.ToLowerInvariant(),
                                                    value = x,
                                                    label = x.ToEnum<MpMainWindowShowBehaviorType>().EnumToUiString()
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowInTaskbar),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefShowInTaskbarLabel,
                                            description = UiStrings.PrefShowInTaskbarHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowInTaskbar.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AnimateMainWindow),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefMainWindowAnimateLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.AnimateMainWindow.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.GlobalBgOpacity),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = UiStrings.PrefMainWindowOpacityLabel,
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.GlobalBgOpacity.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Hints) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowTooltips),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefShowTooltipsLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowTooltips.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowHints),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefShowHintsLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowHints.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.HideCapWarnings),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefHideWatermarksLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.HideCapWarnings.ToString())
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.International) {
                            SortOrderIdx = int.MinValue,
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.CurrentCultureCode),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefLanguageLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.CommonRequiresRestartHint,
                                            values =
                                                MpAvCurrentCultureViewModel.Instance.LangLookup
                                                .Select(x=>
                                                new MpParameterValueFormat() {
                                                    isDefault = x.Key == MpAvPrefViewModel.Instance.CurrentCultureCode,
                                                    label = x.Value,
                                                    value = x.Key
                                                }).ToList()
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Limits) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxUndoLimit),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefUndoLimitLabel,
                                            description = UiStrings.PrefUndoLimitHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "100",
                                                    value = "100".ToString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "-1",
                                                    value = "-1".ToString(),
                                                    label = UiStrings.PrefUndoLimitNoLimitLabel
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxPinClipCount),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Integer,
                                            minimum = 1,
                                            maximum = 50,
                                            label = UiStrings.PrefPinTrayCapacityLabel,
                                            description = UiStrings.PrefPinTrayCapacityHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.MaxPinClipCount.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Tracking) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreAppendedItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefIgnoreAppendsLabel,
                                            description = UiStrings.PrefIgnoreAppendsHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreAppendedItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefIgnoreInternalClipboardLabel,
                                            description = UiStrings.PrefIgnoreInternalClipboardHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefIgnoreWhiteSpaceLabel,
                                            description = UiStrings.PrefIgnoreWhiteSpaceHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.TrackExternalPasteHistory),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefPasteHistoryLabel,
                                            description = UiStrings.PrefPasteHistoryHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.TrackExternalPasteHistory.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsDuplicateCheckEnabled),
                                            description = UiStrings.PrefIgnoreDupHint,
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefIgnoreDupLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsDuplicateCheckEnabled.ToString()
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Startup) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.LoadOnLogin),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefLoginLoadLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.LoadOnLogin.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefStartupClipboardLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.PrefStartupClipboardHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.AddClipboardOnStartup.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.TrashCleanupModeTypeStr),
                                            description = UiStrings.PrefTrashCycleHint,
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefTrashCycleLabel,
                                            values =
                                                Enum.GetNames(typeof(MpTrashCleanupModeType))
                                                .Select(x=> new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.TrashCleanupModeTypeStr.ToLowerInvariant() == x.ToString().ToLowerInvariant(),
                                                    label = x.ToEnum<MpTrashCleanupModeType>().EnumToUiString(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefStartupClipboardListenLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Logs) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsLoggingEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefEnableLoggingLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.PrefEnableLoggingHint,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.IsLoggingEnabled.ToString(),true)
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ShowLogsFolder.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefShowLogsFolderLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.PrefShowLogsFolderBtnText,
                                                    value = MpRuntimePrefParamType.ShowLogsFolder.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Search) {
                            // BUG sorting this so its first to prevent it always scrolling into view on tab change
                            SortOrderIdx = int.MaxValue,
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxRecentTextsCount),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefRecentTextLimitLabel,
                                            description = UiStrings.PrefRecentTextLimitHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "100",
                                                    value = "100".ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsAutoSearchEnabled),
                                            description = UiStrings.PrefAutoSearchHint,
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefAutoSearchLabel,
                                            values =new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsAutoSearchEnabled.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            // TODO this should probably be in a seperate frame but just testing editable list now
                                            paramId = nameof(MpAvPrefViewModel.Instance.UserDefinedFileExtensionsCsv),
                                            controlType = MpParameterControlType.EditableList,
                                            unitType = MpParameterValueUnitType.DelimitedPlainText,
                                            label = UiStrings.PrefUserFileExtLabel,
                                            description = UiStrings.PrefUserFileExtHint,
                                            values =
                                            new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.UserDefinedFileExtensionsCsv
                                                }
                                            }
                                                //MpPrefViewModel.Instance.UserDefinedFileExtensionsCsv
                                                //.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value)
                                                //.Select(x=>
                                                //    new MpPluginParameterValueFormat() {
                                                //        isDefault = true,
                                                //        paramValue = x
                                                //    })
                                                //.ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ClearRecentSearches.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefClearSearchesLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.PrefClearSearchesButtonText,
                                                    value = MpRuntimePrefParamType.ClearRecentSearches.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Content) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsSpellCheckEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefSpellCheckLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsSpellCheckEnabled.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefRichContentLabel,
                                            description = UiStrings.PrefRichContentHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefDestFormatLabel,
                                            description = UiStrings.PrefDestFormatHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefResetClipboardLabel,
                                            description = MpAvToolTipHintView.WARN_PREFIX + UiStrings.PrefResetClipboardHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowContentTitles),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefShowTitlesLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowContentTitles.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.DeleteAllContent.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefDeleteAllLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.PrefDeleteAllButtonText,
                                                    value = MpRuntimePrefParamType.DeleteAllContent.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.TopScreenEdgeGestures) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ScrollToOpen),
                                            description = UiStrings.PrefMainWindowShowOnScrollHint,
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefMainWindowShowOnScrollLabel,
                                            values =new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ScrollToOpen.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ScrollToOpenAndLockTypeStr),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = UiStrings.PrefShowWindowAndLockLabel,
                                            description = UiStrings.PrefShowWindowAndLockHint,
                                            values =
                                                Enum.GetNames(typeof(MpScrollToOpenAndLockType))
                                                .Select((x,idx)=> new MpParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.ScrollToOpenAndLockTypeStr == x,
                                                    value = x,
                                                    label = x.ToEnum<MpScrollToOpenAndLockType>().EnumToUiString(UiStrings.CommonNoneLabel)
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DragToOpen),
                                            description = UiStrings.PrefShowWindowOnDragHint,
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = UiStrings.PrefShowWindowOnDragLabel,
                                            value = new MpParameterValueFormat(MpAvPrefViewModel.Instance.DragToOpen.ToString())
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Shortcuts) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ChangeRoutingType.ToString(),
                                            controlType = MpParameterControlType.ComboBox,
                                            label = UiStrings.PrefGestureProfileLabel,
                                            description = UiStrings.ShortcutRoutingProfileTypeHint,
                                            values =
                                                Enum.GetNames(typeof(MpShortcutRoutingProfileType))
                                                .Select((x,idx)=> new MpParameterValueFormat() {
                                                    isDefault = MpAvShortcutCollectionViewModel.Instance.RoutingProfileType.ToString() == x,
                                                    value = x, // NOTE!!
                                                    label = x.ToEnum<MpShortcutRoutingProfileType>().EnumToUiString()
                                                }).ToList()
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Password) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ChangeDbPassword.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefChangePasswordLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.PrefChangePasswordButtonText,
                                                    value = MpRuntimePrefParamType.ChangeDbPassword.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ClearDbPassword.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefClearPasswordLabel,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.PrefClearPasswordButtonLabel,
                                                    value = MpRuntimePrefParamType.ClearDbPassword.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ForgetDbPassword.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefForgetPasswordLabel,
                                            value = new MpParameterValueFormat() {
                                                label = UiStrings.PrefForgetPasswordButtonLabel
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.System) {
                            PluginFormat = new MpRuntimePlugin() {
                                headless = new MpHeadlessComponent() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetNtf.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefRestorNtfLabel,
                                            description = UiStrings.PrefRestoreNtfHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.CommonResetLabel,
                                                    value = MpRuntimePrefParamType.ResetNtf.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetShortcuts.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefResetShortcutsLabel,
                                            description = UiStrings.PrefResetShortcutsHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.CommonResetLabel,
                                                    value = MpRuntimePrefParamType.ResetShortcuts.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetPluginCache.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = UiStrings.PrefResetPluginCacheLabel,
                                            description = UiStrings.PrefResetPluginCacheHint,
                                            values = new List<MpParameterValueFormat>() {
                                                new MpParameterValueFormat() {
                                                    isDefault = true,
                                                    label = UiStrings.CommonResetLabel,
                                                    value = MpRuntimePrefParamType.ResetPluginCache.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        }
                    }
                    .OrderByDescending(x=>x.SortOrderIdx)
                    .ThenByDescending(x=>x.Items == null ? 0 : x.Items.Where(x=>x.IsVisible).Count())
                }
            };

            // bind runtime params

            foreach (var fvm in Items) {

                // create headless formats
                fvm.Items = await Task.WhenAll(
                    fvm.PluginFormat.headless.parameters.Select(x =>
                        MpAvPluginParameterBuilder.CreateParameterViewModelAsync(
                            new MpParameterValue() {
                                ParamId = x.paramId,
                                Value =
                                x.values.FirstOrDefault(x => x.isDefault) is MpParameterValueFormat ppvf ?
                                    ppvf.value : string.Empty
                            }, fvm)));

                // map button cmds 
                fvm.Items
                    //.Where(x => x.ControlType == MpParameterControlType.Button || x.ControlType == MpParameterControlType.Hyperlink)
                    //.Cast<MpAvButtonParameterViewModel>()
                    .OfType<MpAvButtonParameterViewModel>()
                    .ForEach(x => x.ClickCommand = ButtonParameterClickCommand);


                foreach (var para_item in fvm.Items) {
                    if (HiddenParamIds.Any(x => x == para_item.ParameterFormat.paramId)) {
                        para_item.ParameterFormat.isVisible = false;
                    }
                }

                if (TabLookup.FirstOrDefault(x => x.Value.Contains(fvm)) is { } kvp &&
                    kvp.Key == MpSettingsTabType.Account) {
                    // hide popout button on account textboxes
                    foreach (var sfvm in kvp.Value) {
                        if (sfvm.Items == null) {
                            continue;
                        }
                        foreach (var pivm in sfvm.Items) {
                            if (pivm is MpAvTextBoxParameterViewModel tbpivm) {
                                tbpivm.CanPopOut = false;
                            }
                        }
                    }
                }

            }


        }
        private void InitRuntimeParams() {
            _runtimeParamAttachActions = new Dictionary<object, Action<MpAvPluginParameterItemView>>() {
                {
                    nameof(MpAvPrefViewModel.Instance.CurrentCultureCode),
                    (piv) => {
                        if(piv.GetVisualDescendant<ComboBox>() is not ComboBox cb) {
                            return;
                        }
                        SetupCulturesComboBox(cb);
                    }
                },
                {
                    nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                    (piv) => {
                        if(piv.GetVisualDescendant<ComboBox>() is not ComboBox cb) {
                            return;
                        }
                        SetupFontFamilyComboBox(cb);
                    }
                },
                {
                    nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
                    (piv) => {
                        if(piv.GetVisualDescendant<ComboBox>() is not ComboBox cb) {
                            return;
                        }
                        SetupFontFamilyComboBox(cb);
                    }
                },
                {
                    MpRuntimePrefParamType.ThemeHexColor.ToString(),
                    (piv) => {
                        if(piv.GetVisualDescendant<Button>() is not Button b) {
                            return;
                        }
                        SetThemeButtonColor(b);
                    }
                },
                {
                    MpRuntimePrefParamType.ChangeRoutingType.ToString(),
                    (piv) => {
                        if(piv.GetVisualDescendant<ComboBox>() is not ComboBox cmb) {
                            return;
                        }
                        void RoutingSelectionChanged(object sender, SelectionChangedEventArgs e) {
                            if (cmb.SelectedIndex < 0) {
                                // occurs when filtered out
                                return;
                            }
                            if(cmb.GetSelfAndVisualDescendants().OfType<Control>().All(x=>!x.IsFocused)) {
                                // indirect change, shortcuts reset
                                return;
                            }
                            MpShortcutRoutingProfileType sel_type = (MpShortcutRoutingProfileType)cmb.SelectedIndex;
                            MpAvShortcutCollectionViewModel.Instance.UpdateRoutingProfileCommand.Execute(sel_type);
                        }
                        cmb.SelectionChanged += RoutingSelectionChanged;
                        SetRoutingProfileType(MpAvShortcutCollectionViewModel.Instance.RoutingProfileType);
                    }
                },
                {
                    nameof(MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled),
                    (piv) => {
                        
#if DESKTOP
                        if(piv.GetVisualDescendant<CheckBox>() is not CheckBox cb) {
                            return;
                        }

#if CEFNET_WV
		// ensure rich content cb reflects webview availability
                        cb.IsEnabled = MpAvCefNetApplication.IsCefNetLoaded;    
#endif
#endif
                    }
                }
            };

            MpAvSettingsFrameView.ParamViews.CollectionChanged += ParamViews_CollectionChanged;

            // init bindings (view props not needed)

            ProcessListenOnStartupChanged();
            SetupPasswordButtons();
        }

        private void ParamViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null) {
                foreach (MpAvPluginParameterItemView piv in e.NewItems) {
                    if (piv.DataContext is not MpAvParameterViewModelBase pvmb) {
                        continue;
                    }
                    if (_runtimeParamAttachActions.TryGetValue(pvmb.ParamId, out var setupAction)) {
                        setupAction.Invoke(piv);
                    }
                }
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ShortcutRoutingProfileChanged:
                    SetRoutingProfileType(MpAvShortcutCollectionViewModel.Instance.RoutingProfileType);
                    break;
            }
        }
        private void MpAvSettingsWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {

            switch (e.PropertyName) {
                case nameof(FilterText):
                    MpMessenger.SendGlobal(MpMessageType.SettingsFilterTextChanged);
#if DEBUG
                    if (FilterText.ToStringOrEmpty().StartsWith("#")) {
                        var test = MpAvWindowManager.FindByHashCode(FilterText);
                    }
#endif
                    UpdateFilters();
                    break;
                case nameof(SelectedTabIdx):
                    if (SelectedTabIdx >= 0) {
                        MpAvPrefViewModel.Instance.LastSelectedSettingsTabTypeStr = ((MpSettingsTabType)SelectedTabIdx).ToString();
                    }
                    break;
            }
        }

        private void IsTabSelected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTabSelected));
        }

        private MpAvWindow CreateSettingsWindow() {
            MpAvWindow sw = null;
            if (IsLoginOnly) {
                sw = new MpAvWindow() {
                    ShowInTaskbar = true,
                    MinWidth = 200,
                    MinHeight = 200,
                    Width = 500,
                    Height = 320,
                    CanResize = true,
                    SizeToContent = SizeToContent.Manual,
                    Title = UiStrings.AccountLoginWindowTitle.ToWindowTitleText(),
                    Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("LoginImage", typeof(WindowIcon), null, null) as WindowIcon,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = this,
                    Content = new MpAvSettingsView()
                };
            } else {
                sw = new MpAvWindow() {
                    ShowInTaskbar = true,
                    Width = 1050,
                    Height = 650,
                    Title = UiStrings.CommonSettingsTitle.ToWindowTitleText(),
                    Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("CogColorImage", typeof(WindowIcon), null, null) as WindowIcon,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = this,
                    Content = new MpAvSettingsView()
                };
                sw.Classes.Add("fadeIn");
            }

            void Sw_Opened(object sender, EventArgs e) {
                sw.Activate();
                if (sw.Content is MpAvSettingsView sv &&
                    sv.FindControl<Control>("FilterBox") is { } fb &&
                    fb.GetVisualDescendant<TextBox>() is { } ftb) {
                    // focus filter box by default
                    //ftb.TrySetFocusAsync().FireAndForgetSafeAsync();
                    ftb.Focus();
                }
            }

            void Sw_Closed(object sender, EventArgs e) {
                sw.Opened -= Sw_Opened;
                sw.Closed -= Sw_Closed;
            }

            sw.Opened += Sw_Opened;
            sw.Closed += Sw_Closed;

            return sw;
        }
        private void UpdateFilters() {

            TabLookup.ForEach(x => x.Value.ForEach(y => y.OnPropertyChanged(nameof(y.FilteredItems))));
            OnPropertyChanged(nameof(FilteredTabLookup));
            OnPropertyChanged(nameof(FilteredAccountFrames));
            OnPropertyChanged(nameof(FilteredPreferenceFrames));

            IsTabButtonVisible0 = FilteredTabLookup[MpSettingsTabType.Account].Any();
            IsTabButtonVisible1 = FilteredTabLookup[MpSettingsTabType.Preferences].Any();
            IsTabButtonVisible2 =
                string.IsNullOrEmpty(FilterText) ||
                MpAvAppCollectionViewModel.Instance.FilteredExternalItems.Any() ||
                MpAvUrlCollectionViewModel.Instance.FilteredItems.Any();

            IsTabButtonVisible3 = MpAvShortcutCollectionViewModel.Instance.FilteredItems.Any();

            AddOrUpdateRecentFilterTextsAsync(FilterText).FireAndForgetSafeAsync();
        }
        private async Task AddOrUpdateRecentFilterTextsAsync(string st) {
            while (MpAvPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentSettingsSearches = await MpAvPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpAvPrefViewModel.Instance.RecentSettingsSearchTexts), st);
        }

        #region Pref Handling
        private void MpPrefViewModel_Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpAvPrefViewModel.Instance.IsLoggingEnabled):
                    SetIsLoggingEnabled(MpAvPrefViewModel.Instance.IsLoggingEnabled);
                    break;
                case nameof(MpAvPrefViewModel.Instance.LoadOnLogin):
                    SetLoadOnLogin(MpAvPrefViewModel.Instance.LoadOnLogin);
                    break;
                case nameof(MpAvPrefViewModel.Instance.CurrentCultureCode):
                    SetLanguage(MpAvPrefViewModel.Instance.CurrentCultureCode);
                    break;
                case nameof(MpAvPrefViewModel.Instance.MaxUndoLimit):
                    MpAvUndoManagerViewModel.Instance.MaximumUndoLimit = MpAvPrefViewModel.Instance.MaxUndoLimit;
                    break;
                case nameof(MpAvPrefViewModel.Instance.ShowContentTitles):
                    MpAvClipTrayViewModel.Instance.AllItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsTitleVisible)));
                    break;
                case nameof(MpAvPrefViewModel.Instance.NotificationSoundVolume):
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                        // ignore preview volume while sliding
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        await MpAvSoundPlayerViewModel.Instance.UpdateVolumeCommand.ExecuteAsync(null);
                        MpAvSoundPlayerViewModel.Instance.PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Copy);
                    });

                    break;
                case nameof(MpAvPrefViewModel.Instance.MaxPinClipCount):
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.InternalPinnedItems));
                    break;
                case nameof(MpAvPrefViewModel.Instance.TrackExternalPasteHistory):
                    MpAvShortcutCollectionViewModel.Instance.InitExternalPasteTracking();
                    break;
                case nameof(MpAvPrefViewModel.Instance.TrashCleanupModeTypeStr):
                    MpAvTagTrayViewModel.Instance.SetNextTrashCleanupDate();
                    break;
                case nameof(MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup):
                    ProcessListenOnStartupChanged();
                    break;
                case nameof(MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled):
                    MpAvClipTrayViewModel.Instance.ReloadAllContentCommand.Execute(null);
                    break;
            }

            if (MpAvThemeViewModel.Instance.IsThemePref(e.PropertyName)) {
                MpAvThemeViewModel.Instance.UpdateThemeResources(true);
            }

            if (IsBatchUpdate) {
                return;
            }
            if (_reinitContentParams.Any(x => x.ToLowerInvariant() == e.PropertyName.ToLowerInvariant())) {
                //Task.WhenAll(MpAvClipTrayViewModel.Instance.AllActiveItems
                //    .Where(x => x.GetContentView() != null)
                //    .Select(x => x.GetContentView().ReloadAsync())).FireAndForgetSafeAsync();
                MpAvClipTrayViewModel.Instance.ReloadAllContentCommand.Execute(null);
            }

            if (_restartContentParams.Any(x => x.ToLowerInvariant() == e.PropertyName.ToLowerInvariant())) {
                ShowRestartDialogAsync().FireAndForgetSafeAsync();
            }
        }
        private async Task ShowRestartDialogAsync() {
            // NOTE only returns if not restarting
            await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                notificationType: MpNotificationType.ModalRestartNowOrLater,
                title: UiStrings.CommonConfirmLabel,
                body: UiStrings.PrefRestartConfirmNtfText,
                iconSourceObj: "ClockArrowImage");
        }

        private void SetIsLoggingEnabled(bool isLoggingEnabled) {
            string log_path = Mp.Services.PlatformInfo.LoggingEnabledCheckPath;
            string log_dir = Path.GetDirectoryName(log_path);
            if (isLoggingEnabled) {
                if (!log_path.IsFile()) {
                    try {
                        if (!log_dir.IsDirectory()) {
                            MpFileIo.TouchDir(log_dir);
                        }
                        // create dummy file so if logging is enabled its known immediatly on startup
                        MpFileIo.TouchFile(Mp.Services.PlatformInfo.LoggingEnabledCheckPath);
                    }
                    catch { }
                }
            } else {
                if (log_path.IsFile()) {
                    MpFileIo.DeleteFile(log_path);
                }
            }

        }
        private void SetLanguage(string cultureCode) {
            MpAvCurrentCultureViewModel.Instance.SetCultureCommand.Execute(cultureCode);
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            Dispatcher.UIThread.Post(async () => {
                await Mp.Services.LoadOnLoginTools.SetLoadOnLoginAsync(loadOnLogin);
                MpAvPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

                MpConsole.WriteLine($"Load At Login: {(MpAvPrefViewModel.Instance.LoadOnLogin ? "ON" : "OFF")}");
            });
        }

        #region Theme Button Color

        private void SetThemeButtonColor(Button tb) {
            if (tb == null) {
                return;
            }
            tb.Margin = new Thickness(0);
            tb.Content = new MpAvClipBorder() {
                BorderBrush = Brushes.Transparent,
                Background = MpAvPrefViewModel.Instance.ThemeColor.ToAvBrush(),
                Content = new TextBlock() {
                    Margin = new Thickness(5, 3),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = MpAvPrefViewModel.Instance.ThemeColor.ToContrastForegoundColor().ToAvBrush(),
                    Text = (tb.DataContext as MpAvParameterViewModelBase).Label
                }
            };
        }

        private void Tb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            SetThemeButtonColor(sender as Button);
        }

        #endregion

        #region Routing Profile 

        private void SetRoutingProfileType(MpShortcutRoutingProfileType rpt) {
            Dispatcher.UIThread.VerifyAccess();
            if (GetParameterControlByParamId<ComboBox>(MpRuntimePrefParamType.ChangeRoutingType.ToString()) is not ComboBox cmb) {
                return;
            }
            cmb.SelectedIndex = (int)rpt;
        }
        #endregion

        #region Remember Account 

        #endregion

        #region Clear Password

        private void SetupPasswordButtons() {
            Dispatcher.UIThread.VerifyAccess();
            if (TryGetParamAndFrameViewModelsByParamId(MpRuntimePrefParamType.ClearDbPassword.ToString(), out var clear_pwd_param_tuple)) {
                clear_pwd_param_tuple.Item2.IsEnabled = Mp.Services.DbInfo.HasUserDefinedPassword;
            }

            if (TryGetParamAndFrameViewModelsByParamId(MpRuntimePrefParamType.ChangeDbPassword.ToString(), out var change_pwd_param_tuple)) {
                if (change_pwd_param_tuple.Item2 is MpAvButtonParameterViewModel bpvm) {
                    bpvm.Title =
                        Mp.Services.DbInfo.HasUserDefinedPassword ? UiStrings.CommonChangeLabel : UiStrings.CommonSetLabel;
                }
            }
            if (TryGetParamAndFrameViewModelsByParamId(MpRuntimePrefParamType.ForgetDbPassword.ToString(), out var forget_pwd_param_tuple)) {
                forget_pwd_param_tuple.Item2.IsEnabled = MpAvPrefViewModel.Instance.RememberedDbPassword != null;
            }
        }
        #endregion

        private void ProcessListenOnStartupChanged() {
            Dispatcher.UIThread.VerifyAccess();
            if (TryGetParamAndFrameViewModelsByParamId(nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup), out var add_startup_param_tupe)) {
                add_startup_param_tupe.Item2.IsEnabled = MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup;
            }
        }

        #region Culture Helpers

        private void SetupCulturesComboBox(ComboBox cb) {
            if (cb == null ||
                cb.DataContext is not MpAvEnumerableParameterViewModelBase cbpvm) {
                return;
            }

            cb.Classes.Add("culture-chooser");
        }
        #endregion

        #region Font Helpers

        private void SetupFontFamilyComboBox(ComboBox cb) {
            if (cb == null || cb.DataContext is not MpAvEnumerableParameterViewModelBase cbpvm) {
                return;
            }

            cb.Classes.Add("font-chooser");
        }
        #endregion

        #endregion

        #endregion


        #region Param Locators
        public Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> GetParamAndFrameViewModelsByParamId(string paramId, MpSettingsFrameType frameType = MpSettingsFrameType.None) {
            if (Items == null) {
                return null;
            }
            foreach (var sfvm in Items) {
                if (sfvm.Items == null) {
                    continue;
                }
                if (sfvm.Items.FirstOrDefault(x => x.ParamId.ToStringOrEmpty().ToLowerInvariant() == paramId.ToLowerInvariant())
                    is MpAvParameterViewModelBase param_vm) {
                    if (frameType != MpSettingsFrameType.None && sfvm.FrameType != frameType) {
                        continue;
                    }
                    return new Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase>(sfvm, param_vm);
                }
            }
            return null;
        }

        public bool TryGetParamAndFrameViewModelsByParamId(MpSettingsFrameType frameType, string paramId, out Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> result) {
            result = GetParamAndFrameViewModelsByParamId(paramId, frameType);
            return result != null && result.Item1 != null && result.Item2 != null;
        }
        private T GetParameterControlByParamId<T>(string paramId) where T : Control {
            if (MpAvSettingsFrameView.ParamViews
                .FirstOrDefault(x => x.DataContext != null && x.BindingContext.ParamId.ToString() == paramId)
                    is not MpAvPluginParameterItemView piv) {
                return default;
            }
            return piv.GetVisualDescendant<T>();
        }
               
        private bool TryGetParamAndFrameViewModelsByParamId(string paramId, out Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> result) {
            result = GetParamAndFrameViewModelsByParamId(paramId);
            return result != null && result.Item1 != null && result.Item2 != null;
        }
        
        #endregion

        #region Commands

        public ICommand SaveSettingsCommand => new MpCommand(
            () => {
                IsWindowOpen = false;
            });
        public ICommand CancelSettingsCommand => new MpCommand(
            () => {
                IsWindowOpen = false;
            });
        public MpIAsyncCommand<object> SelectTabCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();

                int tab_idx = SelectedTabIdx < 0 ? (int)DefaultSelectedTab : SelectedTabIdx;
                string focus_param_id = null;
                if (args is object[] argParts) {
                    tab_idx = (int)((MpSettingsTabType)argParts[0]);
                    focus_param_id = argParts[1].ToString();
                } else if (args is MpSettingsTabType tt) {
                    tab_idx = (int)tt;
                } else if (args is int intArg) {
                    tab_idx = intArg;
                } else if (args is string strArg && int.TryParse(strArg, out int intStrArg)) {
                    tab_idx = intStrArg;
                }
                // clear search to ensure focus is visible
                FilterText = string.Empty;

                if (focus_param_id == null ||
                    GetParamAndFrameViewModelsByParamId(focus_param_id)
                    is not Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> focus_tuple) {
                    SelectedTabIdx = tab_idx;

                    if (SelectedTabIdx == (int)MpSettingsTabType.CopyAndPaste &&
                        focus_param_id.DeserializeObject<MpPortableProcessInfo>() is { } app_pi &&
                        MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(app_pi) is { } avm) {
                        // select copy&paste tab, expand custom ole, select app
                        MpAvAppCollectionViewModel.Instance.IsCustomClipboardDataGridExpanded = true;
                        MpAvAppCollectionViewModel.Instance.SelectedCustomClipboardFormatItem = avm;
                        await Task.Delay(400);
                        avm.DoFocusPulse = true;
                    }
                    return;
                }
                if (TabLookup.Any(x => x.Value.Contains(focus_tuple.Item1))) {
                    var tab_kvp = TabLookup.FirstOrDefault(x => x.Value.Contains(focus_tuple.Item1));
                    SelectedTabIdx = TabLookup.IndexOf(tab_kvp);
                } else {
                    SelectedTabIdx = (int)DefaultSelectedTab;
                }
                // wait for param to be in view...
                var param_view = GetParameterControlByParamId<MpAvPluginParameterItemView>(focus_param_id);
                while (true) {
                    if (param_view != null && param_view.IsVisible) {
                        break;
                    }
                    if (param_view == null) {
                        param_view = GetParameterControlByParamId<MpAvPluginParameterItemView>(focus_param_id);
                    }
                    await Task.Delay(100);
                }
                focus_tuple.Item1.SelectedItem = focus_tuple.Item2;
                // wait a tid
                await Task.Delay(450);
                Dispatcher.UIThread.Post(param_view.BringIntoView, DispatcherPriority.Background);
                // select arg param and pulse
                param_view.BindingContext.DoFocusPulse = true;

            });
        public ICommand ToggleShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                if (IsWindowOpen) {
                    IsWindowOpen = false;
                    return;
                }
                ShowSettingsWindowCommand.Execute(null);
            });
        public MpIAsyncCommand<object> ShowSettingsWindowCommand => new MpAsyncCommand<object>(
            async (args) => {
                UpdateFilters();
#if DESKTOP
                if (IsWindowOpen) {
                    IsWindowActive = true;
                } else if (Mp.Services.PlatformInfo.IsDesktop) {
                    var sw = CreateSettingsWindow();

                    sw.Show();
                    MpMessenger.SendGlobal(MpMessageType.SettingsWindowOpened);
                }
#else
                App.SetPrimaryView(MpAvSettingsView.Instance);
#endif
                await SelectTabCommand.ExecuteAsync(args);
            });
        public ICommand CloseSettingsCommand => new MpCommand(
            () => {
                IsWindowOpen = false;
            }, () => {
                return IsWindowOpen;
            });
        public ICommand ButtonParameterClickCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpRuntimePrefParamType cmdType = MpRuntimePrefParamType.None;
                if (args is string strArg &&
                    strArg.ToEnum<MpRuntimePrefParamType>() is MpRuntimePrefParamType typeArg) {
                    cmdType = typeArg;
                } else if (args is MpRuntimePrefParamType) {
                    cmdType = (MpRuntimePrefParamType)args;
                }
                if (cmdType == MpRuntimePrefParamType.None) {
                    return;
                }
                switch (cmdType) {
                    case MpRuntimePrefParamType.ChangeDbPassword: {
                            var cpw = new MpAvWindow() {
                                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                                SizeToContent = SizeToContent.Height,
                                Width = 325,
                                Topmost = true,
                                Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeCompliment1DarkColor.ToString()),
                                CanResize = false,
                                Title = UiStrings.PrefSetPasswordWindowTitlePrefix.ToWindowTitleText(),
                                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("LockImage", typeof(WindowIcon), null, null) as WindowIcon,
                                Content = new MpAvSetPasswordView() {
                                    ShowDialogButtons = true
                                }
                            };

                            var result = await cpw.ShowDialogWithResultAsync(MpAvWindowManager.LocateWindow(this));
                            if (result is not string new_pwd || new_pwd == null ||
                                cpw.Content is not MpAvSetPasswordView spv) {
                                // user cancel or closed window
                                break;
                            }

                            bool success = await MpDb.ChangeDbPasswordAsync(new_pwd, spv.RememberPassword);
                            SetupPasswordButtons();

                            if (!success) {
                                Mp.Services.NotificationBuilder.ShowMessageAsync(
                                    msgType: MpNotificationType.DbError,
                                    title: UiStrings.CommonErrorLabel,
                                    body: UiStrings.PrefChangeDbPwdFailedNtfText,
                                    iconSourceObj: "WarningImage").FireAndForgetSafeAsync(this);
                            }

                            break;
                        }
                    case MpRuntimePrefParamType.ForgetDbPassword: {
                            var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                                    title: UiStrings.CommonConfirmLabel,
                                    message: UiStrings.PrefForgetDbPwdNtfText,
                                    iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            Mp.Services.DbInfo.SetPassword(Mp.Services.DbInfo.DbPassword2, false);
                            SetupPasswordButtons();

                            break;
                        }
                    case MpRuntimePrefParamType.ClearDbPassword: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                                title: UiStrings.CommonConfirmLabel,
                                message: UiStrings.PrefResetPwdNtfText,
                                iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            await MpDb.ChangeDbPasswordAsync(null, true);
                            SetupPasswordButtons();
                            break;
                        }
                    case MpRuntimePrefParamType.ResetNtf: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonConfirmLabel,
                            message: UiStrings.PrefResetAllNtfForgetsText,
                            iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;
                            break;
                        }

                    case MpRuntimePrefParamType.ResetPluginCache: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonConfirmLabel,
                            message: UiStrings.PrefResetPluginCacheNtfText,
                            iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }

                            IsBusy = true;

                            var all_presets = MpAvAnalyticItemCollectionViewModel.Instance.Items.ToList();
                            foreach (var aivm in MpAvAnalyticItemCollectionViewModel.Instance.Items) {
                                foreach (var aipvm in aivm.Items) {
                                    aipvm.ResetOrDeleteThisPresetCommand.Execute(null);
                                    while (aivm.IsBusy) {
                                        await Task.Delay(100);
                                    }
                                }
                            }

                            // delete cache dir (it will be recreated on restart)
                            string cache_dir = MpPluginLoader.PluginCacheDir;
                            bool success = MpFileIo.DeleteDirectory(cache_dir);

                            IsBusy = false;
                            if (!success) {
                                Mp.Services.NotificationBuilder.ShowMessageAsync(
                                    title: UiStrings.CommonErrorLabel,
                                    body: string.Format(UiStrings.PrefResetPluginCacheErrorText, cache_dir),
                                    msgType: MpNotificationType.FileIoWarning).FireAndForgetSafeAsync(this);
                            }

                            break;
                        }
                    case MpRuntimePrefParamType.ThemeHexColor: {
                            var result = await
                                Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                    title: UiStrings.PrefThemeColorLabel,
                                    selectedColor: MpAvPrefViewModel.Instance.ThemeColor
                                //fixedPalette: MpSystemColors.SpectrumColors
                                );
                            if (string.IsNullOrEmpty(result)) {
                                // color chooser canceled
                                break;
                            }

                            MpAvPrefViewModel.Instance.ThemeColor = result;
                            Dispatcher.UIThread.Post(() => {
                                var tb = GetParameterControlByParamId<Button>(MpRuntimePrefParamType.ThemeHexColor.ToString());
                                if (tb == null) {
                                    return;
                                }
                                SetThemeButtonColor(tb);
                            });

                            break;
                        }
                    case MpRuntimePrefParamType.ClearRecentSearches:
                        MpAvSearchBoxViewModel.Instance.RecentSearchTexts.Clear();
                        MpAvPrefViewModel.Instance.RecentSearchTexts = string.Empty;

                        MpAvPluginBrowserViewModel.Instance.RecentPluginSearches.Clear();
                        MpAvPrefViewModel.Instance.RecentPluginSearchTexts = string.Empty;

                        RecentSettingsSearches.Clear();
                        MpAvPrefViewModel.Instance.RecentSettingsSearchTexts = string.Empty;
                        break;
                    case MpRuntimePrefParamType.ResetShortcuts:
                        MpAvShortcutCollectionViewModel.Instance.ResetAllShortcutsCommand.Execute(null);
                        break;

                    case MpRuntimePrefParamType.AccountRegister: {
                            MpAvAccountViewModel.Instance.RegisterCommand.Execute(null);
                            break;
                        }

                    case MpRuntimePrefParamType.AccountLogin: {
                            MpAvAccountViewModel.Instance.LoginCommand.Execute(MpLoginSourceType.Click);
                            break;
                        }
                    case MpRuntimePrefParamType.AccountResetPassword: {
                            MpAvAccountViewModel.Instance.ResetPasswordRequestCommand.Execute(null);
                            break;
                        }
                    case MpRuntimePrefParamType.AccountShowPrivacyPolicy: {
                            MpAvAccountViewModel.Instance.ShowPrivacyPolicyCommand.Execute(null);
                            break;
                        }

                    case MpRuntimePrefParamType.AccountLogout: {
                            MpAvAccountViewModel.Instance.LogoutCommand.Execute(null);
                            break;
                        }

                    case MpRuntimePrefParamType.AccountShowRegister: {
                            MpAvAccountViewModel.Instance.ShowRegisterPanelCommand.Execute(null);
                            break;
                        }
                    case MpRuntimePrefParamType.AccountShowLogin: {
                            MpAvAccountViewModel.Instance.ShowLoginPanelCommand.Execute(null);
                            break;
                        }
                    case MpRuntimePrefParamType.DeleteAllContent: {
                            MpAvClipTrayViewModel.Instance.DeleteAllContentCommand.Execute(null);
                            break;
                        }
                    case MpRuntimePrefParamType.ShowLogsFolder: {
                            MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(Mp.Services.PlatformInfo.LogDir.ToFileSystemUriFromPath().LocalStoragePathToPackagePath());
                            break;
                        }

                }
            });
        public ICommand ClearFilterTextCommand => new MpCommand(
            () => {
                FilterText = string.Empty;
            });
        public ICommand RestoreDefaultsCommand => new MpAsyncCommand(
            async () => {
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                    title: UiStrings.CommonConfirmLabel,
                    message: UiStrings.PrefRestoreDefaultsNtfText,
                    iconResourceObj: "WarningImage");
                if (!result) {
                    //cancel
                    return;
                }
                IsBatchUpdate = true;
                MpAvPrefViewModel.Instance.RestoreDefaults();
                IsBatchUpdate = false;

                MpAvAppRestarter.ShutdownWithRestartTaskAsync("Restoring Default Preferences").FireAndForgetSafeAsync();

            });
        #endregion
    }
}
