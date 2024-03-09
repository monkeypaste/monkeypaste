using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

#if CEFNET_WV
#endif

namespace MonkeyPaste.Avalonia {

    public class MpAvSystemTrayViewModel : MpAvViewModelBase {

        #region Private Variables
        #endregion

        #region Statics


        private static MpAvSystemTrayViewModel _instance;
        public static MpAvSystemTrayViewModel Instance => _instance ?? (_instance = new MpAvSystemTrayViewModel());

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public MpAvMenuItemViewModel TrayMenuItemViewModel {
            get {
                var tmivm = new MpAvMenuItemViewModel() {
                    TooltipSrcObj = this,
                    TooltipPropPath = nameof(SystemTrayTooltip),
                    IconSrcBindingObj = this,
                    IconPropPath = nameof(SystemTrayIconResourceObj),
                    CommandPath = nameof(TrayIconClickCommand),
                    CommandSrcObj = this,
                    SubItems = new List<MpAvMenuItemViewModel>() {

                        // SHOW/HIDE MW

                        new MpAvMenuItemViewModel() {
                            HeaderSrcObj = MpAvMainWindowViewModel.Instance,
                            HeaderPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideLabel),
                            IconSrcBindingObj = MpAvMainWindowViewModel.Instance,
                            IconPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideIconSourceObj),
                            CommandSrcObj = MpAvMainWindowViewModel.Instance,
                            CommandPath = nameof(MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand),
                            //ShortcutArgs = new object[] { MpShortcutType.ToggleMainWindow },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleMainWindow),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)

                        },

                        // PAUSE/RESUME CB

                        new MpAvMenuItemViewModel() {
                            HeaderSrcObj = MpAvClipTrayViewModel.Instance,
                            HeaderPropPath = nameof(MpAvClipTrayViewModel.Instance.PlayOrPauseLabel),
                            IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                            IconPropPath = nameof(MpAvClipTrayViewModel.Instance.PlayOrPauseIconResoureKey),
                            CommandSrcObj = MpAvClipTrayViewModel.Instance,
                            CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleIsAppPausedCommand),
                            CommandParameter = "Click",
                            //ShortcutArgs = new object[] { MpShortcutType.ToggleListenToClipboard },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleListenToClipboard),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        },

                        // MODE SUB-MENU

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayModeHeader,
                            //IconResourceKey = "RobotArmColorImage",
                            SubItems = new List<MpAvMenuItemViewModel>() {

                                // APPEND INLINE 

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayAppenInlineHeader,
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendInsertMode),
                                    IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                                    IconPropPath = nameof(MpAvClipTrayViewModel.Instance.AppendInlineSysTrayIconSourceObj),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendInsertModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAppendInsertMode },
                                    
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendInlineMode),
                                    InputGesturePropPath = nameof(MpAvShortcutViewModel.KeyString)
                                },

