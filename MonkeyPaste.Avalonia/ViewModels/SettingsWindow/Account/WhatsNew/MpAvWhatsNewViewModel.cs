namespace MonkeyPaste.Avalonia {
    public class MpAvWhatsNewViewModel : MpAvViewModelBase {
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
        public string CurrentUrl =>
            MpAvDocusaurusHelpers.GetCustomUrl(WHATS_NEW_URL, true, MpAvPrefViewModel.Instance.IsThemeDark);

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
            //switch (e.PropertyName) {
            //}
        }
        #endregion

        #region Commands
        #endregion


    }
}
