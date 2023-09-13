using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSystemTrayViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel {

        #region Private Variables
        #endregion

        #region Statics

        private static MpAvSystemTrayViewModel _instance;
        public static MpAvSystemTrayViewModel Instance => _instance ?? (_instance = new MpAvSystemTrayViewModel());

        #endregion

        #region Interfaces

        #region MpIChildWindowViewModel Implementation

        public MpWindowType WindowType =>
            MpWindowType.PopOut;

        public bool IsWindowOpen { get; set; }

        #endregion

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
                    CommandPath = nameof(MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand),
                    CommandSrcObj = MpAvMainWindowViewModel.Instance,
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            HeaderSrcObj = MpAvMainWindowViewModel.Instance,
                            HeaderPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideLabel),
                            IconSrcBindingObj = MpAvMainWindowViewModel.Instance,
                            IconPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideIconResourceKey),
                            CommandSrcObj = MpAvMainWindowViewModel.Instance,
                            CommandPath = nameof(MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand),
                            //ShortcutArgs = new object[] { MpShortcutType.ToggleMainWindow },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleMainWindow),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)

                        },
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
                        new MpAvMenuItemViewModel() {
                            Header = "Mode",
                            IconResourceKey = "RobotClawImage",
                            SubItems = new List<MpAvMenuItemViewModel>() {
                                new MpAvMenuItemViewModel() {
                                    Header = "Append Inline",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendInsertMode),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendInsertModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAppendInsertMode },
                                    
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendInsertMode),
                                    InputGesturePropPath = nameof(MpAvShortcutViewModel.KeyString)
                                },
                                new MpAvMenuItemViewModel() {
                                    Header = "Append Line",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendLineMode),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAppendLineMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAppendLineMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },
                                new MpAvMenuItemViewModel() {IsSeparator = true},
                                new MpAvMenuItemViewModel() {
                                    Header = "Auto-Copy",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAutoCopyMode),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleAutoCopyMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleAutoCopyMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },
                                new MpAvMenuItemViewModel() {
                                    Header = "Right-Click Paste",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsRightClickPasteMode),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand),
                                    //ShortcutArgs = new object[] { MpShortcutType.ToggleRightClickPasteMode },
                                    InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ToggleRightClickPasteMode),
                                    InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                                },
                            }
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonSettingsTitle,
                            IconResourceKey = "CogImage",
                            CommandSrcObj = MpAvSettingsViewModel.Instance,
                            CommandPath = nameof(MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand),
                            //ShortcutArgs = new object[] { MpShortcutType.ShowSettings },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ShowSettings),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "About",
                            IconResourceKey = "InfoImage",
                            Command = MpAvAboutViewModel.Instance.ShowAboutWindowCommand
                        },
#if DEBUG && DESKTOP
                        new MpAvMenuItemViewModel() {
                            Header = "Show Converter DevTools",
                            Command = MpAvPlainHtmlConverter.Instance.ShowConverterDevTools,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Open Cef Uri",
                            Command = NavigateToCefNetUriCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            Header = "Generic Test",
                            Command = GenericTestCommand,
                        },
                        //new MpMenuItemViewModel() {
                        //    Header = "Show Notifier DevTools",
                        //    Command = MpAvClipTrayViewModel.Instance.ShowAppendDevToolsCommand
                        //},
#endif
                        new MpAvMenuItemViewModel() {IsSeparator = true},
                        new MpAvMenuItemViewModel() {
                            Header = "Quit",
                            IconResourceKey = "SignOutImage",
                            Command = ExitApplicationCommand,
                            CommandParameter = "systray menu click",
                            //ShortcutArgs = new object[] { MpShortcutType.ExitApplication },
                            InputGestureSrcObj = Mp.Services.ShortcutGestureLocator.LocateSourceByType(MpShortcutType.ExitApplication),
                            InputGesturePropPath = nameof(MpAvAssignShortcutViewModel.KeyString)
                        }
                    }
                };

                //tmivm.AllDescendants
                //    .ForEach(x => {
                //        x.IsEnabledSrcObj = this;
                //        x.IsEnabledPropPath = nameof(IsSystemTrayItemsEnabled);
                //    });
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
                    return $"Please wait {Mp.Services.ThisAppInfo.ThisAppProductName} is loading...";
                }
                return $"[{MpAvPrefViewModel.Instance.UserEmail}] {Mp.Services.AccountTools.AccountStateInfo.Replace(" - ", Environment.NewLine)}";
            }
        }

        public object SystemTrayIconResourceObj {
            get {
                if (Mp.Services == null ||
                    Mp.Services.StartupState == null ||
                    !Mp.Services.StartupState.IsReady) {
                    return "HourGlassImage";
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
        public ICommand ExitApplicationCommand => new MpCommand<object>(
            (args) => {
                if (args is string argStr &&
                    argStr == "welcome") {
                    // This should only be able to occur before actual app is loaded
                    // and before welcome moves to 'forget' state
                    // if user closes welcome window, pref file still exists
                    // so welcome still shows up but rest of app doesn't think its 
                    // initial startup. So delete storage dir here to clean everything up
                    MpFileIo.DeleteDirectory(Mp.Services.PlatformInfo.StorageDir);
                }
                Mp.Services.ShutdownHelper.ShutdownApp($"systray cmd - '{args.ToStringOrEmpty("no detail (likely quit cmd) ")}'");
            });

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

                var w = new MpAvWindow() {
                    Width = 500,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Title = result.ToWindowTitleText(),
                    Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", null, null, null) as WindowIcon,
                    Content = new WebView() {
                        InitialUrl = result,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                };

                void W_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
                    if (!e.IsRightPress(w)) {
                        return;
                    }
                    e.Handled = true;
                    if (w.Content is WebView wv) {
                        wv.GoBack();
                    }
                }

                w.AddHandler(Window.PointerPressedEvent, W_PointerPressed, RoutingStrategies.Tunnel);

                w.ShowChild();
            });
        //private INotificationManager? _notificationManager;
        //private INotificationManager NotificationManager => _notificationManager
        //    ??= new WindowNotificationManager(TopLevel.GetTopLevel(MpAvWindowManager.MainWindow)!);
        public ICommand GenericTestCommand => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
                //await MpTestDataBuilder.CreateTestDataAsync();
                //await MpAvWelcomeNotificationViewModel.ShowWelcomeNotification(true);
                //await MpAvPlainHtmlConverter.Instance.ConverterWebView.ReloadAsync();
                //NotificationManager.Show(new Notification("Warning", "There is one o more invalid path.", NotificationType.Information));


            });
        #endregion
    }
}