                                // APPEND LINE

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayAppendLineHeader,
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendLineMode),
                                    IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                                    IconPropPath = nameof(MpAvClipTrayViewModel.Instance.AppendLineSysTrayIconSourceObj),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAppendLineMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendBlockMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },
                                new MpAvMenuItemViewModel() {IsSeparator = true},

                                // AUTO-COPY

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayAutoCopyHeader,
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAutoCopyMode),
                                    IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                                    IconPropPath = nameof(MpAvClipTrayViewModel.Instance.AutoCopySysTrayIconSourceObj),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAutoCopyMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAutoCopyMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },

                                // RIGHT-CLICK PASTE

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayRightClickPasteHeader,
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsRightClickPasteMode),
                                    IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                                    IconPropPath = nameof(MpAvClipTrayViewModel.Instance.RightClickPasteSysTrayIconSourceObj),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleRightClickPasteMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleRightClickPasteMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },
                            }
                        },

                        new MpAvMenuItemViewModel() {IsSeparator = true},

                        // PLUGIN BROWSER

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayPluginBrowserLabel,
                            IconResourceKey = "JigsawImage",
                            CommandSrcObj = MpAvPluginBrowserViewModel.Instance,
                            CommandPath = nameof(MpAvPluginBrowserViewModel.Instance.ShowPluginBrowserCommand)
                        },
                        
                        // SETTINGS

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonSettingsTitle,
                            IconSourceObj = "CogColorImage",
                            CommandSrcObj = MpAvSettingsViewModel.Instance,
                            CommandPath = nameof(MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand),
                            //ShortcutArgs = new object[] { MpShortcutType.ShowSettings },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ShowSettings),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        },
                        new MpAvMenuItemViewModel() {IsSeparator = true},

                        // HELP

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SettingsHelpTabLabel,
                            IconResourceKey = MpAvHelpViewModel.HELP_ICON_KEY,
                            CommandSrcObj = MpAvHelpViewModel.Instance,
                            CommandPath = nameof(MpAvHelpViewModel.Instance.NavigateToHelpLinkCommand),
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.OpenHelp),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        },


                        // RATE APP

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayRateAppLabel,
                            IconResourceKey = "StarYellowImage",
                            CommandSrcObj = MpAvAccountViewModel.Instance,
                            CommandPath = nameof(MpAvAccountViewModel.Instance.RateAppCommand)
                        },
                        
                        // CHECK FOR UPDATE

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayCheckForUpdateLabel,
                            IconResourceKey = "RadarImage",
                            CommandSrcObj = MpAvThisAppVersionViewModel.Instance,
                            CommandPath = nameof(MpAvThisAppVersionViewModel.Instance.CheckForUpdateCommand),
                            CommandParameter = "Click"
                        },
                        
                        // CHECK FOR UPDATE

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayFeebackLabel,
                            IconResourceKey = "LetterImage",
                            Command = MpAvUriNavigator.Instance.NavigateToUriCommand,
                            CommandParameter = MpServerConstants.SUPPORT_EMAIL_URI
                        },

                        // ABOUT

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayAboutHeader,
                            IconResourceKey = "InfoImage",
                            Command = MpAvAboutViewModel.Instance.ShowAboutWindowCommand
                        },
#region DEBUG STUFF
#if DEBUG && DESKTOP
                        new MpAvMenuItemViewModel() {IsSeparator = true},
                        new MpAvMenuItemViewModel() {
                            Header = "Show Converter DevTools",
                            Command = MpAvPlainHtmlConverter.Instance.ShowConverterDevTools,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Open Cef Uri",
                            Command = NavigateToCefNetUriCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Duplicate storage to desktop",
                            Command = CreateLocalStorageCopyCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            HeaderSrcObj = MpAvShortcutCollectionViewModel.Instance,
                            HeaderPropPath = nameof(MpAvShortcutCollectionViewModel.Instance.HookPauseLabel),
                            CommandSrcObj = MpAvShortcutCollectionViewModel.Instance,
                            CommandPath = nameof(MpAvShortcutCollectionViewModel.Instance.ToggleGlobalHooksCommand)
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 1",
                            Command = GenericTestCommand1,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 2",
                            Command = GenericTestCommand2,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 3",
                            Command = GenericTestCommand3,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 4",
                            Command = GenericTestCommand4,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 5",
                            Command = GenericTestCommand5,
                        },
                        //new MpMenuItemViewModel() {
                        //    Header = "Show Notifier DevTools",
                        //    Command = MpAvClipTrayViewModel.Instance.ShowAppendDevToolsCommand
                        //},
