using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpSystemTrayViewModel : MpViewModelBase {
        #region View Models
        private MpSettingsWindowViewModel _settingsWindowViewModel = null;
        public MpSettingsWindowViewModel SettingsWindowViewModel {
            get {
                return _settingsWindowViewModel;
            }
            set {
                if(_settingsWindowViewModel != value) {
                    _settingsWindowViewModel = value;
                    OnPropertyChanged(nameof(SettingsWindowViewModel));
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
                    OnPropertyChanged(nameof(SystemTrayIconToolTipText));
                }
            }
        }

        public string AppStatus {
            get {
                if(MainWindowViewModel == null) {
                    return string.Empty;
                }
                return @"Monkey Paste [" + (MainWindowViewModel.AppModeViewModel.IsAppPaused ? "PAUSED" : "ACTIVE") + "]";
            }
        }

        public string AccountStatus {
            get {
                return "<email address> <online?>";
            }
        }

        public string TotalItemCount {
            get {
                return MpCopyItem.GetTotalItemCount().ToString() + " total entries";
            }
        }

        public string DbSizeInMbs {
            get {
                return Math.Round(MpHelpers.Instance.FileListSize(new string[] { Properties.Settings.Default.DbPath }),2).ToString() + " megabytes";
            }
        }
        #endregion

        #region Public Methods
        public MpSystemTrayViewModel() : base() {
            SettingsWindowViewModel = new MpSettingsWindowViewModel();
        }

        public void SystemTrayTaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
            _taskbarIcon = (TaskbarIcon)sender;
            _taskbarIcon.TrayLeftMouseUp += (s, e1) => {
                MainWindowViewModel.ShowWindowCommand.Execute(null);
            };
            _taskbarIcon.MouseEnter += (s, e3) => {
                OnPropertyChanged(nameof(AppStatus));
                OnPropertyChanged(nameof(AccountStatus));
                OnPropertyChanged(nameof(TotalItemCount));
                OnPropertyChanged(nameof(DbSizeInMbs));
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
                // TODO occurs when app icon macro is clicked so need to 
                // automate paste to app datagrid to auto add item and select
                tabIdx = 1;
            }
            SettingsWindowViewModel.ShowSettingsWindow(tabIdx);

            MainWindowViewModel.IsShowingDialog = false;
        }
        #endregion
    }
}
