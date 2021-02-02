using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
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
        #endregion

        #region Private Variables
        private Window _windowRef;

        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>();
        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToDelete = new ObservableCollection<MpShortcutViewModel>();
        private ObservableCollection<MpShortcutViewModel> _shortcutViewModelsToRegister = new ObservableCollection<MpShortcutViewModel>();
        #endregion

        #region Properties
        private int _maxRtfCharCount = Properties.Settings.Default.MaxRtfCharCount;
        public int MaxRtfCharCount {
            get {
                return _maxRtfCharCount;
            }
            set {
                if(_maxRtfCharCount != value && value > 0) {
                    _maxRtfCharCount = value;
                    OnPropertyChanged(nameof(MaxRtfCharCount));
                }
            }
        }

        private ObservableCollection<string> _languages = null;
        public ObservableCollection<string> Languages {
            get {
                if(_languages == null) {
                    _languages = new ObservableCollection<string>();
                    foreach(var lang in MpLanguageTranslator.Instance.LanguageList) {
                        _languages.Add(lang);
                    }
                }
                return _languages;
            }
        }

        private string _selectedLanguage = Properties.Settings.Default.UserLanguage;
        public string SelectedLanguage {
            get {
                return _selectedLanguage;
            }
            set {
                if(_selectedLanguage != value) {
                    _selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            }
        }

        private bool _useSpellCheck = Properties.Settings.Default.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if(_useSpellCheck != value) {
                    _useSpellCheck = value;
                    Properties.Settings.Default.UseSpellCheck = _useSpellCheck;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

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

        private bool _isLoadOnLoginChecked = false;
        public bool IsLoadOnLoginChecked {
            get {
                return _isLoadOnLoginChecked;
            }
            set {
                if (_isLoadOnLoginChecked != value) {
                    _isLoadOnLoginChecked = value;
                    OnPropertyChanged(nameof(IsLoadOnLoginChecked));
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

                var cvs = CollectionViewSource.GetDefaultView(MpAppCollectionViewModel.Instance);
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

        public MpShortcutCollectionViewModel ShortcutCollectionViewModel {
            get {
                return MpShortcutCollectionViewModel.Instance;
            }
        }
        
        public MpSoundPlayerGroupCollectionViewModel SoundPlayerGroupCollectionViewModel {
            get {
                return MpSoundPlayerGroupCollectionViewModel.Instance;
            }
        }
        #endregion

        #region Static Methods
        
        #endregion

        #region Public Methods
        public MpSettingsWindowViewModel() : base() { }

        public MpSettingsWindowViewModel(MpSystemTrayViewModel stvm) : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsLoadOnLoginChecked):
                        SetLoadOnLogin(IsLoadOnLoginChecked);
                        break;
                    case nameof(SelectedLanguage):
                        SetLanguage(SelectedLanguage);
                        break;
                }
            };
            SystemTrayViewModel = stvm;
            IsLoadOnLoginChecked = Properties.Settings.Default.LoadOnLogin;
            UseSpellCheck = Properties.Settings.Default.UseSpellCheck;
            MpAppCollectionViewModel.Instance.Init();
            return;
        }

        public void SettingsWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            _windowRef.Closed += (s, e2) => {
                IsOpen = false;
            };
            IsOpen = true;
            //var clonedList = MpShortcutCollectionViewModel.Instance.Select(x => (MpShortcutViewModel)x.Clone()).ToList();
            //_shortcutViewModelsBackup = new ObservableCollection<MpShortcutViewModel>(clonedList);

            SettingsPanel1Visibility = Visibility.Visible;
            SettingsPanel2Visibility = Visibility.Collapsed;
            SettingsPanel3Visibility = Visibility.Collapsed;
            SettingsPanel4Visibility = Visibility.Collapsed;
            SettingsPanel5Visibility = Visibility.Collapsed;
        }

        public bool ShowSettingsWindow() {
            var sw = new MpSettingsWindow();
            sw.DataContext = this;
            return sw.ShowDialog() ?? false;
        }
        #endregion

        #region Private Methods       
        private async Task SetLanguage(string newLanguage) {
            foreach (SettingsProperty dsp in Properties.DefaultUiStrings.Default.Properties) {
                foreach (SettingsProperty usp in Properties.UserUiStrings.Default.Properties) {
                    if(dsp.Name == usp.Name) {
                        usp.DefaultValue = await MpLanguageTranslator.Instance.Translate((string)dsp.DefaultValue, newLanguage, false);
                        Console.WriteLine("Default: " + (string)dsp.DefaultValue + "New: " + (string)usp.DefaultValue);
                    }
                }
            }
            Properties.Settings.Default.Save();
            Properties.UserUiStrings.Default.Save();
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            string appPath = MpHelpers.Instance.GetApplicationDirectory();// MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.ThisAppPath;
            if (loadOnLogin) {
                rk.SetValue(appName, appPath);
            } else {
                rk.DeleteValue(appName, false);
            }
            Properties.Settings.Default.LoadOnLogin = loadOnLogin;
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
            var scvm = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex];
            MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                scvm,
                scvm.ShortcutDisplayName,
                scvm.KeyList,
                scvm.Command
            );
            //SystemTrayViewModel.MainWindowViewModel.IsShowingDialog = true;
            
            //string newKeyList = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow(scvm.ShortcutDisplayName,scvm.KeyList, scvm.Command);
            //if(newKeyList == null) {
            //    //assignment was canceled so do nothing
            //} else if(newKeyList == string.Empty) {
            //    //shortcut was cleared
            //    scvm.ClearKeyList();
            //    scvm.Shortcut.WriteToDatabase();
            //    scvm.Unregister();
            //} else {
            //    scvm.KeyList = newKeyList;
            //    scvm.Shortcut.WriteToDatabase();
            //    scvm.Register();
            //}
            
            //SystemTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
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
            var scvm = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex];
            MpShortcutCollectionViewModel.Instance.Remove(scvm);
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
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].KeyList = MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Shortcut.DefaultKeyList;
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Register();
            MpShortcutCollectionViewModel.Instance[SelectedShortcutIndex].Shortcut.WriteToDatabase();
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
            MpAppCollectionViewModel.Instance[MpAppCollectionViewModel.Instance.IndexOf(eavm)].IsAppRejected = false;
            MpAppCollectionViewModel.Instance[MpAppCollectionViewModel.Instance.IndexOf(eavm)].App.WriteToDatabase();
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
                string appPath = openFileDialog.FileName;
                if(Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                    appPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                }
                var neavm = MpAppCollectionViewModel.Instance.GetAppViewModelByProcessPath(appPath);
                if (neavm == null) {
                    //if unknown app just add it with rejection flag
                    neavm = new MpAppViewModel(new MpApp(appPath));
                    MpAppCollectionViewModel.Instance.Add(neavm);
                } else if (neavm.IsAppRejected) {
                    //if app is already rejected set it to selected in grid
                    MessageBox.Show(neavm.AppName + " is already being rejected");
                    neavm.IsSelected = true;
                } else {
                    //otherwise update rejection and prompt about current clips
                    MpAppCollectionViewModel.Instance.UpdateRejection(neavm, true);
                }
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