#endif
#endregion
                        new MpAvMenuItemViewModel() {IsSeparator = true},

                        // QUIT

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayQuitHeader,
                            IconResourceKey = "SignOutImage",
                            Command = ExitApplicationCommand,
                            CommandParameter = "systray menu click",
                            //ShortcutArgs = new object[] { MpShortcutType.ExitApplication },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ExitApplication),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        }
                    }
                };
                return tmivm;
            }
        }
        public MpAvMenuItemViewModel TrayMenuItemViewModel2 {
            get {
                var tmivm = new MpAvMenuItemViewModel() {
                    TooltipSrcObj = this,
                    TooltipPropPath = nameof(SystemTrayTooltip),
                    IconSrcBindingObj = this,
                    IconPropPath = nameof(SystemTrayIconResourceObj),
                    CommandPath = nameof(TrayIconClickCommand),
                    CommandSrcObj = this,
                    SubItems = new List<MpAvMenuItemViewModel>() {

                        // SHOW/HIDE MW

                        new MpAvMenuItemViewModel() {
                            Header = MpAvMainWindowViewModel.Instance.ShowOrHideLabel,
                            IconSourceObj = MpAvMainWindowViewModel.Instance.ShowOrHideIconSourceObj,
                            Command = MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand,
                            ShortcutArgs = new object[] { MpShortcutType.ToggleMainWindow },

                        },

                        // PAUSE/RESUME CB

                        new MpAvMenuItemViewModel() {
                            Header = MpAvClipTrayViewModel.Instance.PlayOrPauseLabel,
                            IconSourceObj = MpAvClipTrayViewModel.Instance.PlayOrPauseIconResoureKey,
                            Command = MpAvClipTrayViewModel.Instance.ToggleIsAppPausedCommand,
                            CommandParameter = "Click",
                            ShortcutArgs = new object[] { MpShortcutType.ToggleListenToClipboard }
                        },

                        // MODE SUB-MENU

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayModeHeader,
                            //IconResourceKey = "RobotArmColorImage",
                            SubItems = new List<MpAvMenuItemViewModel>() {

                                // APPEND INLINE 

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayAppenInlineHeader,
                                    IsChecked = MpAvClipTrayViewModel.Instance.IsAppendInsertMode,
                                    IconSourceObj = MpAvClipTrayViewModel.Instance.AppendInlineSysTrayIconSourceObj,
                                    ToggleType = "Radio",
                                    Command = MpAvClipTrayViewModel.Instance.ToggleAppendInsertModeCommand,
                                },

                                // APPEND LINE

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayAppendLineHeader,
                                    IsChecked = MpAvClipTrayViewModel.Instance.IsAppendLineMode,
                                    IconSourceObj = MpAvClipTrayViewModel.Instance.AppendLineSysTrayIconSourceObj,
                                    ToggleType = "Radio",
                                    Command = MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAppendBlockMode },
                                },

                                // AUTO-COPY

                                new MpAvMenuItemViewModel() {
                                    HasLeadingSeparator = true,
                                    Header = UiStrings.SysTrayAutoCopyHeader,
                                    IsChecked = MpAvClipTrayViewModel.Instance.IsAutoCopyMode,
                                    IconSourceObj = MpAvClipTrayViewModel.Instance.AutoCopySysTrayIconSourceObj,
                                    ToggleType = "CheckBox",
                                    Command = MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAutoCopyMode },
                                },

                                // RIGHT-CLICK PASTE

                                new MpAvMenuItemViewModel() {
                                    Header = UiStrings.SysTrayRightClickPasteHeader,
                                    IsChecked = MpAvClipTrayViewModel.Instance.IsRightClickPasteMode,
                                    IconSourceObj = MpAvClipTrayViewModel.Instance.RightClickPasteSysTrayIconSourceObj,
                                    ToggleType = "CheckBox",
                                    Command = MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleRightClickPasteMode },
                                },
                            }
                        },

                        // PLUGIN BROWSER

                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.SysTrayPluginBrowserLabel,
                            IconResourceKey = "JigsawImage",
                            Command = MpAvPluginBrowserViewModel.Instance.ShowPluginBrowserCommand
                        },
                        
                        // SETTINGS

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonSettingsTitle,
                            IconSourceObj = "CogColorImage",
                            Command = MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand,
                            ShortcutArgs = new object[] { MpShortcutType.ShowSettings },
                        },

                        // HELP

                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.SettingsHelpTabLabel,
                            IconResourceKey = MpAvHelpViewModel.HELP_ICON_KEY,
                            Command = MpAvHelpViewModel.Instance.NavigateToHelpLinkCommand,
                            ShortcutArgs = new object[] { MpShortcutType.OpenHelp },
                        },


                        // RATE APP

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayRateAppLabel,
                            IconResourceKey = "StarYellowImage",
                            Command = MpAvAccountViewModel.Instance.RateAppCommand
                        },
                        
                        // CHECK FOR UPDATE

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayCheckForUpdateLabel,
                            IconResourceKey = "RadarImage",
                            Command = MpAvThisAppVersionViewModel.Instance.CheckForUpdateCommand,
                            CommandParameter = "Click"
                        },
                        
                        // CHECK FOR UPDATE

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayFeebackLabel,
                            IconResourceKey = "LetterImage",
                            Command = MpAvUriNavigator.Instance.NavigateToUriCommand,
                            CommandParameter = MpServerConstants.SUPPORT_EMAIL_URI
                        },

                        // ABOUT

                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.SysTrayAboutHeader,
                            IconResourceKey = "InfoImage",
                            Command = MpAvAboutViewModel.Instance.ShowAboutWindowCommand
                        },
