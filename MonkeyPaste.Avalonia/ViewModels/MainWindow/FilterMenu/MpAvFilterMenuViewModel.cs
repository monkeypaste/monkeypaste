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
            MpAvThemeViewModel.Instance.IsMobileOrWindowed ? 50 : 40;

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
            var fmv = MpAvMainView.Instance.FilterMenuView;
            var ttrvm = MpAvTagTrayViewModel.Instance;

            if(fmv.TagTrayView.IsVisible) {
                MaxTagTrayScreenWidth =
                fmv.Bounds.Width -
                fmv.SearchBoxView.Bounds.Width -
                fmv.SortView.Bounds.Width;

                double total_item_width = fmv.TagTrayView.IsVisible ? fmv.TagTrayView.TagTray.Bounds.Width : 0;
                if (total_item_width > MaxTagTrayScreenWidth) {
                    double total_navs_width = ((ttrvm.NavButtonSize + 15) * 2);
                    MaxTagTrayScreenWidth -= total_navs_width;
                    ttrvm.IsNavButtonsVisible = MaxTagTrayScreenWidth > total_navs_width;
                } else {
                    ttrvm.IsNavButtonsVisible = false;
                }
                MaxTagTrayScreenWidth = Math.Max(0, MaxTagTrayScreenWidth); 
                
                MaxSearchBoxWidth =
                fmv.Bounds.Width -
                    fmv.TagTrayView.Bounds.Width -
                    fmv.SortView.Bounds.Width;
            } else {
                MaxTagTrayScreenWidth = 0;
                MaxSearchBoxWidth = double.PositiveInfinity;
            }

            

        }
        #endregion
    }
}
