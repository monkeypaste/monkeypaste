using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
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
        MpIChildWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIActiveWindowViewModel {
        #region Private Variables

        private string[] _reinitContentParams = new string[] {
            nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
            nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily),
            nameof(MpPrefViewModel.Instance.DefaultFontSize),
            nameof(MpPrefViewModel.Instance.IsSpellCheckEnabled),
            nameof(MpPrefViewModel.Instance.CurrentThemeName),
            nameof(MpPrefViewModel.Instance.GlobalBgOpacity),
        };
        #endregion

        #region Statics

        private static MpAvSettingsViewModel _instance;
        public static MpAvSettingsViewModel Instance => _instance ?? (_instance = new MpAvSettingsViewModel());


        #endregion

        #region Interfaces
        #region MpIWindowViewModel Implementatiosn
        public MpWindowType WindowType =>
            MpWindowType.Main;

        public bool IsOpen { get; set; }

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation 
        public bool WantsTopmost =>
            true;

        #endregion
        #region MpIActiveWindowViewModel Implementation
        public bool IsActive { get; set; }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvSettingsFrameViewModel> Items { get; set; } = new ObservableCollection<MpAvSettingsFrameViewModel>();
        #region Account

        public IEnumerable<MpAvSettingsFrameViewModel> AccountItems =>
            Items
            .Where(x => x.TabType == MpSettingsTabType.Account)
            .OrderBy(x => x.SortOrderIdx)
            .ToList();

        public MpAvSettingsFrameViewModel AccountFrame { get; set; }

        #endregion

        #region Preferences
        public IEnumerable<MpAvSettingsFrameViewModel> PreferenceItems =>
            Items
            .Where(x => x.TabType == MpSettingsTabType.Preferences)
            .OrderBy(x => x.SortOrderIdx)
            .ToList();

        public MpAvSettingsFrameViewModel LookAndFeelFrame { get; set; }
        public MpAvSettingsFrameViewModel InternationalFrame { get; set; }
        public MpAvSettingsFrameViewModel ContentFrame { get; set; }
        public MpAvSettingsFrameViewModel HistoryFrame { get; set; }
        public MpAvSettingsFrameViewModel SystemFrame { get; set; }

        public MpAvSettingsFrameViewModel InteropFrame { get; set; }
        #endregion

        #region Shortcuts


        #endregion

        #endregion

        #region State

        public string FilterText { get; set; }


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
            IsTabSelected = new ObservableCollection<bool>(Enumerable.Repeat(false, 6));
            IsTabSelected.CollectionChanged += IsTabSelected_CollectionChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            #region Account

            AccountFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Account,
                SortOrderIdx = 1,
                LabelText = "Account",
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

                            }
                        }
                    }
                }
            };

            Items.Add(AccountFrame);

            #endregion

            #region Preferences

            #region look & feel

            LookAndFeelFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 0,
                LabelText = "Look & Feel",
                PluginFormat = new MpPluginFormat() {
                    headless = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.CurrentThemeName),
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                label = "Theme",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.CurrentThemeName.ToLower() == MpThemeType.Light.ToString().ToLower(),
                                        value = MpThemeType.Light.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.CurrentThemeName.ToLower() == MpThemeType.Dark.ToString().ToLower(),
                                        value = MpThemeType.Dark.ToString()
                                    },
                                }
                            },
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                label = "UI Font Family",
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
                                label = "Content Font Family",
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
                                label = "Content Font Size",
                                values =
                                    new int[]{ 8, 9, 10, 12, 14, 16, 20, 24, 32, 42, 54, 68, 84, 98 }
                                    .Select(x=>new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.DefaultFontSize == x,
                                        value = x.ToString(),
                                    }).ToList()
                            },
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
                            },
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
            };

            Items.Add(LookAndFeelFrame);

            #endregion

            #region International

            InternationalFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 1,
                IsVisible = false,
                LabelText = "International Settings",
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

                            }
                        }
                    }
                }
            };

            Items.Add(InternationalFrame);

            #endregion

            #region History

            HistoryFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 3,
                LabelText = "History",
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
                                paramId = nameof(MpPrefViewModel.Instance.MaxStagedClipCount),
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Integer,
                                minimum = 1,
                                maximum = 50,
                                label = "Max Staged Clips",
                                description = "Large or many staged items can consume significant memory. removal follows a first-in-first-out policy",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpPrefViewModel.Instance.MaxStagedClipCount.ToString()
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
                            }
                        }
                    }
                }
            };

            Items.Add(HistoryFrame);

            #endregion

            #region System

            SystemFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 4,
                LabelText = "System",
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
                                paramId = MpButtonCommandPrefType.ResetNtf.ToString(),
                                controlType = MpParameterControlType.Button,
                                label = "Reset Notifications",
                                description = "All ignored notifications previously set.",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpButtonCommandPrefType.ResetNtf.ToString()
                                    }
                                }
                            },
                            new MpParameterFormat() {
                                paramId = MpButtonCommandPrefType.ResetPluginCache.ToString(),
                                controlType = MpParameterControlType.Button,
                                label = "Reset Plugins",
                                description = "All plugins will be reset to initial default state",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpButtonCommandPrefType.ResetPluginCache.ToString()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Items.Add(SystemFrame);

            #endregion

            #region Content

            ContentFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 2,
                LabelText = "Content",
                PluginFormat = new MpPluginFormat() {
                    headless = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.IsDuplicateCheckEnabled),
                                description = "When <b>duplicate</b> is detected on clipboard, it will be <b>staged</b> from original source and not added redundantly.",
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                label = "Ignore New Duplicates",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.ShowInTaskSwitcher.ToString() == MpThemeType.Light.ToString(),
                                        value = MpPrefViewModel.Instance.IsDuplicateCheckEnabled.ToString()
                                    },
                                }
                            },
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.IsSpellCheckEnabled),
                                controlType = MpParameterControlType.CheckBox,
                                unitType = MpParameterValueUnitType.Bool,
                                label = "Content Spell Check",
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
            };

            Items.Add(ContentFrame);

            #endregion

            #region Interop

            InteropFrame = new MpAvSettingsFrameViewModel() {
                TabType = MpSettingsTabType.Preferences,
                SortOrderIdx = 1,
                LabelText = "Input",
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
                                paramId = nameof(MpPrefViewModel.Instance.MainWindowShowBehaviorType),
                                controlType = MpParameterControlType.ComboBox,
                                unitType = MpParameterValueUnitType.PlainText,
                                label = "Show Behavior",
                                values =
                                    Enum.GetNames(typeof(MpMainWindowShowBehaviorType))
                                    .Select(x=> new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.MainWindowShowBehaviorType.ToLower() == x,
                                        value = x
                                    }).ToList()
                            }
                        }
                    }
                }
            };

            Items.Add(InteropFrame);

            #endregion

            #endregion


            foreach (var fvm in Items) {
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
        }


        #endregion

        #region Private Methods

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
                case nameof(MpPrefViewModel.Instance.MaxStagedClipCount):
                    MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.InternalPinnedItems));
                    break;
            }
            if (_reinitContentParams.Any(x => x.ToLower() == e.PropertyName.ToLower())) {
                Task.WhenAll(MpAvClipTrayViewModel.Instance.AllActiveItems
                    .Where(x => x.GetContentView() != null)
                    .Select(x => x.GetContentView().ReloadAsync())).FireAndForgetSafeAsync();
            }
        }
        private void IsTabSelected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTabSelected));
        }

        private void SetLanguage(string cultureCode) {
            MpCurrentCultureViewModel.Instance.SetLanguageCommand.Execute(cultureCode);
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            if (!Mp.Services.ProcessWatcher.IsAdmin(Mp.Services.ProcessWatcher.ThisAppHandle)) {
                //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
                return;
            }
#if WINDOWS
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string appPath = Mp.Services.PlatformInfo.ExecutingPath;
            if (loadOnLogin) {
                rk.SetValue(appName, appPath);
            } else {
                rk.DeleteValue(appName, false);
            }
#else
            // TODO add other os'
            loadOnLogin = false;
#endif
            MpPrefViewModel.Instance.LoadOnLogin = loadOnLogin;

            MpConsole.WriteLine($"Load At Login: {(loadOnLogin ? "ON" : "OFF")}");
        }

        #endregion
        private void MpAvSettingsWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {

            switch (e.PropertyName) {
                case nameof(FilterText):
                    MpMessenger.SendGlobal(MpMessageType.SettingsFilterTextChanged);
                    if (FilterText.StartsWith("#")) {
                        var test = MpAvWindowManager.FindByHashCode(FilterText);
                    }

                    break;
                case nameof(IsOpen):
                    MpConsole.WriteLine($"Settings window: {(IsOpen ? "OPEN" : "CLOSED")}");
                    break;

                case nameof(IsActive):
                    MpConsole.WriteLine($"Settings window: {(IsActive ? "ACTIVE" : "INACTIVE")}");
                    break;
            }
        }

        private void Sw_Opened(object sender, EventArgs e) {
            var sw = sender as Window;
            SetParamClasses<ComboBox>(sw, nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily), "fontChooser");
            SetParamClasses<ComboBox>(sw, nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily), "fontChooser");

        }

        private void SetParamClasses<T>(Window w, string paramId, string classes) where T : Visual {
            if (w == null) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var ff_vm = LookAndFeelFrame.Items.FirstOrDefault(x => x.ParamId.ToString().ToLower() == paramId.ToLower());
                if (ff_vm != null) {
                    T ff_cb = w.GetVisualDescendants<T>().FirstOrDefault(x => x.DataContext == ff_vm);
                    while (ff_cb == null) {
                        await Task.Delay(100);
                        ff_cb = w.GetVisualDescendants<T>().FirstOrDefault(x => x.DataContext == ff_vm);
                        if (!w.IsInitialized) {
                            // pref tab never selected and closed so stop it
                            return;
                        }
                    }
                    ff_cb.Classes.Add("fontChooser");

                }
            });
        }

        #endregion

        #region Commands

        public ICommand ResetSettingsCommand => new MpCommand(
            () => {

            });
        public ICommand SaveSettingsCommand => new MpCommand(
            () => {
                IsOpen = false;
            });

        public ICommand CancelSettingsCommand => new MpCommand(
            () => {
                IsOpen = false;
            });
        public ICommand SelectTabCommand => new MpCommand<object>(
            (args) => {
                int tab_idx = 0;
                if (args is int intArg) {
                    tab_idx = intArg;
                } else if (args is string strArg) {
                    try {
                        tab_idx = int.Parse(strArg);
                    }
                    catch { }
                }

                SelectedTabIdx = tab_idx;
            });

        public ICommand ToggleShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                if (IsOpen) {
                    IsOpen = false;
                    return;
                }
                ShowSettingsWindowCommand.Execute(null);
            });
        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                SelectTabCommand.Execute(args);
                if (IsOpen) {
                    IsActive = true;
                    return;
                }
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    var sw = new MpAvWindow() {
                        ShowInTaskbar = true,
                        Width = 800,
                        Height = 500,
                        Topmost = true,
                        Title = "Settings".ToWindowTitleText(),
                        Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("CogColorImage", typeof(WindowIcon), null, null) as WindowIcon,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowState = WindowState.Normal,
                        DataContext = this,
                        Content = new MpAvSettingsView()
                    };
                    sw.Classes.Add("fadeIn");
                    sw.Opened += Sw_Opened;
                    sw.ShowChild();

                    MpMessenger.SendGlobal(MpMessageType.SettingsWindowOpened);
                }
            });


        public ICommand ButtonParameterClickCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpButtonCommandPrefType cmdType = MpButtonCommandPrefType.None;
                if (args is string strArg &&
                    strArg.ToEnum<MpButtonCommandPrefType>() is MpButtonCommandPrefType typeArg) {
                    cmdType = typeArg;
                } else if (args is MpButtonCommandPrefType) {
                    cmdType = (MpButtonCommandPrefType)args;
                }
                if (cmdType == MpButtonCommandPrefType.None) {
                    return;
                }
                switch (cmdType) {
                    case MpButtonCommandPrefType.ResetNtf: {
                            var result = await Mp.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync(
                            title: "Confirm",
                            message: "Are you sure you want to reset all notifications?",
                            iconResourceObj: "QuestionMarkImage");
                            if (!result) {
                                return;
                            }
                            MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;
                            break;
                        }

                    case MpButtonCommandPrefType.ResetPluginCache: {
                            var result = await Mp.Services.NativeMessageBox.ShowOkCancelMessageBoxAsync(
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
                                MpNotificationBuilder.ShowMessageAsync(
                                    title: "Error",
                                    body: $"Could not delete plugin cache from path: '{cache_dir}'",
                                    msgType: MpNotificationType.FileIoError).FireAndForgetSafeAsync(this);
                            }

                            break;
                        }

                }
            });
        #endregion
    }

    public enum MpButtonCommandPrefType {
        None = 0,
        ResetNtf,
        ResetPluginCache,
        AccountRegister,
        AccountSignIn,
        AccountClick,
        AccountSignOut,
    }

    public enum MpSettingsTabType {
        None = 0,
        Account,
        Preferences,
        Interop,
        Security,
        Shortcuts,
        Help
    }
}
