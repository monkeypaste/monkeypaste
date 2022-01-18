using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MonkeyPaste;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSystemTrayViewModel : MpSingletonViewModel2<MpSystemTrayViewModel> {

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
        //private TaskbarIcon _taskbarIcon = null;
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
                if(MpMainWindowViewModel.Instance == null) {
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

        public string TotalItemCountLabel { get; set; }

        public string DbSizeInMbs {
            get {
                return Math.Round(MpHelpers.Instance.FileListSize(new string[] { Properties.Settings.Default.DbPath }),2).ToString() + " megabytes";
            }
        }

        public string PauseOrPlayIconSource {
            get {
                if(MpMainWindowViewModel.Instance == null || MpAppModeViewModel.Instance == null) {
                    return string.Empty;
                }
                if(MpAppModeViewModel.Instance.IsAppPaused) {
                    return Application.Current.Resources["PlayIcon"] as string;
                }
                return Application.Current.Resources["PauseIcon"] as string;
            }
        }

        public string PauseOrPlayHeader {
            get {
                if (MpMainWindowViewModel.Instance == null || MpAppModeViewModel.Instance == null) {
                    return string.Empty;
                }
                if (MpAppModeViewModel.Instance.IsAppPaused) {
                    return @"Resume";
                }
                return @"Pause";
            }
        }
        #endregion

        #region Constructors


        public MpSystemTrayViewModel() {
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
                //var result = MessageBox.Show(MonkeyPaste.MpSyncManager.Instance.StatusLog, "Server Log", MessageBoxButton.OK, MessageBoxImage.Error);
            });

        public ICommand ShowSettingsWindowCommand => new RelayCommand<object>(
            async (args) => {
                
                //MpMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                int tabIdx = 0;
                if (args is int) {
                    tabIdx = (int)args;
                } else if (args is MpClipTileViewModel) {
                    args = (args as MpClipTileViewModel).PrimaryItem.CopyItem.Source.App;
                    tabIdx = 1;
                } else if (args is MpContentItemViewModel) {
                    args = (args as MpContentItemViewModel).CopyItem.Source.App;
                    tabIdx = 1;
                }

                await MpSettingsWindow.ShowDialog(tabIdx, args);
            });
        #endregion
    }
}
