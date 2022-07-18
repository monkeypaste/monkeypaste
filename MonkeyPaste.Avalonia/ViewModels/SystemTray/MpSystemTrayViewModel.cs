
using System;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpSystemTrayViewModel : MpViewModelBase, MpIAsyncSingletonViewModel<MpSystemTrayViewModel> {


        #region Private Variables
        //private TaskbarIcon _taskbarIcon = null;
        #endregion

        #region Properties

        #region View Models
        private MpAvSettingsWindowViewModel _settingsWindowViewModel = null;
        public MpAvSettingsWindowViewModel SettingsWindowViewModel {
            get {
                return _settingsWindowViewModel;
            }
            set {
                if (_settingsWindowViewModel != value) {
                    _settingsWindowViewModel = value;
                    OnPropertyChanged(nameof(SettingsWindowViewModel));
                }
            }
        }
        #endregion

        private string _systemTrayIconToolTipText = MpPrefViewModel.Instance.ApplicationName;
        public string SystemTrayIconToolTipText {
            get {
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
                return Math.Round(MpFileIo.FileListSize(new string[] {MpPlatformWrapper.Services.DbInfo.DbPath }),2).ToString() + " megabytes";
            }
        }

        public string PauseOrPlayIconSource {
            get {
                if(MpAvMainWindowViewModel.Instance == null || MpAvClipTrayViewModel.Instance == null) {
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

        private static MpSystemTrayViewModel _instance;
        public static MpSystemTrayViewModel Instance => _instance ?? (_instance = new MpSystemTrayViewModel());

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        public MpSystemTrayViewModel() : base(null) {
            //MpPlatformWrapper.Services.PlatformResource.GetResource("SystemTrayViewModel") = this;
        }

        #endregion

        #region Commands
        public ICommand ExitApplicationCommand => new MpCommand(
            () => {
                MpAvMainWindowViewModel.Instance.Dispose();
                MpAvAppViewModel.Instance.ExitCommand.Execute(null);
            });

        public ICommand ShowLogDialogCommand => new MpCommand(
            () => {
                //var result = MessageBox.Show(MonkeyPaste.MpSyncManager.StatusLog, "Server Log", MessageBoxButton.OK, MessageBoxImage.Error);
            });

        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            async (args) => {
                
                //MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                int tabIdx = 0;
                if (args is int) {
                    tabIdx = (int)args;
                } else if (args is MpAvClipTileViewModel) {
                    args = (args as MpAvClipTileViewModel).AppViewModel.App;
                    tabIdx = 1;
                } 

                //await MpAvSettingsWindow.ShowDialog(tabIdx, args);
            });
        #endregion
    }
}
