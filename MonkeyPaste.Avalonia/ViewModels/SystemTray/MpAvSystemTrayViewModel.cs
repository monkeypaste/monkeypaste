using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Application = Avalonia.Application;

namespace MonkeyPaste.Avalonia {
    public class MpAvSystemTrayViewModel :
        MpViewModelBase,
        MpIAsyncSingletonViewModel<MpAvSystemTrayViewModel> {

        #region Private Variables
        #endregion

        #region Statics

        private static MpAvSystemTrayViewModel _instance;
        public static MpAvSystemTrayViewModel Instance => _instance ?? (_instance = new MpAvSystemTrayViewModel());

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
                                    IsCheckedPropPath = nameof(MpAvClipTrayViewModel.Instance.IsAppendMode),
                                    ToggleType = "Radio",
                                    CommandSrcObj = MpAvClipTrayViewModel.Instance,
                                    CommandPath = nameof(MpAvClipTrayViewModel.Instance.ToggleAppendModeCommand),
                                    ShortcutArgs = new object[] { MpShortcutType.ToggleAppendMode },
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
                            CommandSrcObj = MpAvSettingsWindowViewModel.Instance,
                            CommandPath = nameof(MpAvSettingsWindowViewModel.Instance.ShowSettingsWindowCommand),
                            ShortcutArgs = new object[] { MpShortcutType.ShowSettings },
                        },
#if DEBUG && DESKTOP
                        new MpMenuItemViewModel() {
                            Header = "Show Converter DevTools",
                            Command = MpAvPlainHtmlConverter.Instance.ShowConverterDevTools,
                        },
                        new MpMenuItemViewModel() {
                            Header = "Show Notifier DevTools",
                            Command = MpAvAppendNotificationWindow.Instance.ShowNotifierDevToolsCommand
                        },
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

                    //MpAvMainView.Instance.Close();
                    //MpAvMainWindowViewModel.Instance.Dispose();
                    MpAvCefNetApplication.ShutdownCefNet();
                    await Task.Delay(3000);

                    MpConsole.WriteLine("CefNet Shutdown Complete");

                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                        lifetime.Shutdown();
                    }
                });
            });
        #endregion
    }
}
