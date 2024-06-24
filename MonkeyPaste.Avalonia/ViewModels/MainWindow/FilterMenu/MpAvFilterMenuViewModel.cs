using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using System;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvFilterMenuViewModel :
        MpAvViewModelBase {
        #region Statics

        private static MpAvFilterMenuViewModel _instance;
        public static MpAvFilterMenuViewModel Instance => _instance ?? (_instance = new MpAvFilterMenuViewModel());

        #endregion

        #region Properties

        #region State
        public bool IsExpanded { get; set; } =
#if MOBILE_OR_WINDOWED
            false;
#else
            true;
#endif

        #endregion

        #region Layout
        public int FilterAnimTimeMs => 300; // NOTE needs to match resource time
        public double DefaultFilterMenuFixedSize =>
            !IsExpanded ? 0 : MpAvThemeViewModel.Instance.IsMobileOrWindowed ? 50 : 40;

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
                case nameof(IsExpanded):
                    MpMessenger.SendGlobal(MpMessageType.FilterExpandedChanged);
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
                double tag_tray_pad = 25;
                MaxTagTrayScreenWidth =
                fmv.Bounds.Width -
                fmv.SearchBoxView.Bounds.Width -
                fmv.SortView.Bounds.Width -
                tag_tray_pad;

                double total_item_width = fmv.TagTrayView.IsVisible ? fmv.TagTrayView.TagTray.Bounds.Width : 0;
                if (MpAvThemeViewModel.Instance.IsMultiWindow &&
                    total_item_width >= MaxTagTrayScreenWidth) {
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

        #region Commands

        public ICommand ToggleIsFilterMenuExpandedCommand => new MpCommand(
            () => {
                IsExpanded = !IsExpanded;
            });

        #endregion
    }
}
