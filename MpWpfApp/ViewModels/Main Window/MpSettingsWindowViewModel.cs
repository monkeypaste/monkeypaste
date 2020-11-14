using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MpWpfApp {
    // + Account
    //   - Email Address (tb)
    //   - Password (pb)
    //   - Remember (cb)
    //   - Register (link)
    //   - Log In/Out (b)
    //   - Devices (eg)
    //   - Sync (b)

    // + Preferences
    //   - load on login (cb)
    //   - Play sound notifiations (cb)
    //   - Show Copy Notifications (cb)
    //   - Show Append Change Notifications (cb)
    //   - Theme Selection (dd)

    // + Data
    //   - Excluded Apps (eg)
    //   - Import/Export Data (b)
    //   - Reset Data (b)
    //   - History Capacity (sb)
    //   - Clipboard data types (cb's)
    //   - Text recognition (msdb)
    //   - Always Paste as Plain Text (cb)
    //   - Db Statistics (labels)

    // + Shortcuts
    //   - Info box w/ tips & warnings
    //   - Hotkeys (eg)
    //   - Hotcorners (cc)

    // + Help
    //   - Keyboard shortcuts
    //   - Support
    //   - Send Feedback
    //   - About Monkey Paste

    public class MpSettingsWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region View Models
        public MpSystemTrayViewModel SystemTrayViewModel { get; set; }
        public MpShortcutCollectionViewModel ShortcutCollectionViewModel {
            get {
                return SystemTrayViewModel?.MainWindowViewModel.ShortcutCollectionViewModel;
            }
        }
        public MpAppCollectionViewModel AppCollectionViewModel { get; set; }
        #endregion

        #region Private Variables
        private Window _windowRef;

        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>();
        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToDelete = new ObservableCollection<MpShortcutViewModel>();
        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToRegister = new ObservableCollection<MpShortcutViewModel>();
        #endregion

        #region Properties

        private Visibility _settingsPanel1Visibility;
        public Visibility SettingsPanel1Visibility {
            get { 
                return _settingsPanel1Visibility; 
            }
            set { 
                if(_settingsPanel1Visibility != value) {
                    _settingsPanel1Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel1Visibility));
                }
            }
        }

        private Visibility _settingsPanel2Visibility;
        public Visibility SettingsPanel2Visibility {
            get {
                return _settingsPanel2Visibility;
            }
            set {
                if (_settingsPanel2Visibility != value) {
                    _settingsPanel2Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel2Visibility));
                }
            }
        }

        private Visibility _settingsPanel3Visibility;
        public Visibility SettingsPanel3Visibility {
            get {
                return _settingsPanel3Visibility;
            }
            set {
                if (_settingsPanel3Visibility != value) {
                    _settingsPanel3Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel3Visibility));
                }
            }
        }

        private Visibility _settingsPanel4Visibility;
        public Visibility SettingsPanel4Visibility {
            get {
                return _settingsPanel4Visibility;
            }
            set {
                if (_settingsPanel4Visibility != value) {
                    _settingsPanel4Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel4Visibility));
                }
            }
        }

        private Visibility _settingsPanel5Visibility;
        public Visibility SettingsPanel5Visibility {
            get {
                return _settingsPanel5Visibility;
            }
            set {
                if (_settingsPanel5Visibility != value) {
                    _settingsPanel5Visibility = value;
                    OnPropertyChanged(nameof(SettingsPanel5Visibility));
                }
            }
        }                

        private int _selectedShortcutIndex;
        public int SelectedShortcutIndex {
            get {
                return _selectedShortcutIndex;
            }
            set {
                if (_selectedShortcutIndex != value) {
                    _selectedShortcutIndex = value;
                    OnPropertyChanged(nameof(SelectedShortcutIndex));
                }
            }
        }

        private int _selectedExcludedAppIndex;
        public int SelectedExcludedAppIndex {
            get {
                return _selectedExcludedAppIndex;
            }
            set {
                if (_selectedExcludedAppIndex != value) {
                    _selectedExcludedAppIndex = value;
                    OnPropertyChanged(nameof(SelectedExcludedAppIndex));
                }
            }
        }

        public ObservableCollection<MpAppViewModel> ExcludedAppViewModels {
            get {
                var cvs = CollectionViewSource.GetDefaultView(AppCollectionViewModel);
                cvs.Filter += item => {
                    var avm = (MpAppViewModel)item;
                    return avm.IsAppRejected;
                };
                var eavms = new ObservableCollection<MpAppViewModel>(cvs.Cast<MpAppViewModel>().ToList());
                //this adds empty row
                eavms.Add(new MpAppViewModel(null));
                return eavms;
            }
        }
        #endregion

        #region Static Methods
        public static bool ShowSettingsWindow(MpSystemTrayViewModel stvm) {
            var sw = new MpSettingsWindow();
            sw.DataContext = new MpSettingsWindowViewModel(stvm);
            return sw.ShowDialog() ?? false;
        }
        #endregion

        #region Public Methods
        public MpSettingsWindowViewModel() : this(null) { }

        public void SettingsWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            _windowRef.Closed += (s, e2) => {
                IsOpen = false;
            };

            IsOpen = true;
            //var clonedList = ShortcutCollectionViewModel.Select(x => (MpShortcutViewModel)x.Clone()).ToList();
            //_shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>(clonedList);

            SettingsPanel1Visibility = Visibility.Visible;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Private Methods
        private MpSettingsWindowViewModel(MpSystemTrayViewModel stvm) {
            SystemTrayViewModel = stvm;
            AppCollectionViewModel = new MpAppCollectionViewModel();
            return;
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            string appPath = ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).ClipTrayViewModel.ClipboardMonitor.LastWindowWatcher.ThisAppPath;
            if (loadOnLogin) {
                rk.SetValue(appName, appPath);
            } else {
                rk.DeleteValue(appName, false);
            }
            Console.WriteLine("App " + appName + " with path " + appPath + " has load on login set to: " + loadOnLogin);
        }
        #endregion

        #region Commands
        private RelayCommand _cancelSettingsCommand;
        public ICommand CancelSettingsCommand {
            get {
                if (_cancelSettingsCommand == null) {
                    _cancelSettingsCommand = new RelayCommand(CancelSettings);
                }
                return _cancelSettingsCommand;
            }
        }
        private void CancelSettings() {
            //ShortcutViewModels = _shortcutViewModelsBackup;
            _windowRef.Close();
        }

        private RelayCommand _saveSettingsCommand;
        public ICommand SaveSettingsCommand {
            get {
                if (_saveSettingsCommand == null) {
                    _saveSettingsCommand = new RelayCommand(SaveSettings);
                }
                return _saveSettingsCommand;
            }
        }
        private void SaveSettings() {
            _windowRef.Close();
        }

        private RelayCommand _resetSettingsCommand;
        public ICommand ResetSettingsCommand {
            get {
                if (_resetSettingsCommand == null) {
                    _resetSettingsCommand = new RelayCommand(ResetSettings);
                }
                return _resetSettingsCommand;
            }
        }
        private void ResetSettings() {

        }

        private RelayCommand _reassignShortcutCommand;
        public ICommand ReassignShortcutCommand {
            get {
                if (_reassignShortcutCommand == null) {
                    _reassignShortcutCommand = new RelayCommand(ReassignShortcut);
                }
                return _reassignShortcutCommand;
            }
        }
        private void ReassignShortcut() {
            var scvm = ShortcutCollectionViewModel[SelectedShortcutIndex];
            SystemTrayViewModel.MainWindowViewModel.IsShowingDialog = true;
            
            string newKeyList = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(scvm.ShortcutDisplayName,scvm.KeyList, scvm.Command);
            if(newKeyList == null) {
                //assignment was canceled so do nothing
            } else if(newKeyList == string.Empty) {
                //shortcut was cleared
                scvm.ClearKeyList();
                scvm.Shortcut.WriteToDatabase();
                scvm.Unregister();
            } else {
                scvm.KeyList = newKeyList;
                scvm.Shortcut.WriteToDatabase();
                scvm.Register();
            }
            
            SystemTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
        }

        private RelayCommand _deleteShortcutCommand;
        public ICommand DeleteShortcutCommand {
            get {
                if (_deleteShortcutCommand == null) {
                    _deleteShortcutCommand = new RelayCommand(DeleteShortcut);
                }
                return _deleteShortcutCommand;
            }
        }
        private void DeleteShortcut() {
            Console.WriteLine("Deleting shortcut row: " + SelectedShortcutIndex);
            var scvm = ShortcutCollectionViewModel[SelectedShortcutIndex];
            ShortcutCollectionViewModel.Remove(scvm);
        }

        private RelayCommand _resetShortcutCommand;
        public ICommand ResetShortcutCommand {
            get {
                if (_resetShortcutCommand == null) {
                    _resetShortcutCommand = new RelayCommand(ResetShortcut);
                }
                return _resetShortcutCommand;
            }
        }
        private void ResetShortcut() {
            Console.WriteLine("Reset row: " + SelectedShortcutIndex);
            ShortcutCollectionViewModel[SelectedShortcutIndex].KeyList = ShortcutCollectionViewModel[SelectedShortcutIndex].Shortcut.DefaultKeyList;
            ShortcutCollectionViewModel[SelectedShortcutIndex].Register();
            ShortcutCollectionViewModel[SelectedShortcutIndex].Shortcut.WriteToDatabase();
        }

        private RelayCommand _deleteExcludedAppCommand;
        public ICommand DeleteExcludedAppCommand {
            get {
                if (_deleteExcludedAppCommand == null) {
                    _deleteExcludedAppCommand = new RelayCommand(DeleteExcludedApp);
                }
                return _deleteExcludedAppCommand;
            }
        }
        private void DeleteExcludedApp() {
            Console.WriteLine("Deleting excluded app row: " + SelectedExcludedAppIndex);
            var eavm = ExcludedAppViewModels[SelectedExcludedAppIndex];
            AppCollectionViewModel[AppCollectionViewModel.IndexOf(eavm)].IsAppRejected = false;
            AppCollectionViewModel[AppCollectionViewModel.IndexOf(eavm)].App.WriteToDatabase();
            OnPropertyChanged(nameof(ExcludedAppViewModels));
        }

        private RelayCommand _addExcludedAppCommand;
        public ICommand AddExcludedAppCommand {
            get {
                if (_addExcludedAppCommand == null) {
                    _addExcludedAppCommand = new RelayCommand(AddExcludedApp);
                }
                return _addExcludedAppCommand;
            }
        }
        private void AddExcludedApp() {
            Console.WriteLine("Add excluded app : ");
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Filter = "Applications|*.lnk;*.exe",
                Title = "Select an application to exclude"
            };
            bool? openResult = openFileDialog.ShowDialog();
            if (openResult != null && openResult.Value) {
                MpApp newExcludedApp = null;
                if(Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                    newExcludedApp = new MpApp(MpHelpers.GetShortcutTargetPath(openFileDialog.FileName), true, Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                } else {
                    newExcludedApp = new MpApp(openFileDialog.FileName, true, openFileDialog.FileName);
                }
                AppCollectionViewModel.Add(new MpAppViewModel(newExcludedApp));
            }
            OnPropertyChanged(nameof(ExcludedAppViewModels));
        }

        private RelayCommand _clickSettingsPanel1Command;
        public ICommand ClickSettingsPanel1Command {
            get {
                if(_clickSettingsPanel1Command == null) {
                    _clickSettingsPanel1Command = new RelayCommand(ClickSettingsPanel1);
                }
                return _clickSettingsPanel1Command;
            }
        }
        private void ClickSettingsPanel1() {
            SettingsPanel1Visibility = Visibility.Visible;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        private RelayCommand _clickSettingsPanel2Command;
        public ICommand ClickSettingsPanel2Command {
            get {
                if (_clickSettingsPanel2Command == null) {
                    _clickSettingsPanel2Command = new RelayCommand(ClickSettingsPanel2);
                }
                return _clickSettingsPanel2Command;
            }
        }
        private void ClickSettingsPanel2() {
            SettingsPanel1Visibility = Visibility.Collapsed;
            SettingsPanel2Visibility = Visibility.Visible;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        private RelayCommand _clickSettingsPanel3Command;
        public ICommand ClickSettingsPanel3Command {
            get {
                if (_clickSettingsPanel3Command == null) {
                    _clickSettingsPanel3Command = new RelayCommand(ClickSettingsPanel3);
                }
                return _clickSettingsPanel3Command;
            }
        }
        private void ClickSettingsPanel3() {
            SettingsPanel1Visibility = Visibility.Collapsed;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Visible;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        private RelayCommand _clickSettingsPanel4Command;
        public ICommand ClickSettingsPanel4Command {
            get {
                if (_clickSettingsPanel4Command == null) {
                    _clickSettingsPanel4Command = new RelayCommand(ClickSettingsPanel4);
                }
                return _clickSettingsPanel4Command;
            }
        }
        private void ClickSettingsPanel4() {
            SettingsPanel1Visibility = Visibility.Collapsed;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Visible;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        private RelayCommand _clickSettingsPanel5Command;
        public ICommand ClickSettingsPanel5Command {
            get {
                if (_clickSettingsPanel5Command == null) {
                    _clickSettingsPanel5Command = new RelayCommand(ClickSettingsPanel5);
                }
                return _clickSettingsPanel5Command;
            }
        }
        private void ClickSettingsPanel5() {
            SettingsPanel1Visibility = Visibility.Collapsed;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Visible;
        }
        #endregion
    }
}