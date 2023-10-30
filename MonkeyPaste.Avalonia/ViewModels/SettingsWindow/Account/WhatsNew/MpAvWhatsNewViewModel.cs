using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvWhatsNewViewModel : MpAvViewModelBase, MpAvIWebPageViewModel {
        #region Private Variables
        #endregion

        #region Constants 
        static string WHATS_NEW_URL = MpServerConstants.BLOG_BASE_URL;

        #endregion

        #region Statics
        private static MpAvWhatsNewViewModel _instance;
        public static MpAvWhatsNewViewModel Instance => _instance ?? (_instance = new MpAvWhatsNewViewModel());

        #endregion

        #region Interfaces

        #region MpAvIWebPageViewModel Implementatiosn
        private string _themedWhatsNewUrl;
        private string _currentUrl;
        public string CurrentUrl {
            get {
                if (_themedWhatsNewUrl == null) {
                    _themedWhatsNewUrl = WHATS_NEW_URL + MpAvDocusaurusHelpers.GetThemeUrlAttrb(MpAvPrefViewModel.Instance.IsThemeDark);
                    _currentUrl = _themedWhatsNewUrl;
                }
                return _currentUrl;
            }
        }
        public ICommand ReloadCommand => new MpCommand(
            () => {
                // ensure reload
                _currentUrl = MpUrlHelpers.BLANK_URL;
                OnPropertyChanged(nameof(CurrentUrl));
                _currentUrl = _themedWhatsNewUrl;
                OnPropertyChanged(nameof(CurrentUrl));
            });

        public object ReloadCommandParameter =>
            null;

        #endregion
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MpAvWhatsNewViewModel() {
            PropertyChanged += MpAvWhatsNewViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvWhatsNewViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsBusy):
                    if (IsBusy) {
                        if (MpAvWindowManager.LocateWindow(MpAvSettingsViewModel.Instance) is not MpAvWindow w ||
                                w.GetVisualDescendant<MpAvAccountView>() is not MpAvAccountView av ||
                                av.GetVisualDescendant<MpAvWebView>() is not MpAvWebView wv) {
                            return;
                        }
                        MpAvDocusaurusHelpers.LoadMainOnlyAsync(wv).FireAndForgetSafeAsync();
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion


    }
}
