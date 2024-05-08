using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvFilterMenuViewModel :
        MpAvViewModelBase {
        #region Statics

        private static MpAvFilterMenuViewModel _instance;
        public static MpAvFilterMenuViewModel Instance => _instance ?? (_instance = new MpAvFilterMenuViewModel());

        #endregion

        #region Properties

        #region Layout
        public int FilterAnimTimeMs => 300; // NOTE needs to match resource time
        public double DefaultFilterMenuFixedSize =>
#if DESKTOP
        40;
#else
            40;
#endif

        public double FilterMenuHeight { get; set; }

        public double ObservedFilterMenuWidth { get; set; }
        public double ObservedTagTrayWidth { get; set; }
        public double ObservedSortViewWidth { get; set; }
        public double ObservedSearchBoxWidth { get; set; }

        public double MaxSearchBoxWidth { get; set; }
        public double MaxTagTrayScreenWidth { get; set; }

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
                    UpdateFilterLayouts();
                    break;

            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                case MpMessageType.ResizingMainWindowComplete:
                case MpMessageType.MainWindowOrientationChangeEnd:
                case MpMessageType.FilterItemSizeChanged:
                case MpMessageType.MainWindowSizeChanged:
                    UpdateFilterLayouts();
                    break;
            }
        }

        private void UpdateFilterLayouts() {
            var ttrvm = MpAvTagTrayViewModel.Instance;
            double total_item_width = ttrvm.PinnedItems.Count() * 130.0d;

            double sbw = MpAvSearchBoxViewModel.Instance.IsExpanded ?
                335.0d :
                ObservedSearchBoxWidth;

            double svw = MpAvClipTileSortDirectionViewModel.Instance.IsExpanded ?
                160 :
                ObservedSortViewWidth;

            MaxTagTrayScreenWidth =
                ObservedFilterMenuWidth -
                svw -
                sbw;

            if (total_item_width > MaxTagTrayScreenWidth) {
                double total_navs_width = ((ttrvm.NavButtonSize + 15) * 2);
                MaxTagTrayScreenWidth -= total_navs_width;
                ttrvm.IsNavButtonsVisible = MaxTagTrayScreenWidth > total_navs_width;
            } else {
                ttrvm.IsNavButtonsVisible = false;
            }
            MaxTagTrayScreenWidth = Math.Max(0, MaxTagTrayScreenWidth);

            MaxSearchBoxWidth =
                Math.Max(
                    MpAvSearchBoxViewModel.Instance.IsExpanded ?
                        sbw :
                        35,
                    ObservedFilterMenuWidth -
                    ObservedTagTrayWidth -
                    svw);

        }
        #endregion
    }
}
