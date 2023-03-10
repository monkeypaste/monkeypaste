using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste.Avalonia {
    public class MpAvPreferencesMenuViewModel :
        MpViewModelBase {
        #region Private Variables

        #endregion
        #region Statics

        private static MpAvPreferencesMenuViewModel _instance;
        public static MpAvPreferencesMenuViewModel Instance => _instance ?? (_instance = new MpAvPreferencesMenuViewModel());


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


        #region View Models
        public ObservableCollection<MpAvPreferenceFrameViewModel> Items { get; set; } = new ObservableCollection<MpAvPreferenceFrameViewModel>();

        public MpAvPreferenceFrameViewModel LookAndFeelFrame { get; set; }
        #endregion
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

        #region Constructors
        public MpAvPreferencesMenuViewModel() : base(null) {
            PropertyChanged += MpAvPreferencesMenuViewModel_PropertyChanged;
            InitializeAsync().FireAndForgetSafeAsync(this);
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsLoadOnLoginChecked = MpPrefViewModel.Instance.LoadOnLogin;
            UseSpellCheck = MpPrefViewModel.Instance.UseSpellCheck;

            // look & feel
            LookAndFeelFrame = new MpAvPreferenceFrameViewModel(this) {
                LabelText = "Look & Feel",
                PluginFormat = new MpPluginFormat() {
                    headless = new MpHeadlessPluginFormat() {
                        parameters = new List<MpParameterFormat>() {
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.MainWindowOpacity),
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Decimal,
                                label = "Background Opacity",
                                minimum = 0,
                                maximum = 1,
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpPrefViewModel.Instance.MainWindowOpacity.ToString()
                                    }
                                }
                            },
                            new MpParameterFormat() {
                                paramId = nameof(MpPrefViewModel.Instance.NotificationSoundVolume),
                                controlType = MpParameterControlType.Slider,
                                unitType = MpParameterValueUnitType.Decimal,
                                label = "Notification Volume",
                                minimum = 0,
                                maximum = 1,
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        value = MpPrefViewModel.Instance.NotificationSoundVolume.ToString()
                                    }
                                }
                            }
                        }
                    }
                }
            };

            LookAndFeelFrame.Items = await Task.WhenAll(
                LookAndFeelFrame.PluginFormat.headless.parameters.Select(x =>
                    MpAvPluginParameterBuilder.CreateParameterViewModelAsync(
                        new MpParameterValue() {
                            ParamId = x.paramId,
                            Value = x.values.FirstOrDefault(x => x.isDefault).value
                        }, LookAndFeelFrame)));

            Items.Add(LookAndFeelFrame);
        }
        #endregion

        #region Private Methods

        private void MpAvPreferencesMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsLoadOnLoginChecked):
                    SetLoadOnLogin(IsLoadOnLoginChecked);
                    break;
                case nameof(SelectedLanguage):
                    SetLanguage(SelectedLanguage);
                    break;
            }
        }
        private void SetLanguage(string newLanguage) {
            MpCurrentCultureViewModel.Instance.SetLanguageCommand.Execute(newLanguage);
        }

        private void SetLoadOnLogin(bool loadOnLogin) {
            if (!Mp.Services.ProcessWatcher.IsAdmin(Mp.Services.ProcessWatcher.ThisAppHandle)) {
                //MonkeyPaste.MpConsole.WriteLine("Process not running as admin, cannot alter load on login");
                return;
            }
#if WINDOWS
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string appName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            string appPath = Mp.Services.PlatformInfo.ExecutingPath;
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
