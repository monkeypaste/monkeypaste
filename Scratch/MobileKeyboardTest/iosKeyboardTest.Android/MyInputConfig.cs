namespace iosKeyboardTest.Android {
    public class MyInputConfig {
        #region Private Variables
        #endregion

        #region Constants
        const int DEFAULT_VIBRATION_LEVEL = 500;
        #endregion

        #region Statics

        private static MyInputConfig _instance;
        public static MyInputConfig Instance => _instance ?? (_instance = new MyInputConfig());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public int VibrationLevel { get; set; } = DEFAULT_VIBRATION_LEVEL;
        #endregion

        #region Constructors
        private MyInputConfig() { }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
