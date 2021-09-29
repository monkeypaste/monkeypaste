using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSystemTrayViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpSystemTrayViewModel> _Lazy = new Lazy<MpSystemTrayViewModel>(() => new MpSystemTrayViewModel());
        public static MpSystemTrayViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion

        #region View Models
        private MpSettingsWindowViewModel _settingsWindowViewModel = null;
        public MpSettingsWindowViewModel SettingsWindowViewModel {
            get {
                return _settingsWindowViewModel;
            }
            set {
                if(_settingsWindowViewModel != value) {
                    _settingsWindowViewModel = value;
                    OnPropertyChanged_old(nameof(SettingsWindowViewModel));
                }
            }
        }
        #endregion

        #region Private Variables
        private TaskbarIcon _taskbarIcon = null;
        #endregion

        #region Properties
        private string _systemTrayIconToolTipText = Properties.Settings.Default.ApplicationName;
        public string SystemTrayIconToolTipText {
            get {
                return _systemTrayIconToolTipText;
            }
            set {
                if (_systemTrayIconToolTipText != value) {
                    _systemTrayIconToolTipText = value;
                    OnPropertyChanged_old(nameof(SystemTrayIconToolTipText));
                }
            }
        }

        public string AppStatus {
            get {
                if(MainWindowViewModel == null) {
                    return string.Empty;
                }
                return @"Monkey Paste [" + (MpAppModeViewModel.Instance.IsAppPaused ? "PAUSED" : "ACTIVE") + "]";
            }
        }

        public string AccountStatus {
            get {
                return "<email address> <online?>";
            }
        }

        public string TotalItemCount {
            get {
                return MpDb.Instance.GetItems<MpCopyItem>().Count.ToString() + " total entries";
            }
        }

        public string DbSizeInMbs {
            get {
                return Math.Round(MpHelpers.Instance.FileListSize(new string[] { Properties.Settings.Default.DbPath }),2).ToString() + " megabytes";
            }
        }

        public string PauseOrPlayIconSource {
            get {
                if(MainWindowViewModel == null || MpAppModeViewModel.Instance == null) {
                    return string.Empty;
                }
                if(MpAppModeViewModel.Instance.IsAppPaused) {
                    return @"/Images/play.png";
                }
                return @"/Images/pause.png";
            }
        }

        public string PauseOrPlayHeader {
            get {
                if (MainWindowViewModel == null || MpAppModeViewModel.Instance == null) {
                    return string.Empty;
                }
                if (MpAppModeViewModel.Instance.IsAppPaused) {
                    return @"Resume";
                }
                return @"Pause";
            }
        }
        #endregion

        #region Public Methods
        private MpSystemTrayViewModel() : base(null) {
        }

        public void SystemTrayTaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
            _taskbarIcon = (TaskbarIcon)sender;
            _taskbarIcon.TrayLeftMouseUp += (s, e1) => {
                MainWindowViewModel.ShowWindowCommand.Execute(null);
            };
            _taskbarIcon.MouseEnter += (s, e3) => {
                OnPropertyChanged_old(nameof(AppStatus));
                OnPropertyChanged_old(nameof(AccountStatus));
                OnPropertyChanged_old(nameof(TotalItemCount));
                OnPropertyChanged_old(nameof(DbSizeInMbs));
            };
            //ShowStandardBalloon("Monkey Paste", "Successfully loaded", BalloonIcon.Info);
            //ShowStandardBalloon("Test title", "Test balloon text", BalloonIcon.Info);
        }

        //public void ShowStandardBalloon(string title, string text, BalloonIcon icon) {
        //    MpBalloonControl balloon = new MpBalloonControl();
        //    balloon.BalloonTitle = title;
        //    balloon.BalloonText = text;
        //    _taskbarIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, Properties.Settings.Default.NotificationBalloonVisibilityTimeMs);
        //}
        #endregion

        #region Commands
        private RelayCommand _exitApplicationCommand;
        public ICommand ExitApplicationCommand {
            get {
                if (_exitApplicationCommand == null) {
                    _exitApplicationCommand = new RelayCommand(ExitApplication);
                }
                return _exitApplicationCommand;
            }
        }
        private void ExitApplication() {
            MainWindowViewModel.Dispose();
            Application.Current.Shutdown();
        }

        private RelayCommand _showLogDialogCommand;
        public ICommand ShowLogDialogCommand {
            get {
                if(_showLogDialogCommand == null) {
                    _showLogDialogCommand = new RelayCommand(ShowLogDialog);
                }
                return _showLogDialogCommand;
            }
        }
        private void ShowLogDialog() {
           // var result = MessageBox.Show(MonkeyPaste.MpSyncManager.Instance.StatusLog, "Server Log", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private RelayCommand<object> _showSettingsWindowCommand;
        public ICommand ShowSettingsWindowCommand {
            get {
                if (_showSettingsWindowCommand == null) {
                    _showSettingsWindowCommand = new RelayCommand<object>(ShowSettingsWindow);
                }
                return _showSettingsWindowCommand;
            }
        }
        private void ShowSettingsWindow(object args) {
            MainWindowViewModel.IsShowingDialog = true;
            MainWindowViewModel.HideWindowCommand.Execute(null);
            int tabIdx = 0;
            if(args is int) {
                tabIdx = (int)args;
            } else if (args is MpClipTileViewModel) {
                args = (args as MpClipTileViewModel).PrimaryItem.CopyItem.Source.App;
                tabIdx = 1;
            } else if (args is MpContentItemViewModel) {
                args = (args as MpContentItemViewModel).CopyItem.Source.App;
                tabIdx = 1;
            }

            SettingsWindowViewModel = new MpSettingsWindowViewModel();
            SettingsWindowViewModel.ShowSettingsWindow(tabIdx, args);

            MainWindowViewModel.IsShowingDialog = false;
        }
        #endregion
    }
}
