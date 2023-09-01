namespace MonkeyPaste.Avalonia {
    public class MpAvHelpViewModel : MpAvViewModelBase {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvHelpViewModel _instance;
        public static MpAvHelpViewModel Instance =>
            _instance ?? (_instance = new MpAvHelpViewModel());
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public MpAvHelpViewModel() : base(null) { }
        #endregion

        #region Commands

        #endregion
    }
}
