using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpISettingsTools {
        #region Private Variables

        private string[] _reinitContentParams = new string[] {
            nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
            nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
            nameof(MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled),
            nameof(MpAvPrefViewModel.Instance.DefaultFontSize),
            nameof(MpAvPrefViewModel.Instance.IsSpellCheckEnabled),
            nameof(MpAvPrefViewModel.Instance.ThemeTypeName),
            nameof(MpAvPrefViewModel.Instance.ThemeColor),
            nameof(MpAvPrefViewModel.Instance.GlobalBgOpacity),
        };

        public string[] HiddenParamIds = new string[] {
            nameof(MpAvPrefViewModel.Instance.NotificationSoundGroupIdx),
            nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup),
            nameof(MpAvPrefViewModel.Instance.UserLanguageCode),
            nameof(MpAvPrefViewModel.Instance.IsTextRightToLeft)
        };

        private Dictionary<object, Action<MpAvPluginParameterItemView>> _runtimeParamAttachActions;
        #endregion

        #region CONSTANTS

        public const MpSettingsTabType DEFAULT_SELECTED_TAB = MpSettingsTabType.Preferences;
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
            TabLookup.SelectMany(x => x.Value);

        public Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>> TabLookup { get; set; }
        public Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>> FilteredTabLookup =>
            TabLookup.ToDictionary(x => x.Key, x => x.Value.Where(x => x.FilteredItems.Any()));

        public IEnumerable<MpAvSettingsFrameViewModel> FilteredAccountFrames =>
            FilteredTabLookup[MpSettingsTabType.Account];

        public IEnumerable<MpAvSettingsFrameViewModel> FilteredPreferenceFrames =>
            FilteredTabLookup[MpSettingsTabType.Preferences];
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


        public bool IsTabButtonVisible0 { get; set; } = true;
        public bool IsTabButtonVisible1 { get; set; } = true;
        public bool IsTabButtonVisible2 { get; set; } = true;
        public bool IsTabButtonVisible3 { get; set; } = true;
        public bool IsTabButtonVisible4 { get; set; } = true;
        public string FilterText { get; set; } = string.Empty;


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

        public ObservableCollection<bool> IsTabSelected { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvSettingsViewModel() : base() {
            MpAvPrefViewModel.Instance.PropertyChanged += MpPrefViewModel_Instance_PropertyChanged;
            PropertyChanged += MpAvSettingsWindowViewModel_PropertyChanged;

            Mp.Services.SettingsTools = this;

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
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Account) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AccountEmail),
                                            isVisible = !string.IsNullOrEmpty(MpAvPrefViewModel.Instance.AccountEmail),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = "Email",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = string.IsNullOrEmpty(MpAvPrefViewModel.Instance.AccountEmail) ?
                                                        "Unavailable":MpAvPrefViewModel.Instance.AccountEmail
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ChangeAccountType.ToString(),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Status",
                                            values =
                                                Enum.GetNames(typeof(MpUserAccountType))
                                                .Select((x,idx)=> new MpPluginParameterValueFormat() {
                                                    isDefault = Mp.Services.AccountTools.CurrentAccountType.ToString() == x,
                                                    value = x,
                                                    label = x.ToEnum<MpUserAccountType>().EnumToUiString()
                                                }).ToList()
                                        },
                                    }
                                }
                            }
                        }
                    }

                },
                {
                    MpSettingsTabType.Preferences,
                    new[] {
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Theme) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ThemeTypeName),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Theme Style",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Light,
                                                    value = MpThemeType.Light.ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.ThemeType == MpThemeType.Dark,
                                                    value = MpThemeType.Dark.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ThemeHexColor.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Theme Color",
                                            description = "Caution! Palette colors are decided by math not eyes! So please be careful as some color and style combinations may not have proper contrast for readability.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Select",
                                                    value = MpRuntimePrefParamType.ThemeHexColor.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Fonts) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Interface",
                                            description = "Requires restart",
                                            values =
                                                FontManager.Current.SystemFonts
                                                .Select(x=>x.Name)
                                                //(AvaloniaLocator.Current.GetRequiredService<IFontManagerImpl>()).GetInstalledFontFamilyNames()
                                                .Where(x=>!string.IsNullOrEmpty(x))
                                                .OrderBy(x=>x)
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvThemeViewModel.Instance.DefaultReadOnlyFontFamily.ToLower() == x.ToLower(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultEditableFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            description = "Requires restart",
                                            label = "Content",
                                            values =
                                                FontManager.Current.SystemFonts
                                                .Select(x=>x.Name)
                                                //FontManager.Current.GetInstalledFontFamilyNames(true)
                                                //(AvaloniaLocator.Current.GetRequiredService<IFontManagerImpl>()).GetInstalledFontFamilyNames(true)
                                                .Where(x=>!string.IsNullOrEmpty(x))
                                                .OrderBy(x=>x)
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvThemeViewModel.Instance.DefaultEditableFontFamily.ToLower() == x.ToLower(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DefaultFontSize),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = "Size",
                                            values =
                                                new int[]{ 8, 9, 10, 12, 14, 16, 20, 24, 32, 42, 54, 68, 84, 98 }
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.DefaultFontSize == x,
                                                    value = x.ToString(),
                                                }).ToList()
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Sound) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.NotificationSoundGroupIdx),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = "Sound Theme",
                                            values =
                                                new int[]{ 0, 1, 2, 3 }
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.NotificationSoundGroupIdx == x,
                                                    label = ((MpSoundGroupType)x).ToString(),
                                                    value = x.ToString(),
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.NotificationSoundVolume),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = "Notification Volume",
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.NotificationSoundVolume.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Taskbar) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowInTaskbar),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show In Taskbar",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowInTaskbar.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowInTaskSwitcher),
                                            description = "Requires restart",
                                            isVisible = OperatingSystem.IsWindows(),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show In Task Switcher",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowInTaskSwitcher.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Window) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MainWindowShowBehaviorType),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Show Behavior",
                                            values =
                                                Enum.GetNames(typeof(MpMainWindowShowBehaviorType))
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MainWindowShowBehaviorType.ToLower() == x.ToLower(),
                                                    value = x,
                                                    label = x.ToEnum<MpMainWindowShowBehaviorType>().EnumToUiString()
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta),
                                            description = "Monkey Paste will be revealed by performing a scroll gesture on the top edge of your monitor",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show on top screen scroll",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AnimateMainWindow),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Animate Window",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.AnimateMainWindow.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.GlobalBgOpacity),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = "Background Opacity",
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
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
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowHints),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show Hints",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowHints.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        //new MpAvSettingsFrameViewModel(MpSettingsFrameType.International) {
                        //    PluginFormat = new MpPluginFormat() {
                        //        headless = new MpHeadlessPluginFormat() {
                        //            parameters = new List<MpParameterFormat>() {
                        //                new MpParameterFormat() {
                        //                    paramId = nameof(MpAvPrefViewModel.Instance.UserLanguageCode),
                        //                    controlType = MpParameterControlType.ComboBox,
                        //                    unitType = MpParameterValueUnitType.PlainText,
                        //                    label = "Language",
                        //                    values =
                        //                        MpCurrentCultureViewModel.Instance.AvailableCultureLookup
                        //                        .Select(x=>
                        //                        new MpPluginParameterValueFormat() {
                        //                            isDefault = x.Key == MpAvPrefViewModel.Instance.UserLanguageCode,
                        //                            label = x.Value,
                        //                            value = x.Key
                        //                        }).ToList()
                        //                },
                        //                new MpParameterFormat() {
                        //                    paramId = nameof(MpAvPrefViewModel.Instance.IsTextRightToLeft),
                        //                    controlType = MpParameterControlType.CheckBox,
                        //                    unitType = MpParameterValueUnitType.Bool,
                        //                    label = "Right-to-left",
                        //                    values = new List<MpPluginParameterValueFormat>() {
                        //                        new MpPluginParameterValueFormat() {
                        //                            isDefault = true,
                        //                            value = MpAvPrefViewModel.Instance.IsTextRightToLeft.ToString()
                        //                        }
                        //                    }
                        //                },
                        //            }
                        //        }
                        //    }
                        //},
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Limits) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxUndoLimit),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Undo Limit",
                                            description = "High undo limits may affect performance",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "100",
                                                    value = "100".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxUndoLimit.ToString() == "-1",
                                                    value = "-1".ToString(),
                                                    label = "No Limit"
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxPinClipCount),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Integer,
                                            minimum = 1,
                                            maximum = 50,
                                            label = "Pin Capacity",
                                            description = "Large or many staged items can consume significant memory. removal follows a first-in-first-out policy",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
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
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreAppendedItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Appends",
                                            description = "When new clipboard content is appended it will not be tracked",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreAppendedItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Internal Clipboard Tracking",
                                            description = "Clipboard changes from within the app will not be tracked",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Only White Space",
                                            description = "Spaces, tabs and new lines will be ignored from tracking",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.TrackExternalPasteHistory),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Paste History",
                                            description = "This setting will track system-wide keyboard paste counts for usage statistics. This information is private and not shared but also not guarenteed. Accurate results may require providing application specific keyboard paste command.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.TrackExternalPasteHistory.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsDuplicateCheckEnabled),
                                            description = "When <b>duplicate</b> is detected on clipboard, it will be <b>staged</b> from original source and not added redundantly.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore New Duplicates",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
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
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.LoadOnLogin),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Load on Login",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.LoadOnLogin.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Add Startup Clipboard",
                                            description = "#warn#On startup the source of the clipboard will be unknown and depending on your security settings the content may come from an excluded website or application.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.AddClipboardOnStartup.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.TrashCleanupModeTypeStr),
                                            description = $"Automatic trash cleanup will occur on startup at the set interval. It is not required but definitely don't let it get too large! Overall performance and CPU usage will have an impact if it becomes large ie. over 100 items lets say...",
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Trash Emptying",
                                            values =
                                                Enum.GetNames(typeof(MpTrashCleanupModeType))
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.TrashCleanupModeTypeStr.ToLower() == x.ToString().ToLower(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Listen to clipboard on startup",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Search) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.MaxRecentTextsCount),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Recent Text Limit",
                                            description = "This applies to all auto-completable inputs, lower values will improve performance",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "100",
                                                    value = "100".ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsAutoSearchEnabled),
                                            description = "Text typed will be automatically applied to a search when no text control is focused.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Auto Search Input",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
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
                                            label = "User-Defined Search Extensions",
                                            description = "Used in advanced file search for non preset file kinds",
                                            values =
                                            new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.UserDefinedFileExtensionsCsv
                                                }
                                            }
                                                //MpPrefViewModel.Instance.UserDefinedFileExtensionsCsv
                                                //.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value)
                                                //.Select(x=>
                                                //    new MpPluginParameterValueFormat() {
                                                //        isDefault = true,
                                                //        value = x
                                                //    })
                                                //.ToList()
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Content) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsSpellCheckEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Enable Spell Check",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsSpellCheckEnabled.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Rich Content",
                                            description = "(Enabling can be quite slow) Disabling formatted will reduce memory consumption but will disable some advanced features, such as: templates, annotations and find/replace",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Paste, drop or append with destination formatting",
                                            description = "When true, external content inserted into a clip will use the clips rich formatting and not the source content.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.IsDataTransferDestinationFormattingEnabled.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Reset Clipboard after Monkey Pasting",
                                            description = "#warn#Monkey Paste uses the system clipboard to interoperate and changes your current clipboard when pasting to an external program. This will <b>attempt</b> to restore the state of the clipboard after an external paste. Restoration cannot be guaranteed and may reduce performance or even crash Monkey Paste depending on the source of the data (looking at you windows).",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowContentTitles),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show Titles",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpAvPrefViewModel.Instance.ShowContentTitles.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.DragAndDrop) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop),
                                            description = "This helps fluidly allow dropping data into Monkey paste from another application without extra fumbling with window placement by dragging data to the top of the screen which will activate Monkey Paste and allow drop.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show on drag to top of screen",
                                            value = new MpPluginParameterValueFormat(MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop.ToString(),true)
                                        }
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Shortcuts) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ChangeRoutingType.ToString(),
                                            controlType = MpParameterControlType.ComboBox,
                                            label = "Profile",
                                            description = UiStrings.ShortcutRoutingProfileTypeHint,
                                            values =
                                                Enum.GetNames(typeof(MpShortcutRoutingProfileType))
                                                .Select((x,idx)=> new MpPluginParameterValueFormat() {
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
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ChangeDbPassword.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Change Password",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Change",
                                                    value = MpRuntimePrefParamType.ChangeDbPassword.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ClearDbPassword.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Clear Password",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Clear",
                                                    value = MpRuntimePrefParamType.ClearDbPassword.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.System) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetNtf.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "None Notifications",
                                            description = "All ignored notifications previously set.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Reset",
                                                    value = MpRuntimePrefParamType.ResetNtf.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetShortcuts.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "App GestureProfile",
                                            description = "All application shortcuts will be reset to their default key gestures.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Reset",
                                                    value = MpRuntimePrefParamType.ResetShortcuts.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetPluginCache.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Plugin Cache",
                                            description = "All plugins will be reset to initial default state",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    label = "Reset",
                                                    value = MpRuntimePrefParamType.ResetPluginCache.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    .OrderByDescending(x=>x.Items == null ? 0:x.Items.Count)
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
                                x.values.FirstOrDefault(x => x.isDefault) is MpPluginParameterValueFormat ppvf ?
                                    ppvf.value : string.Empty
                            }, fvm)));

                // map button cmds (currently only in system frame)
                fvm.Items
                    .Where(x => x.ControlType == MpParameterControlType.Button)
                    .Cast<MpAvButtonParameterViewModel>()
                    .ForEach(x => x.ClickCommand = ButtonParameterClickCommand);


                foreach (var para_item in fvm.Items) {
                    if (HiddenParamIds.Any(x => x == para_item.ParameterFormat.paramId)) {
                        para_item.ParameterFormat.isVisible = false;
                    }
                }

            }


        }
        private void InitRuntimeParams() {
            _runtimeParamAttachActions = new Dictionary<object, Action<MpAvPluginParameterItemView>>() {
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
                        if(!cb.Classes.Contains("fontFamilyOverride")) {
                            cb
                            .GetSelfAndVisualDescendants()
                            .OfType<TemplatedControl>()
                            .ForEach(x=>x.Classes.Add("fontFamilyOverride"));
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
                    MpRuntimePrefParamType.ChangeAccountType.ToString(),
                    (piv) => {
                        if(piv.GetVisualDescendant<ComboBox>() is not ComboBox cmb) {
                            return;
                        }
                        void AccountTypeSelectionChanged(object sender, SelectionChangedEventArgs e) {
                            MpUserAccountType sel_type = (MpUserAccountType)cmb.SelectedIndex;
                            Mp.Services.AccountTools.SetAccountType(sel_type);
                        }
                        cmb.SelectionChanged += AccountTypeSelectionChanged;
                    }
                },
                {
                    nameof(MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled),
                    (piv) => {
                        
#if DESKTOP
                        if(piv.GetVisualDescendant<CheckBox>() is not CheckBox cb) {
                            return;
                        }

                		// ensure rich content cb reflects webview availability
                        cb.IsEnabled = MpAvCefNetApplication.IsCefNetLoaded;  
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
                case MpMessageType.AccountDowngrade:
                case MpMessageType.AccountUpgrade:
                    SetAccountType(Mp.Services.AccountTools.CurrentAccountType);
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
                case nameof(IsWindowOpen):
                    MpConsole.WriteLine($"Settings window: {(IsWindowOpen ? "OPEN" : "CLOSED")}");
                    break;

                case nameof(IsWindowActive):
                    MpConsole.WriteLine($"Settings window: {(IsWindowActive ? "ACTIVE" : "INACTIVE")}");
                    break;
            }
        }

        private void IsTabSelected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTabSelected));
        }

        private MpAvWindow CreateSettingsWindow() {
            var sw = new MpAvWindow() {
                ShowInTaskbar = true,
                Width = 1000,
                Height = 500,
                Title = UiStrings.CommonSettingsTitle.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("CogColorImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = this,
                Content = new MpAvSettingsView()
            };
            sw.Classes.Add("fadeIn");

            void Sw_Opened(object sender, EventArgs e) {
                sw.Activate();
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

            // TODO add filtering to help, for now it'll stick around
            IsTabButtonVisible4 = true;

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
                case nameof(MpAvPrefViewModel.Instance.LoadOnLogin):
                    SetLoadOnLogin(MpAvPrefViewModel.Instance.LoadOnLogin);
                    break;
                case nameof(MpAvPrefViewModel.Instance.UserLanguageCode):
                    SetLanguage(MpAvPrefViewModel.Instance.UserLanguageCode);
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

            if (_reinitContentParams.Any(x => x.ToLower() == e.PropertyName.ToLower())) {
                Task.WhenAll(MpAvClipTrayViewModel.Instance.AllActiveItems
                    .Where(x => x.GetContentView() != null)
                    .Select(x => x.GetContentView().ReloadAsync())).FireAndForgetSafeAsync();
            }
        }

        private void SetLanguage(string cultureCode) {
            MpCurrentCultureViewModel.Instance.SetLanguageCommand.Execute(cultureCode);
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            Mp.Services.LoadOnLoginTools.SetLoadOnLogin(loadOnLogin);
            MpAvPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

            MpConsole.WriteLine($"Load At Login: {(MpAvPrefViewModel.Instance.LoadOnLogin ? "ON" : "OFF")}");
        }

        #region Theme Button Color

        private void SetThemeButtonColor(Button tb) {
            if (tb == null) {
                return;
            }
            tb.Margin = new Thickness(0);
            //tb.Padding = new Thickness(0);
            tb.Content = new MpAvClipBorder() {
                //CornerRadius = tb.CornerRadius,
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

        #region Account Type

        private void SetAccountType(MpUserAccountType at) {
            Dispatcher.UIThread.VerifyAccess();
            if (GetParameterControlByParamId<ComboBox>(MpRuntimePrefParamType.ChangeAccountType.ToString()) is not ComboBox cmb) {
                return;
            }
            cmb.SelectedIndex = (int)at;
        }
        #endregion

        #region Clear Password

        private void SetupPasswordButtons() {
            Dispatcher.UIThread.VerifyAccess();
            var clear_pwd_param_tuple = GetParamAndFrameViewModelsByParamId(MpRuntimePrefParamType.ClearDbPassword.ToString());
            clear_pwd_param_tuple.Item2.IsEnabled = Mp.Services.DbInfo.HasUserDefinedPassword;

            var change_pwd_param_tuple = GetParamAndFrameViewModelsByParamId(MpRuntimePrefParamType.ChangeDbPassword.ToString());
            if (change_pwd_param_tuple.Item2 is MpAvButtonParameterViewModel bpvm) {
                bpvm.Title =
                    Mp.Services.DbInfo.HasUserDefinedPassword ? "Change" : "Set";
            }
        }
        #endregion

        private void ProcessListenOnStartupChanged() {
            Dispatcher.UIThread.VerifyAccess();
            var add_startup_param_tupe = GetParamAndFrameViewModelsByParamId(nameof(MpAvPrefViewModel.Instance.AddClipboardOnStartup));
            add_startup_param_tupe.Item2.IsEnabled = MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup;
        }

        #region Font Helpers

        private void SetupFontFamilyComboBox(ComboBox cb) {

            if (cb == null) {
                return;
            }

            var cbpvm = cb.DataContext as MpAvEnumerableParameterViewModelBase;
            if (cbpvm == null) {
                return;
            }
            cb.FontFamily = MpAvStringToFontFamilyConverter.Instance.Convert(cbpvm.CurrentValue, null, null, null) as FontFamily;


            cb.DropDownOpened += Cb_DropDownOpened;

            if (MpAvWindowManager.LocateWindow(this) is Window w) {
                EventHandler w_closed_handler = null;
                w_closed_handler = (s, e) => {
                    cb.DropDownOpened -= Cb_DropDownOpened;
                    w.Closed -= w_closed_handler;
                };
                w.Closed += w_closed_handler;
            }
        }

        private void Cb_DropDownOpened(object sender, EventArgs e) {
            var cb = sender as ComboBox;
            if (cb == null) {
                return;
            }
            var cbil = cb.GetLogicalDescendants().OfType<ComboBoxItem>();
            foreach (var cbi in cbil) {
                if (cbi.DataContext is MpAvEnumerableParameterValueViewModel epvvm) {
                    cbi.FontFamily = MpAvStringToFontFamilyConverter.Instance.Convert(epvvm.Value, null, null, null) as FontFamily;
                }
            }

        }
        #endregion

        #endregion

        #region Param Locators

        private T GetParameterControlByParamId<T>(string paramId) where T : Control {
            if (MpAvSettingsFrameView.ParamViews
                .FirstOrDefault(x => x.DataContext != null && x.BindingContext.ParamId.ToString() == paramId)
                    is not MpAvPluginParameterItemView piv) {
                return default;
            }
            return piv.GetVisualDescendant<T>();
        }

        private Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> GetParamAndFrameViewModelsByParamId(string paramId) {
            if (Items.FirstOrDefault(
                        x => x.Items.Any(
                            y => y.ParamId.ToString().ToLower() == paramId.ToLower()))
                        is MpAvSettingsFrameViewModel frame_vm &&
                        frame_vm.Items.FirstOrDefault(x => x.ParamId.ToString().ToLower() == paramId.ToLower())
                        is MpAvParameterViewModelBase param_vm) {
                MpDebug.Assert(param_vm.ParamId.ToStringOrEmpty().ToLower() == paramId.ToLower(), $"param assert failed '{paramId}'");
                return new Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase>(frame_vm, param_vm);
            }
            return null;
        }
        #endregion

        #endregion

        #region Commands

        public ICommand ResetSettingsCommand => new MpAsyncCommand(
            async () => {
                var result = await Mp.Services.PlatformMessageBox
                .ShowOkCancelMessageBoxAsync(
                    title: UiStrings.CommonNtfConfirmTitle,
                    message: "Are you sure you want to reset all preferences? This action cannot be undone. *Some Settings will not update until restart",
                    iconResourceObj: "WarningImage",
                    owner: MpAvWindowManager.LocateWindow(this));

                if (!result) {
                    // canceled reset all, ignore
                    return;
                }
                MpAvPrefViewModel.Instance.RestoreDefaultsCommand.Execute(null);
            });
        public ICommand SaveSettingsCommand => new MpCommand(
            () => {
                IsWindowOpen = false;
            });
        public ICommand CancelSettingsCommand => new MpCommand(
            () => {
                IsWindowOpen = false;
            });
        public ICommand SelectTabCommand => new MpCommand<object>(
            (args) => {
                int tab_idx = SelectedTabIdx < 0 ? (int)DEFAULT_SELECTED_TAB : SelectedTabIdx;
                string focus_param_id = null;
                if (args is object[] argParts) {
                    tab_idx = (int)((MpSettingsTabType)argParts[0]);
                    focus_param_id = argParts[1].ToString();
                } else if (args is MpSettingsTabType tt) {
                    tab_idx = (int)tt;
                } else if (args is int intArg) {
                    tab_idx = intArg;
                } else if (args is string strArg) {
                    try {
                        tab_idx = int.Parse(strArg);
                    }
                    catch { }
                }

                if (focus_param_id != null &&
                    GetParamAndFrameViewModelsByParamId(focus_param_id) is Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> focus_tuple) {
                    //SelectedTabIdx = (int)focus_tuple.Item1.TabType;
                    if (TabLookup.Any(x => x.Value.Contains(focus_tuple.Item1))) {
                        var tab_kvp = TabLookup.FirstOrDefault(x => x.Value.Contains(focus_tuple.Item1));
                        SelectedTabIdx = TabLookup.IndexOf(tab_kvp);
                    } else {
                        SelectedTabIdx = (int)DEFAULT_SELECTED_TAB;
                    }

                    Dispatcher.UIThread.Post(async () => {
                        // clear search to ensure focus is visible
                        FilterText = string.Empty;

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
                } else {
                    SelectedTabIdx = tab_idx;
                }
            });
        public ICommand ToggleShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                if (IsWindowOpen) {
                    IsWindowOpen = false;
                    return;
                }
                ShowSettingsWindowCommand.Execute(null);
            });
        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                UpdateFilters();
                SelectTabCommand.Execute(args);
                if (IsWindowOpen) {
                    IsWindowActive = true;
                    return;
                }

                if (Mp.Services.PlatformInfo.IsDesktop) {
                    var sw = CreateSettingsWindow();
                    sw.ShowChild();
                    MpMessenger.SendGlobal(MpMessageType.SettingsWindowOpened);
                }
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
                                Title = "Set Password".ToWindowTitleText(),
                                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("LockImage", typeof(WindowIcon), null, null) as WindowIcon,
                                Content = new MpAvSetPasswordView() {
                                    ShowDialogButtons = true
                                }
                            };

                            var result = await cpw.ShowChildDialogWithResultAsync(MpAvWindowManager.LocateWindow(this));
                            if (result is not string new_pwd || new_pwd == null) {
                                // user cancel or closed window
                                break;
                            }

                            var success = await MpDb.ChangeDbPasswordAsync(new_pwd);
                            SetupPasswordButtons();

                            if (!success) {
                                Mp.Services.NotificationBuilder.ShowMessageAsync(
                                title: $"Error",
                                body: "Password change failed",
                                iconSourceObj: "WarningImage").FireAndForgetSafeAsync(this);
                            }

                            break;
                        }
                    case MpRuntimePrefParamType.ClearDbPassword: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                                title: UiStrings.CommonNtfConfirmTitle,
                                message: "Are you sure you want to reset your password?",
                                iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            await MpDb.ChangeDbPasswordAsync(null);
                            SetupPasswordButtons();
                            break;
                        }
                    case MpRuntimePrefParamType.ResetNtf: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonNtfConfirmTitle,
                            message: "Are you sure you want to reset all notifications?",
                            iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;
                            break;
                        }

                    case MpRuntimePrefParamType.ResetPluginCache: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonNtfConfirmTitle,
                            message: "Are you sure you want to reset all plugins to defaults?",
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
                            string cache_dir = MpPluginLoader.PluginManifestBackupFolderPath;
                            bool success = MpFileIo.DeleteDirectory(cache_dir);

                            IsBusy = false;
                            if (!success) {
                                Mp.Services.NotificationBuilder.ShowMessageAsync(
                                    title: "Error",
                                    body: $"Could not delete plugin cache from path: '{cache_dir}'",
                                    msgType: MpNotificationType.FileIoWarning).FireAndForgetSafeAsync(this);
                            }

                            break;
                        }
                    case MpRuntimePrefParamType.ThemeHexColor: {
                            var result = await
                                Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                                    title: "Theme Color",
                                    owner: MpAvWindowManager.LocateWindow(this),
                                    selectedColor: MpAvPrefViewModel.Instance.ThemeColor,
                                    fixedPalette: new[] {
                                        MpSystemColors.Red,
                                        MpSystemColors.orange1,
                                        MpSystemColors.Yellow,
                                        MpSystemColors.green1,
                                        MpSystemColors.blue1,
                                        MpSystemColors.cyan1,
                                        MpSystemColors.magenta,
                                        MpSystemColors.purple
                                    }
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
                    case MpRuntimePrefParamType.ResetShortcuts:
                        MpAvShortcutCollectionViewModel.Instance.ResetAllShortcutsCommand.Execute(null);
                        break;
                }
            });

        public ICommand ClearFilterTextCommand => new MpCommand(
            () => {
                FilterText = string.Empty;
            });
        #endregion
    }
}
