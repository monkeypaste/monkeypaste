using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpPreferencesViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region View Models
        public MpSoundPlayerGroupCollectionViewModel SoundPlayerGroupCollectionViewModel {
            get {
                return MpSoundPlayerGroupCollectionViewModel.Instance;
            }
        }
        #endregion

        #region Properties
        private bool _isTerminalAdmin = Properties.Settings.Default.IsTerminalAdministrator;
        public bool IsTerminalAdmin {
            get {
                return _isTerminalAdmin;
            }
            set {
                if(_isTerminalAdmin != value) {
                    _isTerminalAdmin = value;
                    Properties.Settings.Default.IsTerminalAdministrator = _isTerminalAdmin;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(IsTerminalAdmin));
                }
            }
        }
        private string _pathToTerminal = Properties.Settings.Default.PathToTerminal;
        public string PathToTerrminal {
            get {
                return _pathToTerminal;
            }
            set {
                if(_pathToTerminal != value) {
                    _pathToTerminal = value;
                    Properties.Settings.Default.PathToTerminal = _pathToTerminal;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(PathToTerrminal));
                }
            }
        }

        private int _maxRtfCharCount = Properties.Settings.Default.MaxRtfCharCount;
        public int MaxRtfCharCount {
            get {
                return _maxRtfCharCount;
            }
            set {
                if (_maxRtfCharCount != value && value > 0) {
                    _maxRtfCharCount = value;
                    Properties.Settings.Default.MaxRtfCharCount = _maxRtfCharCount;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(MaxRtfCharCount));
                }
            }
        }

        private ObservableCollection<string> _languages = null;
        public ObservableCollection<string> Languages {
            get {
                if (_languages == null) {
                    _languages = new ObservableCollection<string>();
                    foreach (var lang in MpLanguageTranslator.Instance.LanguageList) {
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
                if (_selectedLanguage != value) {
                    _selectedLanguage = value;
                    Properties.Settings.Default.UserLanguage = _selectedLanguage;
                    Properties.Settings.Default.Save();
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
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    Properties.Settings.Default.UseSpellCheck = _useSpellCheck;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private bool _ignoreNewDuplicates = Properties.Settings.Default.IgnoreNewDuplicates;
        public bool IgnoreNewDuplicates {
            get {
                return _ignoreNewDuplicates;
            }
            set {
                if (_ignoreNewDuplicates != value) {
                    _ignoreNewDuplicates = value;
                    Properties.Settings.Default.IgnoreNewDuplicates = _ignoreNewDuplicates;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(IgnoreNewDuplicates));
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
        #endregion

        #region Public Methods
        public MpPreferencesViewModel() : base() {
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

            IsLoadOnLoginChecked = Properties.Settings.Default.LoadOnLogin;
            UseSpellCheck = Properties.Settings.Default.UseSpellCheck;
        }
        #endregion

        #region Private Methods
        private async Task SetLanguage(string newLanguage) {
            foreach (SettingsProperty dsp in Properties.DefaultUiStrings.Default.Properties) {
                foreach (SettingsProperty usp in Properties.UserUiStrings.Default.Properties) {
                    if (dsp.Name == usp.Name) {
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
            Properties.Settings.Default.Save();
            Console.WriteLine("App " + appName + " with path " + appPath + " has load on login set to: " + loadOnLogin);
        }
        #endregion

        #region Commands
        private RelayCommand _selectPathToTerminalCommand;
        public ICommand SelectPathToTerminalCommand {
            get {
                if(_selectPathToTerminalCommand == null) {
                    _selectPathToTerminalCommand = new RelayCommand(SelectPathToTerminal);
                }
                return _selectPathToTerminalCommand;
            }
        }
        private void SelectPathToTerminal() {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                Filter = "Applications|*.lnk;*.exe",
                Title = "Select terminal path",
                InitialDirectory = Path.GetPathRoot(PathToTerrminal)
            };
            bool? openResult = openFileDialog.ShowDialog();
            if (openResult != null && openResult.Value) {
                string terminalPath = openFileDialog.FileName;
                if (Path.GetExtension(openFileDialog.FileName).Contains("lnk")) {
                    terminalPath = MpHelpers.Instance.GetShortcutTargetPath(openFileDialog.FileName);
                }
                PathToTerrminal = terminalPath;
            }
        }
        #endregion
    }
}
