using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpButtonCommandPrefType {
        None = 0,
        ResetNtf,
        ResetPluginCache
    }

    public class MpAvPreferencesMenuViewModel :
        MpViewModelBase {
        #region Private Variables

        #endregion
        #region Statics

        private static MpAvPreferencesMenuViewModel _instance;
        public static MpAvPreferencesMenuViewModel Instance => _instance ?? (_instance = new MpAvPreferencesMenuViewModel());


        #endregion
        #region Properties


        //private ObservableCollection<string> _languages = null;
        //public ObservableCollection<string> Languages {
        //    get {
        //        if (_languages == null) {
        //            _languages = new ObservableCollection<string>();
        //            foreach (var lang in MpLanguageTranslator.LanguageList) {
        //                _languages.Add(lang);
        //            }
        //        }
        //        return _languages;
        //    }
        //}


        #region View Models
        public ObservableCollection<MpAvPreferenceFrameViewModel> Items { get; set; } = new ObservableCollection<MpAvPreferenceFrameViewModel>();

        public MpAvPreferenceFrameViewModel LookAndFeelFrame { get; set; }
        public MpAvPreferenceFrameViewModel InternationalFrame { get; set; }
        public MpAvPreferenceFrameViewModel ContentFrame { get; set; }
        public MpAvPreferenceFrameViewModel HistoryFrame { get; set; }
        public MpAvPreferenceFrameViewModel SystemFrame { get; set; }

        #endregion
        #endregion

        #region Constructors
        public MpAvPreferencesMenuViewModel() : base(null) {
            MpPrefViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
            PropertyChanged += MpAvPreferencesMenuViewModel_PropertyChanged;
            InitializeAsync().FireAndForgetSafeAsync(this);
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync() {

            #region look & feel

            LookAndFeelFrame = new MpAvPreferenceFrameViewModel(this) {
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
                                        isDefault = MpPrefViewModel.Instance.ShowInTaskSwitcher.ToString() == MpThemeType.Light.ToString(),
                                        value = MpThemeType.Light.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        isDefault = MpPrefViewModel.Instance.ShowInTaskSwitcher.ToString() == MpThemeType.Dark.ToString(),
                                        value = MpThemeType.Dark.ToString()
                                    },
                                }
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
                                paramId = nameof(MpPrefViewModel.Instance.MainWindowOpacity),
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Decimal,
                                label = "Background Opacity",
                                minimum = 0,
                                maximum = 1,
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpPrefViewModel.Instance.MainWindowOpacity.ToString()
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

            InternationalFrame = new MpAvPreferenceFrameViewModel(this) {
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

            #region Content

            ContentFrame = new MpAvPreferenceFrameViewModel(this) {
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
                            }
                        }
                    }
                }
            };

            Items.Add(ContentFrame);

            #endregion

            #region History

            HistoryFrame = new MpAvPreferenceFrameViewModel(this) {
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

            SystemFrame = new MpAvPreferenceFrameViewModel(this) {
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

        private void MpAvPreferencesMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //}
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpPrefViewModel.Instance.LoadOnLogin):
                    SetLoadOnLogin(MpPrefViewModel.Instance.LoadOnLogin);
                    break;
                case nameof(MpPrefViewModel.Instance.UserLanguageCode):
                    SetLanguage(MpPrefViewModel.Instance.UserLanguageCode);
                    break;
            }
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
#endif
            MpPrefViewModel.Instance.LoadOnLogin = loadOnLogin;

            MpConsole.WriteLine("App " + appName + " with path " + appPath + " has load on login set to: " + loadOnLogin);
        }


        #endregion

        #region Commands

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
}
