namespace MonkeyPaste.Avalonia {
    public class MpAvFilterMenuViewModel :
        MpViewModelBase {
        #region Statics

        private static MpAvFilterMenuViewModel _instance;
        public static MpAvFilterMenuViewModel Instance => _instance ?? (_instance = new MpAvFilterMenuViewModel());

        #endregion

        #region Properties

        #region Layout

        public double DefaultFilterMenuFixedSize => 40;

        public double FilterMenuWidth { get; set; }
        public double FilterMenuHeight { get; set; }

        public double ObservedTagTrayWidth { get; set; }
        public double ObservedSortViewWidth { get; set; }
        public double ObservedSearchBoxWidth { get; set; }

        public double MaxSearchBoxWidth =>
            FilterMenuWidth -
            ObservedTagTrayWidth -
            ObservedSortViewWidth;

        #endregion

        #endregion

        #region Constructors

        public MpAvFilterMenuViewModel() {
            PropertyChanged += MpAvTitleMenuViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Private Methods

        private void MpAvTitleMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ObservedSearchBoxWidth):
                case nameof(ObservedSortViewWidth):
                case nameof(ObservedTagTrayWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.MaxTagTrayScreenWidth));
                    break;

            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                case MpMessageType.ResizingMainWindowComplete:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    OnPropertyChanged(nameof(MaxSearchBoxWidth));

                    break;
            }
        }
        #endregion
    }
}