#region DEBUG STUFF
#if DEBUG && DESKTOP
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = "Show Converter DevTools",
                            Command = MpAvPlainHtmlConverter.Instance.ShowConverterDevTools,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Open Cef Uri",
                            Command = NavigateToCefNetUriCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Duplicate storage to desktop",
                            Command = CreateLocalStorageCopyCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = MpAvShortcutCollectionViewModel.Instance.HookPauseLabel,
                            Command = MpAvShortcutCollectionViewModel.Instance.ToggleGlobalHooksCommand
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 1",
                            Command = GenericTestCommand1,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 2",
                            Command = GenericTestCommand2,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 3",
                            Command = GenericTestCommand3,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 4",
                            Command = GenericTestCommand4,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Test Command 5",
                            Command = GenericTestCommand5,
                        },
                        //new MpMenuItemViewModel() {
                        //    Header = "Show Notifier DevTools",
                        //    Command = MpAvClipTrayViewModel.Instance.ShowAppendDevToolsCommand
                        //},
#endif
#endregion

                        // QUIT

                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.SysTrayQuitHeader,
                            IconResourceKey = "SignOutImage",
                            Command = ExitApplicationCommand,
                            CommandParameter = "systray menu click",
                            ShortcutArgs = new object[] { MpShortcutType.ExitApplication },
                        }
                    }
                };
                return tmivm;
            }
        }
        #endregion

        #region State

        public bool IsSystemTrayItemsEnabled =>
            Mp.Services.StartupState.IsReady;

        #endregion

        #region Appearance
        public string SystemTrayTooltip {
            get {
                if (Mp.Services == null ||
                    Mp.Services.StartupState == null ||
                    !Mp.Services.StartupState.IsReady) {
                    return UiStrings.SysTrayPleaseWaitTooltip;
                }
                if (MpAvThisAppVersionViewModel.Instance.IsOutOfDate) {
                    return string.Format(UiStrings.SysTrayUpdateTooltip, MpAvThisAppVersionViewModel.Instance.UpToDateAppVersion.ToString());
                }
                return MpAvAccountViewModel.Instance.AccountStateInfo;
            }
        }

        public object SystemTrayIconResourceObj {
            get {
                if (Mp.Services == null ||
                    Mp.Services.StartupState == null ||
                    !Mp.Services.StartupState.IsReady) {
                    return "HourGlassImage";
                }
                if (MpAvThisAppVersionViewModel.Instance.IsOutOfDate) {
                    return "MonkeyUpdateImage";
                }
                return "AppImage";
            }
        }
        #endregion


        #endregion

        #region Constructors

        public MpAvSystemTrayViewModel() : base(null) {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.VersionInfoChanged:
                    OnPropertyChanged(nameof(SystemTrayTooltip));
                    OnPropertyChanged(nameof(SystemTrayIconResourceObj));
                    break;
                case MpMessageType.AccountInfoChanged:
                    OnPropertyChanged(nameof(SystemTrayTooltip));
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    OnPropertyChanged(nameof(SystemTrayIconResourceObj));
                    OnPropertyChanged(nameof(SystemTrayTooltip));
                    OnPropertyChanged(nameof(IsSystemTrayItemsEnabled));
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand TrayIconClickCommand => new MpCommand<object>(
            (args) => {
                // left click only
                if (MpAvThisAppVersionViewModel.Instance.IsOutOfDate) {
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpAvAccountTools.Instance.ThisProductUri);
                    return;
                }
                MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand.Execute(null);


                //                bool is_left_click = args == null;
                //#if WINDOWS
                //                is_left_click = false;
                //#endif

                //                if (is_left_click) {
                //                    MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand.Execute(null);
                //                    return;
                //                }

                //                var cm = MpAvMenuView.ShowMenu(
                //                    App.MainView,
                //                    TrayMenuItemViewModel2);


                //                void Cm_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
                //                    if (e.Source is not Control c || c.DataContext is not MpAvMenuItemViewModel) {
                //                        return;
                //                    }
                //                    CloseMenu();

                //                }
                //                void OnGlobalMouseReleased(object sender, bool is_left) {
                //                    CloseMenu();
                //                }
                //                void OnGlobalEscReleased(object sender, EventArgs e) {
                //                    CloseMenu();
                //                }

                //                void CloseMenu() {
                //                    cm.Close();
                //                    cm.LostFocus -= Cm_LostFocus;
                //                    MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased -= OnGlobalMouseReleased;
                //                    MpAvShortcutCollectionViewModel.Instance.OnGlobalEscapeReleased -= OnGlobalEscReleased;

                //                }
                //                cm.LostFocus += Cm_LostFocus;
                //                MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += OnGlobalMouseReleased;
                //                MpAvShortcutCollectionViewModel.Instance.OnGlobalEscapeReleased += OnGlobalEscReleased;
            });


        public ICommand ExitApplicationCommand => new MpCommand<object>(
            (args) => {
                if (args is string argStr &&
                    argStr == "welcome") {
                    // This should only be able to occur before actual app is loaded
                    // and before welcome moves to 'forget' state
                    // if user closes welcome window, pref file still exists
                    // so welcome still shows up but rest of app doesn't think its 
                    // initial startup. So delete storage dir here to clean everything up
                    //MpFileIo.DeleteDirectory(Mp.Services.PlatformInfo.StorageDir);
                }
                Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.UserTrayCmd, $"systray cmd - '{args.ToStringOrEmpty("no detail (likely quit cmd) ")}'");
            });


        #region Test Commands
        public ICommand NavigateToCefNetUriCommand => new MpAsyncCommand(
            async () => {
                var result = await Mp.Services.PlatformMessageBox.ShowTextBoxMessageBoxAsync(
                    title: "Browse To",
                    message: "Enter url:",
                    currentText: "chrome://about",
                    iconResourceObj: "WebImage");

                if (string.IsNullOrEmpty(result)) {
                    return;
                }

                Mp.Services.PlatformMessageBox.ShowWebViewWindow(
                    window_title_prefix: result,
                    address: result);
            });
        public ICommand CreateLocalStorageCopyCommand => new MpCommand(
            () => {
                string source_dir = Mp.Services.PlatformInfo.StorageDir.LocalStoragePathToPackagePath();
                string target_dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Path.GetFileName(source_dir));

                MpFileIo.DeleteDirectory(target_dir);
                MpFileIo.CopyDirectory(source_dir, target_dir, true);
            });
        public ICommand GenericTestCommand1 => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
                if (MpAvClipTrayViewModel.Instance.SelectedItem.GetContentView() is MpAvContentWebView cwv) {
                    cwv.RenderToFile(@"C:\Users\tkefauver\Desktop\ss.png");
                }

            });

        public ICommand GenericTestCommand2 => new MpAsyncCommand(
            async () => {
                await MpAvClipTrayViewModel.Instance.ReloadAllCommand.ExecuteAsync();
            });
        public ICommand GenericTestCommand3 => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
                MpAvClipTrayViewModel.Instance.ReloadAllContentCommand.Execute(null);
            });
        public ICommand GenericTestCommand4 => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
                //MpAvClipTrayViewModel.Instance.ClearAllSelection();
                Mp.Services.DataObjectTools
                .WriteToClipboardAsync(
                    new MpAvDataObject(MpPortableDataFormats.Text, MpAvDocusaurusHelpers.GetShortcutsMarkdown()), true).FireAndForgetSafeAsync();
            });

        public ICommand GenericTestCommand5 => new MpAsyncCommand(
            async () => {
                //await Mp.Services.DataObjectTools.WriteToClipboardAsync(new MpAvDataObject(MpPortableDataFormats.Image, MpBase64Images.AppIcon), true);
                //MpAvImageExtensions.Test();
                await Task.Delay(1);
                MpAvSubscriptionPurchaseViewModel.Instance.NavigateToBuyUpgradeCommand.Execute(null);
            });

        #endregion

        #endregion
    }
}
