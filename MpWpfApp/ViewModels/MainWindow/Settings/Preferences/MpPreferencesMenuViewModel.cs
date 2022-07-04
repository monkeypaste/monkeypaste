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
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpPreferencesMenuViewModel : MpViewModelBase<MpSettingsWindowViewModel> {
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

        private string _selectedVoiceName = MpJsonPreferenceIO.Instance.SpeechSynthVoiceName;
        public string SelectedVoiceName {
            get {
                return _selectedVoiceName;
            }
            set {
                if(_selectedVoiceName != value) {
                    _selectedVoiceName = value;
                    MpJsonPreferenceIO.Instance.SpeechSynthVoiceName = _selectedVoiceName;
                    
                    OnPropertyChanged(nameof(SelectedVoiceName));
                }
            }
        }

        private int _maxRtfCharCount = MpJsonPreferenceIO.Instance.MaxRtfCharCount;
        public int MaxRtfCharCount {
            get {
                return _maxRtfCharCount;
            }
            set {
                if (_maxRtfCharCount != value && value > 0) {
                    _maxRtfCharCount = value;
                    MpJsonPreferenceIO.Instance.MaxRtfCharCount = _maxRtfCharCount;
                    
                    OnPropertyChanged(nameof(MaxRtfCharCount));
                }
            }
        }

        //private ObservableCollection<string> _languages = null;
        //public ObservableCollection<string> Languages {
        //    get {
        //        if (_languages == null) {
        //            _languages = new ObservableCollection<string>();
        //            foreach (var lang in MpLanguageTranslator.LanguageList) {
        //                _languages.Add(lang);
        //            }
        //        }
        //        return _languages;
        //    }
        //}

        private string _selectedLanguage = MpJsonPreferenceIO.Instance.UserLanguage;
        public string SelectedLanguage {
            get {
                return _selectedLanguage;
            }
            set {
                if (_selectedLanguage != value) {
                    _selectedLanguage = value;
                    MpJsonPreferenceIO.Instance.UserLanguage = _selectedLanguage;
                    
                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            }
        }
        private bool _ignoreWhiteSpaceCopyItems = MpJsonPreferenceIO.Instance.IgnoreWhiteSpaceCopyItems;
        public bool IgnoreWhiteSpaceCopyItems {
            get {
                return _ignoreWhiteSpaceCopyItems;
            }
            set {
                if (_ignoreWhiteSpaceCopyItems != value) {
                    _ignoreWhiteSpaceCopyItems = value;
                    MpJsonPreferenceIO.Instance.IgnoreWhiteSpaceCopyItems = _ignoreWhiteSpaceCopyItems;
                    OnPropertyChanged(nameof(IgnoreWhiteSpaceCopyItems));
                }
            }
        }

        private bool _resetClipboard = MpJsonPreferenceIO.Instance.ResetClipboardAfterMonkeyPaste;
        public bool ResetClipboard {
            get {
                return _resetClipboard;
            }
            set {
                if (_resetClipboard != value) {
                    _resetClipboard = value;
                    MpJsonPreferenceIO.Instance.ResetClipboardAfterMonkeyPaste = _resetClipboard;
                    
                    OnPropertyChanged(nameof(ResetClipboard));
                }
            }
        }

        private bool _showItemPreview = MpJsonPreferenceIO.Instance.ShowItemPreview;
        public bool ShowItemPreview {
            get {
                return _showItemPreview;
            }
            set {
                if (_showItemPreview != value) {
                    _showItemPreview = value;
                    MpJsonPreferenceIO.Instance.ShowItemPreview = _showItemPreview;
                    
                    if(!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                        foreach(var ctvm in MpClipTrayViewModel.Instance.Items) {
                            ctvm.OnPropertyChanged(nameof(ctvm.ToolTipVisibility));
                        }
                    }
                    OnPropertyChanged(nameof(ShowItemPreview));
                }
            }
        }

        private bool _useSpellCheck = MpJsonPreferenceIO.Instance.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    MpJsonPreferenceIO.Instance.UseSpellCheck = _useSpellCheck;
                    
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private bool _ignoreNewDuplicates = MpJsonPreferenceIO.Instance.IgnoreNewDuplicates;
        public bool IgnoreNewDuplicates {
            get {
                return _ignoreNewDuplicates;
            }
            set {
                if (_ignoreNewDuplicates != value) {
                    _ignoreNewDuplicates = value;
                    MpJsonPreferenceIO.Instance.IgnoreNewDuplicates = _ignoreNewDuplicates;
                    
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
        public MpPreferencesMenuViewModel() : base(null) { }

        public MpPreferencesMenuViewModel(MpSettingsWindowViewModel parent) : base(parent) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsLoadOnLoginChecked):
                        SetLoadOnLogin(IsLoadOnLoginChecked);
                        break;
                    case nameof(SelectedLanguage):
                        Task.Run(async () => { 
                            await SetLanguage(SelectedLanguage); 
                        });
                        break;
                }
            };

            IsLoadOnLoginChecked = MpJsonPreferenceIO.Instance.LoadOnLogin;
            UseSpellCheck = MpJsonPreferenceIO.Instance.UseSpellCheck;
            if(string.IsNullOrEmpty(MpJsonPreferenceIO.Instance.SpeechSynthVoiceName) && VoiceNames != null && VoiceNames.Count > 0) {
                SelectedVoiceName = VoiceNames[0];
            }
        }
        #endregion

        #region Private Methods
        private async Task SetLanguage(string newLanguage) {
            foreach (SettingsProperty dsp in Properties.DefaultUiStrings.Default.Properties) {
                foreach (SettingsProperty usp in Properties.UserUiStrings.Default.Properties) {
                    if (dsp.Name == usp.Name) {
                        //usp.DefaultValue = await MpLanguageTranslator.TranslateAsync((string)dsp.DefaultValue, newLanguage,"");
                        MpConsole.WriteLine("Default: " + (string)dsp.DefaultValue + "New: " + (string)usp.DefaultValue);                    }
                }
            }
            
            Properties.UserUiStrings.Default.Save();
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            if(!MpHelpers.IsThisAppAdmin()) {
                //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
                return;
            }
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            string appPath = MpHelpers.GetApplicationDirectory();// MpClipTrayViewModel.Instance.ClipboardManager.LastWindowWatcher.ThisAppPath;
            if (loadOnLogin) {
                rk.SetValue(appName, appPath);
            } else {
                rk.DeleteValue(appName, false);
            }
            MpJsonPreferenceIO.Instance.LoadOnLogin = loadOnLogin;
            
            MpConsole.WriteLine("App " + appName + " with path " + appPath + " has load on login set to: " + loadOnLogin);
        }

        
        #endregion

        #region Commands
        

        #endregion
    }
}
