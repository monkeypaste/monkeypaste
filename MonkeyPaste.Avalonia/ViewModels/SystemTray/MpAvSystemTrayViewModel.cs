using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Application = Avalonia.Application;

namespace MonkeyPaste.Avalonia {
    public class MpAvSystemTrayViewModel :
        MpViewModelBase,
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

        public MpMenuItemViewModel TrayMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    TooltipSrcObj = MpAvMainWindowViewModel.Instance,
                    TooltipPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideLabel),
                    IconResourceKey = "AppIcon",
                    CommandPath = nameof(MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand),
                    CommandSrcObj = MpAvMainWindowViewModel.Instance,
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            HeaderSrcObj = MpAvMainWindowViewModel.Instance,
                            HeaderPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideLabel),
                            IconSrcBindingObj = MpAvMainWindowViewModel.Instance,
                            IconPropPath = nameof(MpAvMainWindowViewModel.Instance.ShowOrHideIconResourceKey),
                            CommandSrcObj = MpAvMainWindowViewModel.Instance,
                            CommandPath = nameof(MpAvMainWindowViewModel.Instance.ToggleShowMainWindowCommand),
                        },
                        new MpMenuItemViewModel() {
                            HeaderSrcObj = MpAvClipTrayViewModel.Instance,
                            HeaderPropPath = nameof(MpAvClipTrayViewModel.Instance.PlayOrPauseLabel),
                            IconSrcBindingObj = MpAvClipTrayViewModel.Instance,
                            IconPropPath = nameof(MpAvClipTrayViewModel.Instance.PlayOrPauseIconResoureKey),
                            CommandSrcObj = MpAvClipTrayViewModel.Instance,
                            CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleIsAppPausedCommand),
                            ShortcutArgs = new object[] { MpShortcutType.ToggleListenToClipboard },
                        },
                        new MpMenuItemViewModel() {
                            Header = "Mode",
                            IconResourceKey = "RobotClawImage",
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "Append",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendInsertMode),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendInsertModeCommand),
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAppendInsertMode },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Append Line",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendLineMode),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand),
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAppendLineMode },
                                },
                                new MpMenuItemViewModel() {IsSeparator = true},
                                new MpMenuItemViewModel() {
                                    Header = "Auto-Copy",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAutoCopyMode),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAutoCopyModeCommand),
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAutoCopyMode },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Right-Click Paste",
                                    IsCheckedSrcObj = MpAvClipTrayViewModel.Instance,
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsRightClickPasteMode),
                                    ToggleType = "CheckBox",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleRightClickPasteCommand),
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleRightClickPasteMode },
                                },
                            }
                        },
                        new MpMenuItemViewModel() {
                            Header = "Settings",
                            IconResourceKey = "CogImage",
                            CommandSrcObj = MpAvSettingsViewModel.Instance,
                            CommandPath = nameof(MpAvSettingsViewModel.Instance.ShowSettingsWindowCommand),
                            ShortcutArgs = new object[] { MpShortcutType.ShowSettings },
                        },
#if DEBUG && DESKTOP
                        new MpMenuItemViewModel() {
                            Header = "Show Converter DevTools",
                            Command = MpAvPlainHtmlConverter.Instance.ShowConverterDevTools,
                        },
                        new MpMenuItemViewModel() {
                            Header = "Open Cef Uri",
                            Command = NavigateToCefNetUriCommand,
                        },
                        //new MpMenuItemViewModel() {
                        //    Header = "Show Notifier DevTools",
                        //    Command = MpAvClipTrayViewModel.Instance.ShowAppendDevToolsCommand
                        //},
#endif
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = "Close",
                            IconResourceKey = "SignOutImage",
                            Command = ExitApplicationCommand,
                            ShortcutArgs = new object[] { MpShortcutType.ExitApplication },
                        }
                    }
                };
            }
        }
        #endregion

        #region State

        #endregion

        #endregion

        #region Constructors

        public MpAvSystemTrayViewModel() : base(null) { }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        #endregion

        #region Commands
        public ICommand ExitApplicationCommand => new MpCommand(
            () => {
                Dispatcher.UIThread.Post(async () => {
                    MpConsole.WriteLine("ExitApplicationCommand begin");
#if DESKTOP
                    MpAvCefNetApplication.ShutdownCefNet();
#endif
                    await Task.Delay(3000);

                    MpConsole.WriteLine("CefNet Shutdown Complete");

                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                        lifetime.Shutdown();
                    }
                });
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

        #endregion
    }
}
