using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        private int _maxRtfCharCount = Properties.Settings.Default.MaxRtfCharCount;
        public int MaxRtfCharCount {
            get {
                return _maxRtfCharCount;
            }
            set {
                if (_maxRtfCharCount != value && value > 0) {
                    _maxRtfCharCount = value;
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

        #endregion
    }
}
