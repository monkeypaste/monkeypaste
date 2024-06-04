using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvMainWindowTitleMenuViewModel : MpAvViewModelBase {
        #region Statics

        private static MpAvMainWindowTitleMenuViewModel _instance;
        public static MpAvMainWindowTitleMenuViewModel Instance => _instance ?? (_instance = new MpAvMainWindowTitleMenuViewModel());

        #endregion

        #region Properties

        #region Appearance


        public string ZoomSliderHexColor {
            get {
                if (IsZoomSliderHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.White;
            }
        }

        #endregion

        #region Layout
        public double ZoomSliderLength => 125;
        public double ZoomSliderLineWidth => 1;

        public double ZoomSliderLineLengthRatio =>
            MpAvThemeViewModel.Instance.IsMultiWindow ? 0.5 : 0.25;
        public double ZoomSliderValueLength =>
            MpAvThemeViewModel.Instance.IsMultiWindow ? 3 : 5;
        public double ZoomSliderShortMargin => 3;
        public double ZoomSliderLongMargin => 10;

        public double TitleDragHandleLongLength => 50;
        public double DefaultTitleMenuFixedLength =>
            MpAvThemeViewModel.Instance.IsMultiWindow ? 20 : 50;
        public double TitleMenuWidth { get; set; }
        public double TitleMenuHeight { get; set; }

        public double SettingsButtonWidth { get; set; }
        public double LockButtonWidth { get; set; }
        //public double ZoomSliderWidth { get; set; }
        public double LayoutButtonWidth { get; set; }
        public double OrientationButtonWidth { get; set; }

        #endregion

        #region State
        public bool IsZoomSliderHovering { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvMainWindowTitleMenuViewModel() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            PropertyChanged += MpAvTitleMenuViewModel_PropertyChanged;
        }

        #endregion

        #region Private Methods

        private void MpAvTitleMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {

            //}
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowActivated:
                case MpMessageType.MainWindowDeactivated:
                case MpMessageType.MainWindowOpened:
                    //OnPropertyChanged(nameof(TitleBarBackgroundHexColor));
                    break;
            }
        }


        #endregion
    }
}
