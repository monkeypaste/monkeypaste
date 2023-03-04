using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;

namespace MonkeyPaste.Avalonia {
    public class MpAvPreferencesMenuViewModel : MpViewModelBase<MpAvSettingsWindowViewModel> {
        #region Private Variables

        #endregion

        #region View Models
        #endregion

        #region Properties


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

        private string _selectedLanguage = MpPrefViewModel.Instance.UserLanguage;
        public string SelectedLanguage {
            get {
                return _selectedLanguage;
            }
            set {
                if (_selectedLanguage != value) {
                    _selectedLanguage = value;
                    MpPrefViewModel.Instance.UserLanguage = _selectedLanguage;

                    OnPropertyChanged(nameof(SelectedLanguage));
                }
            }
        }
        private bool _ignoreWhiteSpaceCopyItems = MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems;
        public bool IgnoreWhiteSpaceCopyItems {
            get {
                return _ignoreWhiteSpaceCopyItems;
            }
            set {
                if (_ignoreWhiteSpaceCopyItems != value) {
                    _ignoreWhiteSpaceCopyItems = value;
                    MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems = _ignoreWhiteSpaceCopyItems;
                    OnPropertyChanged(nameof(IgnoreWhiteSpaceCopyItems));
                }
            }
        }

        private bool _resetClipboard = MpPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste;
        public bool ResetClipboard {
            get {
                return _resetClipboard;
            }
            set {
                if (_resetClipboard != value) {
                    _resetClipboard = value;
                    MpPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste = _resetClipboard;

                    OnPropertyChanged(nameof(ResetClipboard));
                }
            }
        }

        private bool _showItemPreview = MpPrefViewModel.Instance.ShowItemPreview;
        public bool ShowItemPreview {
            get {
                return _showItemPreview;
            }
            set {
                if (_showItemPreview != value) {
                    _showItemPreview = value;
                    MpPrefViewModel.Instance.ShowItemPreview = _showItemPreview;

                    if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                        foreach (var ctvm in MpAvClipTrayViewModel.Instance.AllItems) {
                            ctvm.OnPropertyChanged(nameof(ctvm.IsTooltipVisible));
                        }
                    }
                    OnPropertyChanged(nameof(ShowItemPreview));
                }
            }
        }

        private bool _useSpellCheck = MpPrefViewModel.Instance.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    MpPrefViewModel.Instance.UseSpellCheck = _useSpellCheck;

                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private bool _ignoreNewDuplicates = MpPrefViewModel.Instance.IgnoreNewDuplicates;
        public bool IgnoreNewDuplicates {
            get {
                return _ignoreNewDuplicates;
            }
            set {
                if (_ignoreNewDuplicates != value) {
                    _ignoreNewDuplicates = value;
                    MpPrefViewModel.Instance.IgnoreNewDuplicates = _ignoreNewDuplicates;

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
        public MpAvPreferencesMenuViewModel() : base(null) { }

        public MpAvPreferencesMenuViewModel(MpAvSettingsWindowViewModel parent) : base(parent) {
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

            IsLoadOnLoginChecked = MpPrefViewModel.Instance.LoadOnLogin;
            UseSpellCheck = MpPrefViewModel.Instance.UseSpellCheck;
        }
        #endregion

        #region Private Methods
        private void SetLanguage(string newLanguage) {
            MpCurrentCultureViewModel.Instance.SetLanguageCommand.Execute(newLanguage);
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            if (!MpPlatform.Services.ProcessWatcher.IsAdmin(MpPlatform.Services.ProcessWatcher.ThisAppHandle)) {
                //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
                return;
            }
#if WINDOWS
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            string appPath = MpPlatform.Services.PlatformInfo.ExecutingPath;
            if (loadOnLogin) {
                rk.SetValue(appName, appPath);
            } else {
                rk.DeleteValue(appName, false);
            }
#endif
            MpPrefViewModel.Instance.LoadOnLogin = loadOnLogin;

            MpConsole.WriteLine("App " + appName + " with path " + appPath + " has load on login set to: " + loadOnLogin);
        }


        #endregion

        #region Commands


        #endregion
    }
}
