using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

#if CEFNET_WV
#endif

namespace MonkeyPaste.Avalonia {

    public class MpAvSystemTrayViewModel :
        MpAvViewModelBase {

        #region Private Variables
        #endregion

        #region Statics

        static string VERSION_CHECK_URL = $"{MpServerConstants.VERSION_BASE_URL}/version.php";
        static string CHANGE_LOG_BASE_URL = $"{MpServerConstants.DOCS_BASE_URL}/versions";

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
                                    
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendInsertMode),
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
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendLineMode),
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
                            Command = CheckForUpdateCommand,
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
        #endregion

        #region State

        public bool IsUpdateAvailable =>
            UpToDateAppVersion != null &&
            ThisAppVersion < UpToDateAppVersion;

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
                if (IsUpdateAvailable) {
                    return string.Format(UiStrings.SysTrayUpdateTooltip, UpToDateAppVersion.ToString());
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
                if (IsUpdateAvailable) {
                    return "MonkeyUpdateImage";
                }
                return "AppImage";
            }
        }
        #endregion

        #region Model
        public string ChangeLogUrl =>
            MpAvDocusaurusHelpers.GetCustomUrl(
                url: $"{CHANGE_LOG_BASE_URL}/{Mp.Services.ThisAppInfo.ThisAppProductVersion}",
                hideNav: true,
                hideSidebars: true,
                isDark: MpAvPrefViewModel.Instance.IsThemeDark);

        public Version LastLoadedVersion {
            get => MpAvPrefViewModel.Instance.LastLoadedVersion.ToVersion();
            set {
                if (LastLoadedVersion.CompareTo(value) != 0) {
                    MpAvPrefViewModel.Instance.LastLoadedVersion = value.ToString();
                    OnPropertyChanged(nameof(LastLoadedVersion));
                }
            }
        }
        public Version ThisAppVersion =>
            Mp.Services.ThisAppInfo.ThisAppProductVersion.ToVersion();

        public Version UpToDateAppVersion { get; private set; }

        public Version LastNotfiedVersion { get; set; }
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
            StartUpdateCheckTimer();
        }

        #endregion

        #region Private Methods

        private void StartUpdateCheckTimer() {
            var update_check_timer = new DispatcherTimer() {
                Interval = TimeSpan.FromMinutes(5)
            };
            void CheckForUpdate_tick(object sender, EventArgs e) {
                CheckForUpdateCommand.Execute("timer");
            }
            update_check_timer.Tick += CheckForUpdate_tick;
            update_check_timer.Start();
            // initial check
            CheckForUpdate_tick(update_check_timer, EventArgs.Empty);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.AccountInfoChanged:
                    OnPropertyChanged(nameof(SystemTrayTooltip));
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    OnPropertyChanged(nameof(SystemTrayIconResourceObj));
                    OnPropertyChanged(nameof(SystemTrayTooltip));
                    OnPropertyChanged(nameof(IsSystemTrayItemsEnabled));
                    if (LastLoadedVersion < ThisAppVersion) {
                        LastLoadedVersion = ThisAppVersion;
                        ShowChangeLogWindowCommand.Execute(null);
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand TrayIconClickCommand => new MpCommand(
            () => {
                // left click only
                if (IsUpdateAvailable) {
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpAvAccountTools.Instance.ThisProductUri);
                    return;
                }
                MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand.Execute(null);
            });

        public ICommand ShowChangeLogWindowCommand => new MpCommand(
            () => {
                Mp.Services.PlatformMessageBox.ShowWebViewWindow(
                    window_title_prefix: string.Format(UiStrings.ChangeLogWindowTitle, ThisAppVersion.ToString()),
                    address: ChangeLogUrl,
                    iconResourceObj: "MegaPhoneImage");
            });

        public MpIAsyncCommand<object> CheckForUpdateCommand => new MpAsyncCommand<object>(
            async (args) => {
                string source = args.ToStringOrEmpty();
                bool from_user = source == "Click";

                CancellationTokenSource cts = null;
                if (from_user) {
                    cts = new CancellationTokenSource();
                    Mp.Services.PlatformMessageBox.ShowBusyMessageBoxAsync(
                        title: UiStrings.CommonBusyLabel,
                        iconResourceObj: "HourGlassImage",
                        cancel_token_arg: cts.Token).FireAndForgetSafeAsync();
                }
                var req_args = new Dictionary<string, string>() {
                    {"device_type",Mp.Services.PlatformInfo.OsType.ToString() }
                };
                // send device type and receive most recent version by device
                var resp = await MpHttpRequester.SubmitPostDataToUrlAsync(VERSION_CHECK_URL, req_args);
                bool success = MpHttpRequester.ProcessServerResponse(resp, out var resp_args);
                if (cts != null) {
                    cts.Cancel();
                }
                if (!success) {
                    // couldn't connect
                    if (from_user) {
                        Mp.Services.NotificationBuilder.ShowMessageAsync(
                            msgType: MpNotificationType.BadHttpRequest,
                            title: UiStrings.CommonConnectionFailedCaption,
                            body: UiStrings.CommonConnectionFailedText,
                            iconSourceObj: "NoEntryImage").FireAndForgetSafeAsync();
                    }
                    return;
                }
                UpToDateAppVersion = resp_args["device_version"].ToVersion();
                OnPropertyChanged(nameof(SystemTrayIconResourceObj));

                if (!IsUpdateAvailable) {
                    // this is most recent version
                    if (from_user) {
                        Mp.Services.NotificationBuilder.ShowMessageAsync(
                            msgType: MpNotificationType.NoUpdateAvailable,
                            title: UiStrings.NtfUpToDateTitle,
                            body: string.Format(UiStrings.NtfUpToDateText, ThisAppVersion.ToString()),
                            iconSourceObj: new object[] { MpSystemColors.forestgreen, "CheckRoundImage" }).FireAndForgetSafeAsync();
                    }
                    return;
                }
                // update available
                bool show_ntf = source == "Click";
                if (!show_ntf) {
                    if (LastNotfiedVersion == null ||
                        UpToDateAppVersion.CompareTo(LastNotfiedVersion) != 0) {
                        show_ntf = true;
                    }
                }
                if (!show_ntf) {
                    // non-user check, already notified
                    return;
                }
                LastNotfiedVersion = UpToDateAppVersion;
                var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                            notificationType: MpNotificationType.UpdateAvailable,
                            title: UiStrings.NtfUpdateAvailableTitle,
                            maxShowTimeMs: from_user ? -1 : 5_000,
                            body: string.Format(UiStrings.NtfUpdateAvailableText, UpToDateAppVersion.ToString()),
                            iconSourceObj: "MegaPhoneImage");
                if (result == MpNotificationDialogResultType.Ok) {
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpAvAccountTools.Instance.ThisProductUri);
                }
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
                string source_dir = Mp.Services.PlatformInfo.StorageDir;
                string target_dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Path.GetFileName(source_dir));

                MpFileIo.DeleteDirectory(target_dir);
                MpFileIo.CopyDirectory(source_dir, target_dir, true);
            });
        public ICommand GenericTestCommand1 => new MpAsyncCommand(
            async () => {
                await MpAvClipTrayViewModel.Instance.DisposeAndReloadAllCommand.ExecuteAsync();
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

                //await MpAvCommonTools.Services.DeviceClipboard.ClearAsync();
                var sil = await new[] {
                    @"/Users/tkefauver/Desktop/icon_test.png",
                    @"/Users/tkefauver/Desktop/Info.plist" }.ToAvFilesObjectAsync();


                //var avdo = new DataObject();
                //avdo.Set("FileNames", sil.Select(x => x.TryGetLocalPath()).ToArray());
                //await MpAvCommonTools.Services.DeviceClipboard.SetDataObjectAsync(avdo);


                var avdo = new MpAvDataObject(MpPortableDataFormats.Files, sil);
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true);

            });

        public ICommand GenericTestCommand5 => new MpAsyncCommand(
            async () => {
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(new MpAvDataObject(MpPortableDataFormats.Image, MpBase64Images.AppIcon), true);
            });

        #endregion

        #endregion
    }
}
