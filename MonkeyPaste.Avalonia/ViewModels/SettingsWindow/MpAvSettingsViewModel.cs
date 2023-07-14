using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using MonoMac.ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsViewModel :
        MpViewModelBase,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpISettingsTools {
        #region Private Variables

        private string[] _reinitContentParams = new string[] {
            nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
            nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily),
            nameof(MpPrefViewModel.Instance.DefaultFontSize),
            nameof(MpPrefViewModel.Instance.IsSpellCheckEnabled),
            nameof(MpPrefViewModel.Instance.ThemeTypeName),
            nameof(MpPrefViewModel.Instance.ThemeColor),
            nameof(MpPrefViewModel.Instance.GlobalBgOpacity),
        };
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
            MpPrefViewModel.Instance.PropertyChanged += MpPrefViewModel_Instance_PropertyChanged;
            PropertyChanged += MpAvSettingsWindowViewModel_PropertyChanged;

            Mp.Services.SettingsTools = this;

            IsTabSelected = new ObservableCollection<bool>(Enumerable.Repeat(false, 6));
            IsTabSelected.CollectionChanged += IsTabSelected_CollectionChanged;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            TabLookup = new Dictionary<MpSettingsTabType, IEnumerable<MpAvSettingsFrameViewModel>>() {
                {
                    MpSettingsTabType.Account,
                    new [] {
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Account) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.UserEmail),
                                            isVisible = !string.IsNullOrEmpty(MpPrefViewModel.Instance.UserEmail),
                                            controlType = MpParameterControlType.TextBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            isReadOnly = true,
                                            label = "Email",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = string.IsNullOrEmpty(MpPrefViewModel.Instance.UserEmail) ?
                                                        "Unavailable":MpPrefViewModel.Instance.UserEmail
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
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = Mp.Services.AccountTools.CurrentAccountType.ToString() == x,
                                                    value = x,
                                                    label = x.ToLabel()
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
                                            paramId = nameof(MpPrefViewModel.Instance.ThemeTypeName),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Theme Style",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.ThemeType == MpThemeType.Default,
                                                    value = MpThemeType.Default.ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.ThemeType == MpThemeType.Light,
                                                    value = MpThemeType.Light.ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.ThemeType == MpThemeType.Dark,
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
                                            paramId = nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Interface",
                                            description = "Requires restart :(",
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
                                            paramId = nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
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
                                            paramId = nameof(MpPrefViewModel.Instance.DefaultFontSize),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = "Size",
                                            values =
                                                new int[]{ 8, 9, 10, 12, 14, 16, 20, 24, 32, 42, 54, 68, 84, 98 }
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.DefaultFontSize == x,
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
                                            paramId = nameof(MpPrefViewModel.Instance.NotificationSoundGroupIdx),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.Integer,
                                            label = "Sound Theme",
                                            values =
                                                new int[]{ 0, 1, 2, 3 }
                                                .Select(x=>new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.NotificationSoundGroupIdx == x,
                                                    label = ((MpSoundGroupType)x).ToString(),
                                                    value = x.ToString(),
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.NotificationSoundVolume),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = "Notification Volume",
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.NotificationSoundVolume.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.ShowInTaskbar),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show In Taskbar",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ShowInTaskbar.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.ShowInTaskSwitcher),
                                            description = "Requires restart",
                                            isVisible = OperatingSystem.IsWindows(),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show In Task Switcher",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ShowInTaskSwitcher.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.MainWindowShowBehaviorType),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Show Behavior",
                                            values =
                                                Enum.GetNames(typeof(MpMainWindowShowBehaviorType))
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MainWindowShowBehaviorType.ToLower() == x.ToLower(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta),
                                            description = "Monkey Paste will be revealed by performing a scroll gesture on the top edge of your monitor",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show on top screen scroll",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.AnimateMainWindow),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Animate Window",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.AnimateMainWindow.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.GlobalBgOpacity),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Decimal,
                                            label = "Background Opacity",
                                            minimum = 0,
                                            maximum = 1,
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.GlobalBgOpacity.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.ShowHints),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show Hints",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ShowHints.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.International) {
                            IsVisible = false,
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.UserLanguageCode),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Language",
                                            values =
                                                MpCurrentCultureViewModel.Instance.AvailableCultureLookup
                                                .Select(x=>
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = x.Key == MpPrefViewModel.Instance.UserLanguageCode,
                                                    label = x.Value,
                                                    value = x.Key
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IsTextRightToLeft),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Right-to-left",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsTextRightToLeft.ToString()
                                                }
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        new MpAvSettingsFrameViewModel(MpSettingsFrameType.Limits) {
                            PluginFormat = new MpPluginFormat() {
                                headless = new MpHeadlessPluginFormat() {
                                    parameters = new List<MpParameterFormat>() {
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.MaxUndoLimit),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Undo Limit",
                                            description = "High undo limits may affect performance",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxUndoLimit.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxUndoLimit.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxUndoLimit.ToString() == "100",
                                                    value = "100".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxUndoLimit.ToString() == "-1",
                                                    value = "-1".ToString(),
                                                    label = "No Limit"
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.MaxPinClipCount),
                                            controlType = MpParameterControlType.Slider,
                                            unitType = MpParameterValueUnitType.Integer,
                                            minimum = 1,
                                            maximum = 50,
                                            label = "Pin Capacity",
                                            description = "Large or many staged items can consume significant memory. removal follows a first-in-first-out policy",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.MaxPinClipCount.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.IgnoreAppendedItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Appends",
                                            description = "When new clipboard content is appended it will not be tracked",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IgnoreAppendedItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IgnoreInternalClipboardChanges),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Internal Clipboard Tracking",
                                            description = "Clipboard changes from within the app will not be tracked",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IgnoreInternalClipboardChanges.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore Only White Space",
                                            description = "Spaces, tabs and new lines will be ignored from tracking",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.TrackExternalPasteHistory),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Paste History",
                                            description = "This setting will track system-wide keyboard paste counts for usage statistics. This information is private and not shared but also not guarenteed. Accurate results may require providing application specific keyboard paste command.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.TrackExternalPasteHistory.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IsDuplicateCheckEnabled),
                                            description = "When <b>duplicate</b> is detected on clipboard, it will be <b>staged</b> from original source and not added redundantly.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Ignore New Duplicates",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsDuplicateCheckEnabled.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.LoadOnLogin),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Load on Login",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.LoadOnLogin.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.AddClipboardOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Add Startup Clipboard",
                                            description = "On startup the source of the clipboard will be unknown and depending on your security settings the content may come from an excluded website or application.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.AddClipboardOnStartup.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.TrashCleanupModeTypeStr),
                                            description = $"Automatic trash cleanup will occur on startup at the set interval. It is not required but definitely don't let it get too large! Overall performance and CPU usage will have an impact if it becomes large ie. over 100 items lets say...",
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Trash Emptying",
                                            values =
                                                Enum.GetNames(typeof(MpTrashCleanupModeType))
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.TrashCleanupModeTypeStr.ToLower() == x.ToString().ToLower(),
                                                    value = x
                                                }).ToList()
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IsClipboardListeningOnStartup),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Listen to clipboard on startup",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsClipboardListeningOnStartup.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.MaxRecentTextsCount),
                                            controlType = MpParameterControlType.ComboBox,
                                            unitType = MpParameterValueUnitType.PlainText,
                                            label = "Recent Text Limit",
                                            description = "This applies to all auto-completable inputs, lower values will improve performance",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "10",
                                                    value = "10".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "50",
                                                    value = "50".ToString()
                                                },
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = MpPrefViewModel.Instance.MaxRecentTextsCount.ToString() == "100",
                                                    value = "100".ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IsAutoSearchEnabled),
                                            description = "Text typed will be automatically applied to a search when no text control is focused.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Auto Search Input",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsAutoSearchEnabled.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            // TODO this should probably be in a seperate frame but just testing editable list now
                                            paramId = nameof(MpPrefViewModel.Instance.UserDefinedFileExtensionsCsv),
                                            controlType = MpParameterControlType.EditableList,
                                            unitType = MpParameterValueUnitType.DelimitedPlainText,
                                            label = "User-Defined Search Extensions",
                                            description = "Used in advanced file search for non preset file kinds",
                                            values =
                                            new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.UserDefinedFileExtensionsCsv
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
                                            paramId = nameof(MpPrefViewModel.Instance.IsSpellCheckEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Enable Spell Check",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsSpellCheckEnabled.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.IsRichHtmlContentEnabled),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Rich Content",
                                            description = "Disabling rich text will significantly reduce memory consumption but will disable some advanced features, such as: templates, annotations, find/replace and drag drop",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.IsRichHtmlContentEnabled.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste),
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Reset Clipboard after Monkey Pasting",
                                            description = "<warning/>Monkey Paste uses the system clipboard to interoperate and changes your current clipboard when pasting to an external program. This will <b>attempt</b> to restore the state of the clipboard after an external paste. Restoration cannot be guaranteed and may reduce performance or even crash Monkey Paste depending on the source of the data (looking at you windows).",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste.ToString()
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
                                            paramId = nameof(MpPrefViewModel.Instance.ShowExternalDropWidget),
                                            description = "The drop widget is a floating menu showing while drag and dropping out of Monkey Paste that allows on-demand format conversion onto your drop target by hovering on or off of currently available formats. <b>Be aware conversion may not be an acceptable format for the target application so do not select 'YES' and remember the settings!</b>",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Drop Widget",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ShowExternalDropWidget.ToString()
                                                },
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = nameof(MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop),
                                            description = "This helps fluidly allow dropping data into Monkey paste from another application without extra fumbling with window placement by dragging data to the top of the screen which will activate Monkey Paste and allow drop.",
                                            controlType = MpParameterControlType.CheckBox,
                                            unitType = MpParameterValueUnitType.Bool,
                                            label = "Show on drag to top of screen",
                                            values =new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop.ToString()
                                                },
                                            }
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
                                            description = Mp.Services.PlatformResource.GetResource<string>("RoutingProfileInfoHtml"),
                                            values =
                                                Enum.GetNames(typeof(MpShortcutRoutingProfileType))
                                                .Select(x=> new MpPluginParameterValueFormat() {
                                                    isDefault = MpAvShortcutCollectionViewModel.Instance.RoutingProfileType.ToString() == x,
                                                    value = x,
                                                    label = x.ToLabel()
                                                }).ToList()
                                        }
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
                                            label = "Reset",
                                            description = "All ignored notifications previously set.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpRuntimePrefParamType.ResetNtf.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetShortcuts.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Reset",
                                            description = "All application shortcuts will be reset to their default key gestures.",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpRuntimePrefParamType.ResetShortcuts.ToString()
                                                }
                                            }
                                        },
                                        new MpParameterFormat() {
                                            paramId = MpRuntimePrefParamType.ResetPluginCache.ToString(),
                                            controlType = MpParameterControlType.Button,
                                            label = "Reset",
                                            description = "All plugins will be reset to initial default state",
                                            values = new List<MpPluginParameterValueFormat>() {
                                                new MpPluginParameterValueFormat() {
                                                    isDefault = true,
                                                    value = MpRuntimePrefParamType.ResetPluginCache.ToString()
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    .OrderBy(x=>x.FrameType.ToString())
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
            }
            UpdateFilters();
        }


        #endregion

        #region Private Methods

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
                Width = 660,
                Height = 500,
                Topmost = true,
                Title = "Settings".ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("CogColorImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = this,
                Content = new MpAvSettingsView()
            };
            sw.Classes.Add("fadeIn");

            void Sw_Opened(object sender, EventArgs e) {
                SetupFontFamilyComboBoxes();
                ProcessListenOnStartupChanged();
                AttachThemeButtonColorUpdate();
                AttachRoutingProfileSelectionChange();
                AttachAccountTypeSelectionChange();
            }
            void Sw_Closed(object sender, EventArgs e) {
                sw.Opened -= Sw_Opened;
                sw.Closed -= Sw_Closed;
            }

            sw.Opened += Sw_Opened;
            sw.Closed += Sw_Closed;
            sw.Topmost = true;
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
                MpAvAppCollectionViewModel.Instance.FilteredItems.Any() ||
                MpAvUrlCollectionViewModel.Instance.FilteredItems.Any();

            IsTabButtonVisible3 = MpAvShortcutCollectionViewModel.Instance.FilteredItems.Any();
            IsTabButtonVisible4 = true;

            AddOrUpdateRecentFilterTextsAsync(FilterText).FireAndForgetSafeAsync();
        }

        private async Task AddOrUpdateRecentFilterTextsAsync(string st) {
            while (MpPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentSettingsSearches = await MpPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpPrefViewModel.Instance.RecentSettingsSearchTexts), st);
        }

        #region Pref Handling
        private void MpPrefViewModel_Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpPrefViewModel.Instance.LoadOnLogin):
                    SetLoadOnLogin(MpPrefViewModel.Instance.LoadOnLogin);
                    break;
                case nameof(MpPrefViewModel.Instance.UserLanguageCode):
                    SetLanguage(MpPrefViewModel.Instance.UserLanguageCode);
                    break;
                case nameof(MpPrefViewModel.Instance.MaxUndoLimit):
                    MpAvUndoManagerViewModel.Instance.MaximumUndoLimit = MpPrefViewModel.Instance.MaxUndoLimit;
                    break;
                case nameof(MpPrefViewModel.Instance.NotificationSoundVolume):
                    if (MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                        // ignore preview volume while sliding
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        await MpAvSoundPlayerViewModel.Instance.UpdateVolumeCommand.ExecuteAsync(null);
                        MpAvSoundPlayerViewModel.Instance.PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Copy);
                    });

                    break;
                case nameof(MpPrefViewModel.Instance.MaxPinClipCount):
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.InternalPinnedItems));
                    break;
                case nameof(MpPrefViewModel.Instance.TrackExternalPasteHistory):
                    MpAvShortcutCollectionViewModel.Instance.InitExternalPasteTracking();
                    break;
                case nameof(MpPrefViewModel.Instance.TrashCleanupModeTypeStr):
                    MpAvTagTrayViewModel.Instance.SetNextTrashCleanupDate();
                    break;
                case nameof(MpPrefViewModel.Instance.IsClipboardListeningOnStartup):
                    ProcessListenOnStartupChanged();
                    break;
            }
            if (MpAvThemeViewModel.Instance.IsThemePref(e.PropertyName)) {
                MpAvThemeViewModel.Instance.SyncThemePrefs(true);
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

            MpPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

            MpConsole.WriteLine($"Load At Login: {(MpPrefViewModel.Instance.LoadOnLogin ? "ON" : "OFF")}");
        }

        #region Theme Button Color
        private void AttachThemeButtonColorUpdate() {
            Dispatcher.UIThread.Post(async () => {
                var tb = await GetParameterControlByParamIdAsync<Button>(MpRuntimePrefParamType.ThemeHexColor.ToString());
                if (tb == null) {
                    return;
                }
                tb.AttachedToVisualTree += Tb_AttachedToVisualTree;
                SetThemeButtonColor(tb);
            });
        }

        private void SetThemeButtonColor(Button tb) {
            if (tb == null) {
                return;
            }
            tb.Margin = new Thickness(0);
            //tb.Padding = new Thickness(0);
            tb.Content = new MpAvClipBorder() {
                //CornerRadius = tb.CornerRadius,
                BorderBrush = Brushes.Transparent,
                Background = MpPrefViewModel.Instance.ThemeColor.ToAvBrush(),
                Content = new TextBlock() {
                    Margin = new Thickness(5, 3),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = MpPrefViewModel.Instance.ThemeColor.ToContrastForegoundColor().ToAvBrush(),
                    Text = (tb.DataContext as MpAvParameterViewModelBase).Label
                }
            };
        }

        private void Tb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            SetThemeButtonColor(sender as Button);
        }

        #endregion

        #region Routing Profile 
        private void AttachRoutingProfileSelectionChange() {
            Dispatcher.UIThread.Post(async () => {
                var cmb = await GetParameterControlByParamIdAsync<ComboBox>(MpRuntimePrefParamType.ChangeRoutingType.ToString());
                if (cmb == null) {
                    return;
                }
                void RoutingSelectionChanged(object sender, SelectionChangedEventArgs e) {
                    if (cmb.SelectedIndex < 0) {
                        // occurs when filtered out
                        return;
                    }
                    MpShortcutRoutingProfileType sel_type = (MpShortcutRoutingProfileType)cmb.SelectedIndex;
                    MpAvShortcutCollectionViewModel.Instance.UpdateRoutingProfileCommand.Execute(sel_type);
                }
                cmb.SelectionChanged += RoutingSelectionChanged;
            });
        }

        private void SetRoutingProfileType(MpShortcutRoutingProfileType rpt) {
            Dispatcher.UIThread.Post(async () => {
                var cmb = await GetParameterControlByParamIdAsync<ComboBox>(MpRuntimePrefParamType.ChangeRoutingType.ToString());
                if (cmb == null) {
                    return;
                }
                cmb.SelectedIndex = (int)rpt;
            });
        }
        #endregion

        #region Account Type
        private void AttachAccountTypeSelectionChange() {
            Dispatcher.UIThread.Post(async () => {
                var cmb = await GetParameterControlByParamIdAsync<ComboBox>(MpRuntimePrefParamType.ChangeAccountType.ToString());
                if (cmb == null) {
                    return;
                }
                void AccountTypeSelectionChanged(object sender, SelectionChangedEventArgs e) {
                    MpUserAccountType sel_type = (MpUserAccountType)cmb.SelectedIndex;
                    Mp.Services.AccountTools.SetAccountType(sel_type);
                }
                cmb.SelectionChanged += AccountTypeSelectionChanged;
            });
        }

        private void SetAccountType(MpUserAccountType at) {
            Dispatcher.UIThread.Post(async () => {
                var cmb = await GetParameterControlByParamIdAsync<ComboBox>(MpRuntimePrefParamType.ChangeAccountType.ToString());
                if (cmb == null) {
                    return;
                }
                cmb.SelectedIndex = (int)at;
            });
        }
        #endregion

        private void ProcessListenOnStartupChanged() {
            Dispatcher.UIThread.Post(async () => {
                var add_startup_item_cb = await GetParameterControlByParamIdAsync<CheckBox>(nameof(MpPrefViewModel.Instance.AddClipboardOnStartup));
                if (add_startup_item_cb == null) {
                    return;
                }
                add_startup_item_cb.IsEnabled = MpPrefViewModel.Instance.IsClipboardListeningOnStartup;
            });
        }

        #region Font Helpers
        private void SetupFontFamilyComboBoxes() {
            Dispatcher.UIThread.Post(async () => {
                var ro_cb = await GetParameterControlByParamIdAsync<ComboBox>(nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily));
                if (ro_cb != null) {
                    SetupFontFamilyComboBox(ro_cb);
                }

                var e_cb = await GetParameterControlByParamIdAsync<ComboBox>(nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily));
                if (e_cb != null) {
                    SetupFontFamilyComboBox(e_cb);
                }
            });
        }


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
        private async Task<T> GetParameterControlByParamIdAsync<T>(string paramId) where T : Visual {
            if (!IsWindowOpen) {
                return null;
            }
            var param_tuple = GetParamAndFrameByParamId(paramId);
            if (param_tuple == null) {
                return default;
            }

            Window w = MpAvWindowManager.LocateWindow(this);
            while (w == null) {
                await Task.Delay(100);
                w = MpAvWindowManager.LocateWindow(this);
            }

            var param_vm = param_tuple.Item2;
            T param_control_of_t = w.GetSelfAndLogicalDescendants().OfType<T>().FirstOrDefault(x => x.DataContext == param_vm);
            while (param_control_of_t == null) {
                await Task.Delay(100);
                if (!w.IsInitialized || !w.IsLoaded) {
                    // pref tab never selected and closed so stop it
                    return default;
                }
                param_control_of_t = w.GetSelfAndLogicalDescendants().OfType<T>().FirstOrDefault(x => x.DataContext == param_vm);
            }
            return param_control_of_t;
        }

        private Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> GetParamAndFrameByParamId(string paramId) {
            if (Items.FirstOrDefault(
                        x => x.Items.Any(
                            y => y.ParamId.ToString().ToLower() == paramId.ToLower()))
                        is MpAvSettingsFrameViewModel frame_vm &&
                        frame_vm.Items.FirstOrDefault(x => x.ParamId.ToString().ToLower() == paramId.ToLower())
                        is MpAvParameterViewModelBase param_vm) {
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
                    title: "Confirm",
                    message: "Are you sure you want to reset all preferences? This action cannot be undone.",
                    iconResourceObj: "WarningImage",
                    owner: MpAvWindowManager.LocateWindow(this));

                if (!result) {
                    // canceled reset all, ignore
                    return;
                }
                MpPrefViewModel.Instance.RestoreDefaultsCommand.Execute(null);
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
                int tab_idx = (int)DEFAULT_SELECTED_TAB;
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
                    GetParamAndFrameByParamId(focus_param_id) is Tuple<MpAvSettingsFrameViewModel, MpAvParameterViewModelBase> focus_tuple) {
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
                        var param_view = await GetParameterControlByParamIdAsync<MpAvPluginParameterItemView>(focus_param_id);
                        while (true) {
                            if (param_view != null && param_view.IsVisible) {
                                break;
                            }
                            if (param_view == null) {
                                param_view = await GetParameterControlByParamIdAsync<MpAvPluginParameterItemView>(focus_param_id);
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
                    case MpRuntimePrefParamType.ResetNtf: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: "Confirm",
                            message: "Are you sure you want to reset all notifications?",
                            iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;
                            break;
                        }

                    case MpRuntimePrefParamType.ResetPluginCache: {
                            var result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: "Confirm",
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
                                    selectedColor: MpPrefViewModel.Instance.ThemeColor,
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

                            MpPrefViewModel.Instance.ThemeColor = result;
                            Dispatcher.UIThread.Post(async () => {
                                var tb = await GetParameterControlByParamIdAsync<Button>(MpRuntimePrefParamType.ThemeHexColor.ToString());
                                if (tb == null) {
                                    return;
                                }
                                SetThemeButtonColor(tb);
                            });

                            break;
                        }
                }
            });

        public ICommand ClearFilterTextCommand => new MpCommand(
            () => {
                FilterText = string.Empty;
            });
        #endregion
    }
}
