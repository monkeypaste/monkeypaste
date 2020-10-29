using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpSystemTrayViewModel : MpViewModelBase {
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }
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
        public MpSystemTrayViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;
        }

        public void SystemTrayTaskbarIcon_Loaded(object sender, RoutedEventArgs e) {
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
            MpSettingsWindow sw = new MpSettingsWindow();
            var swvm = (MpSettingsWindowViewModel)sw.DataContext;
            swvm.Init(this);
            sw.ShowDialog();
            MainWindowViewModel.IsShowingDialog = false;
        }
        #endregion
    }
}
