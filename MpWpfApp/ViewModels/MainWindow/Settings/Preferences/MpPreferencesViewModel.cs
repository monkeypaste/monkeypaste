using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
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

        public MpPasteToAppPathViewModelCollection PasteToAppPathViewModelCollection {
            get {
                return MpPasteToAppPathViewModelCollection.Instance;
            }
        }
        #endregion

        #region Properties
        private ObservableCollection<string> _voiceNames = null;
        public ObservableCollection<string> VoiceNames {
            get {
                if(_voiceNames == null) {
                    _voiceNames = new ObservableCollection<string>();
                    var speechSynthesizer = new SpeechSynthesizer();
                    foreach (var voice in speechSynthesizer.GetInstalledVoices()) {
                        var name = voice.VoiceInfo.Name;
                        
                        _voiceNames.Add(voice.VoiceInfo.Name.Replace(@"Microsoft ",string.Empty).Replace(@" Desktop",string.Empty));
                    }
                }
                return _voiceNames;
            }
            set {
                if(_voiceNames != value) {
                    _voiceNames = value;
                    OnPropertyChanged(nameof(VoiceNames));
                }
            }
        }

        private string _selectedVoiceName = Properties.Settings.Default.SpeechSynthVoiceName;
        public string SelectedVoiceName {
            get {
                return _selectedVoiceName;
            }
            set {
                if(_selectedVoiceName != value) {
                    _selectedVoiceName = value;
                    Properties.Settings.Default.SpeechSynthVoiceName = _selectedVoiceName;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(SelectedVoiceName));
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
        private bool _ignoreWhiteSpaceCopyItems = Properties.Settings.Default.IgnoreWhiteSpaceCopyItems;
        public bool IgnoreWhiteSpaceCopyItems {
            get {
                return _ignoreWhiteSpaceCopyItems;
            }
            set {
                if (_ignoreWhiteSpaceCopyItems != value) {
                    _ignoreWhiteSpaceCopyItems = value;
                    Properties.Settings.Default.IgnoreWhiteSpaceCopyItems = _ignoreWhiteSpaceCopyItems;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(IgnoreWhiteSpaceCopyItems));
                }
            }
        }

        private bool _resetClipboard = Properties.Settings.Default.ResetClipboardAfterMonkeyPaste;
        public bool ResetClipboard {
            get {
                return _resetClipboard;
            }
            set {
                if (_resetClipboard != value) {
                    _resetClipboard = value;
                    Properties.Settings.Default.ResetClipboardAfterMonkeyPaste = _resetClipboard;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(ResetClipboard));
                }
            }
        }

        private bool _showItemPreview = Properties.Settings.Default.ShowItemPreview;
        public bool ShowItemPreview {
            get {
                return _showItemPreview;
            }
            set {
                if (_showItemPreview != value) {
                    _showItemPreview = value;
                    Properties.Settings.Default.ShowItemPreview = _showItemPreview;
                    Properties.Settings.Default.Save();
                    if(!MpMainWindowViewModel.IsApplicationLoading) {
                        foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                            ctvm.OnPropertyChanged(nameof(ctvm.ToolTipVisibility));
                            foreach(var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemToolTipVisibility));
                            }
                        }
                    }
                    OnPropertyChanged(nameof(ShowItemPreview));
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
            if(string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName) && VoiceNames != null && VoiceNames.Count > 0) {
                SelectedVoiceName = VoiceNames[0];
            }
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
            if(!MpHelpers.Instance.IsThisAppAdmin()) {
                //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
                return;
            }
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
