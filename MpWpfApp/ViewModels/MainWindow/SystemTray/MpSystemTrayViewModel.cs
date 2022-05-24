using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MonkeyPaste;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSystemTrayViewModel : MpViewModelBase, MpISingletonViewModel<MpSystemTrayViewModel> {


        #region Private Variables
        //private TaskbarIcon _taskbarIcon = null;
        #endregion

        #region Properties

        #region View Models
        private MpSettingsWindowViewModel _settingsWindowViewModel = null;
        public MpSettingsWindowViewModel SettingsWindowViewModel {
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

        private string _systemTrayIconToolTipText = MpPreferences.ApplicationName;
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
                if(MpMainWindowViewModel.Instance == null) {
                    return string.Empty;
                }
                return @"Monkey Paste [" + (MpClipTrayViewModel.Instance.IsAppPaused ? "PAUSED" : "ACTIVE") + "]";
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
                return Math.Round(MpHelpers.FileListSize(new string[] { MpPreferences.DbPath }),2).ToString() + " megabytes";
            }
        }

        public string PauseOrPlayIconSource {
            get {
                if(MpMainWindowViewModel.Instance == null || MpClipTrayViewModel.Instance == null) {
                    return string.Empty;
                }
                if(MpClipTrayViewModel.Instance.IsAppPaused) {
                    return Application.Current.Resources["PlayIcon"] as string;
                }
                return Application.Current.Resources["PauseIcon"] as string;
            }
        }

        public string PauseOrPlayHeader {
            get {
                if (MpMainWindowViewModel.Instance == null || MpClipTrayViewModel.Instance == null) {
                    return string.Empty;
                }
                if (MpClipTrayViewModel.Instance.IsAppPaused) {
                    return @"Resume";
                }
                return @"Pause";
            }
        }
        #endregion

        #region Constructors

        private static MpSystemTrayViewModel _instance;
        public static MpSystemTrayViewModel Instance => _instance ?? (_instance = new MpSystemTrayViewModel());

        public async Task Init() {
            await Task.Delay(1);
        }

        public MpSystemTrayViewModel() : base(null) {
            Application.Current.Resources["SystemTrayViewModel"] = this;
        }

        #endregion

        #region Commands
        public ICommand ExitApplicationCommand => new RelayCommand(
            () => {
                MpMainWindowViewModel.Instance.Dispose();
                Application.Current.Shutdown();
            });

        public ICommand ShowLogDialogCommand => new RelayCommand(
            () => {
                //var result = MessageBox.Show(MonkeyPaste.MpSyncManager.StatusLog, "Server Log", MessageBoxButton.OK, MessageBoxImage.Error);
            });

        public ICommand ShowSettingsWindowCommand => new RelayCommand<object>(
            async (args) => {
                
                //MpMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                int tabIdx = 0;
                if (args is int) {
                    tabIdx = (int)args;
                } else if (args is MpClipTileViewModel) {
                    args = (args as MpClipTileViewModel).AppViewModel.App;
                    tabIdx = 1;
                } 

                await MpSettingsWindow.ShowDialog(tabIdx, args);
            });
        #endregion
    }
}
