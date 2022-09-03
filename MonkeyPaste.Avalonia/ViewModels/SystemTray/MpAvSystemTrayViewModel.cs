
using System;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using MonkeyPaste.Common;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public class MpAvSystemTrayViewModel : MpViewModelBase, MpIAsyncSingletonViewModel<MpAvSystemTrayViewModel> {

        #region Private Variables
        //private TaskbarIcon _taskbarIcon = null;
        #endregion

        #region Statics

        private static MpAvSystemTrayViewModel _instance;
        public static MpAvSystemTrayViewModel Instance => _instance ?? (_instance = new MpAvSystemTrayViewModel());

        #endregion

        #region Properties

        #region View Models
        public MpAvSettingsWindowViewModel SettingsWindowViewModel { get; set; }
        #endregion

        private string _systemTrayIconToolTipText;
        public string SystemTrayIconToolTipText {
            get {
                if(_systemTrayIconToolTipText == null) {
                    if(MpAvBootstrapperViewModel.IsLoaded) {
                        _systemTrayIconToolTipText = MpPrefViewModel.Instance.ApplicationName;
                    }
                }
                return _systemTrayIconToolTipText;
            }
            set {
                if (_systemTrayIconToolTipText != value) {
                    _systemTrayIconToolTipText = value;
                    OnPropertyChanged(nameof(SystemTrayIconToolTipText));
                }
            }
        }

        public string AppStatus {
            get {
                if(!MpAvBootstrapperViewModel.IsLoaded) {
                    return string.Empty;
                }
                if(MpAvMainWindowViewModel.Instance == null) {
                    return string.Empty;
                }
                return @"Monkey Paste [" + (MpAvClipTrayViewModel.Instance.IsAppPaused ? "PAUSED" : "ACTIVE") + "]";
            }
        }

        public string AccountStatus {
            get {
                return "<email address> <online?>";
            }
        }

        public string TotalItemCountLabel { get; set; }

        public string DbSizeInMbs {
            get {
                if(!MpAvBootstrapperViewModel.IsLoaded) {
                    return string.Empty;
                }
                return Math.Round(MpFileIo.FileListSize(new string[] {MpPlatformWrapper.Services.DbInfo.DbPath }),2).ToString() + " megabytes";
            }
        }

        public string PauseOrPlayIconSource {
            get {
                if (!MpAvBootstrapperViewModel.IsLoaded) {
                    return string.Empty;
                }
                if (MpAvMainWindowViewModel.Instance == null || MpAvClipTrayViewModel.Instance == null) {
                    return string.Empty;
                }
                if(MpAvClipTrayViewModel.Instance.IsAppPaused) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("PlayIcon") as string;
                }
                return MpPlatformWrapper.Services.PlatformResource.GetResource("PauseIcon") as string;
            }
        }

        public string PauseOrPlayHeader {
            get {
                if (!MpAvBootstrapperViewModel.IsLoaded) {
                    return string.Empty;
                }
                if (MpAvMainWindowViewModel.Instance == null || MpAvClipTrayViewModel.Instance == null) {
                    return string.Empty;
                }
                if (MpAvClipTrayViewModel.Instance.IsAppPaused) {
                    return @"Resume";
                }
                return @"Pause";
            }
        }
        #endregion

        #region Constructors

        public MpAvSystemTrayViewModel() : base(null) {
            //MpPlatformWrapper.Services.PlatformResource.GetResource("SystemTrayViewModel") = this;
        }

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

                    //MpAvMainWindow.Instance.Close();
                    //MpAvMainWindowViewModel.Instance.Dispose();
                    MpAvCefNetApplication.ShutdownCefNet();
                    await Task.Delay(3000);

                    MpConsole.WriteLine("CefNet Shutdown Complete");

                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                        lifetime.Shutdown();
                    }
                });
            },()=> MpBootstrapperViewModelBase.IsLoaded);

        public ICommand ShowLogDialogCommand => new MpCommand(
            () => {
                //var result = MessageBox.Show(MonkeyPaste.MpSyncManager.StatusLog, "Server Log", MessageBoxButton.OK, MessageBoxImage.Error);
            });

        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                
                //MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                int tabIdx = 0;
                if (args is int) {
                    tabIdx = (int)args;
                } else if (args is MpAvClipTileViewModel) {
                    args = (args as MpAvClipTileViewModel).AppViewModel.App;
                    tabIdx = 1;
                } 

                //await MpAvSettingsWindow.ShowDialog(tabIdx, args);
            },(args)=>MpAvBootstrapperViewModel.IsLoaded);
        #endregion
    }
}
