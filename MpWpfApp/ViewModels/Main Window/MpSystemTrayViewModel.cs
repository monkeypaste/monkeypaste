using System.Windows;

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
    }
}
