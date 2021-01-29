using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
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
        #endregion

        #region Public Methods
        public MpSystemTrayViewModel() : base() {
            SettingsWindowViewModel = new MpSettingsWindowViewModel(this);
        }

        public void SystemTrayTaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
            _taskbarIcon = (TaskbarIcon)sender;
            _taskbarIcon.TrayLeftMouseUp += (s, e1) => {
                MainWindowViewModel.ShowWindowCommand.Execute(null);
            };
            //ShowStandardBalloon("Test title", "Test balloon text", BalloonIcon.Info);
        }

        public void ShowStandardBalloon(string title, string text, BalloonIcon icon) {
            MpBalloonControl balloon = new MpBalloonControl();
            balloon.BalloonTitle = title;
            balloon.BalloonText = text;
            _taskbarIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, Properties.Settings.Default.NotificationBalloonVisibilityTimeMs);
        }
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
            Application.Current.Shutdown();
        }

        private RelayCommand _showSettingsWindowCommand;
        public ICommand ShowSettingsWindowCommand {
            get {
                if (_showSettingsWindowCommand == null) {
                    _showSettingsWindowCommand = new RelayCommand(ShowSettingsWindow);
                }
                return _showSettingsWindowCommand;
            }
        }
        private void ShowSettingsWindow() {
            MainWindowViewModel.IsShowingDialog = true;

            MainWindowViewModel.HideWindowCommand.Execute(null);       
            
            SettingsWindowViewModel.ShowSettingsWindow();

            MainWindowViewModel.IsShowingDialog = false;
        }
        #endregion
    }
}
