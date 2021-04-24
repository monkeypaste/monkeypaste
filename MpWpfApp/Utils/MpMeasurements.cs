using System;
using System.Windows;

namespace MpWpfApp {
    public class MpMeasurements : MpViewModelBase {
        private static readonly Lazy<MpMeasurements> _Lazy = new Lazy<MpMeasurements>(() => new MpMeasurements());
        public static MpMeasurements Instance { get { return _Lazy.Value; } }

        public double MainWindowToScreenHeightRatio = 0.35;

        private MpMeasurements() {
            Properties.Settings.Default.MaxRecentClipItems = 3;// TotalVisibleClipTiles * 2;
            Properties.Settings.Default.Save();
        }

        private double _screenWidth = SystemParameters.PrimaryScreenWidth;
        private double _screenHeight = SystemParameters.PrimaryScreenHeight;

        public double MainWindowMinHeight {
            get {
                return SystemParameters.PrimaryScreenHeight * MainWindowToScreenHeightRatio;
            }
        }

        public double MainWindowMaxHeight {
            get {
                return SystemParameters.WorkArea.Height;
            }
        }

        private double _taskBarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

        public Rect MainWindowRect {
            get {
                return new Rect(
                    0,
                    SystemParameters.WorkArea.Height - MainWindowMinHeight,
                    _screenWidth,
                    MainWindowMinHeight);
            }
        }

        public double TitleMenuHeight {
            get {
                return MainWindowMinHeight / 20;
            }
        }

        public double FilterMenuHeight {
            get {
                return MainWindowMinHeight / 8;
            }
        }

        public double AppStateButtonPanelWidth {
            get {
                return MainWindowMinHeight / 7;
            }
        }

        public double ClipTrayWidth {
            get {
                return _screenWidth - AppStateButtonPanelWidth;
            }
        }
        public double ClipTrayMinHeight {
            get {
                return MainWindowRect.Height - TitleMenuHeight - FilterMenuHeight;
            }
        }


        public double ClipTileMargin {
            get {
                return ClipTrayMinHeight / 50;
            }
        }

        public double ClipTilePadding {
            get {
                return 15;
            }
        }

        public double ClipTileExpandedMargin {
            get {
                return ClipTilePadding * 2;
            }
        }

        public int TotalVisibleClipTiles {
            get {
                return (int)(ClipTrayWidth / ClipTileMinSize);
            }
        }

        public double ClipTileMinSize {
            get {
                return ClipTrayMinHeight - (ClipTileMargin * 2) - ClipTilePadding;
            }
        }

        public double ClipTileLoadingSpinnerSize {
            get {
                return ClipTileMinSize * 0.333;
            }
        }

        public double ClipTileTitleIconSize {
            get {
                return ClipTileTitleHeight * 0.75;
            }
        }
        public double ClipTileTitleIconBorderSizeRatio {
            get {
                return 1.25;
            }
        }

        public double ClipTileTitleIconBorderSize {
            get {
                return ClipTileTitleIconSize * ClipTileTitleIconBorderSizeRatio;
            }
        }

        public double ClipTileTitleIconRightMargin {
            get {
                return 10;
            }
        }

        public double ScrollbarWidth {
            get {
                return 20;
            }
        }

        public double ClipTileScrollViewerWidth {
            get {
                return ClipTileContentMinWidth - ScrollbarWidth;
            }
        }

        public double ClipTileTitleIconCanvasLeft {
            get {
                return ClipTileBorderMinSize - ClipTileTitleHeight - ClipTileTitleIconRightMargin;
            }
        }

        public double ClipTileTitleTextGridCanvasTop {
            get {
                return 2;
            }
        }

        public double ClipTileTitleTextGridCanvasLeft {
            get {
                return 5;
            }
        }

        public double ClipTileTitleFontSize {
            get {
                return 20;
            }
        }

        public double RtbCompositeItemTitleFontSize {
            get {
                return 14;
            }
        }

        public double RtbEditModeMinMargin {
            get {
                return 5;
            }
        }

        public double ClipTileTitleTextGridCanvasRight {
            get {
                return ClipTileTitleIconCanvasLeft - 5;
            }
        }

        public double ClipTileTitleTextGridWidth {
            get {
                return ClipTileTitleTextGridCanvasRight - ClipTileTitleTextGridCanvasLeft;
            }
        }

        public double ClipTileBorderMinSize {
            get {
                return ClipTileMinSize - ClipTilePadding;
            }
        }

        public double ClipTileBorderMinMaxSize {
            get {
                return 650;
            }
        }

        public double ClipTileBorderThickness {
            get {
                return 5;
            }
        }

        public double ClipTilePasteTemplateToolbarHeight {
            get {
                return 40;
            }
        }

        public double ClipTileEditTemplateToolbarHeight {
            get {
                return 40;
            }
        }

        public double ClipTileEditToolbarHeight {
            get {
                return 40;
            }
        }

        public double ClipTileEditModeMinWidth {
            get {
                return 700;
            }
        }

        public double ClipTileEditModeContentMargin {
            get {
                return 10;
            }
        }

        public double ClipTileEditToolbarIconSize {
            get {
                return 22;
            }
        }

        public double ClipTileTitleHeight {
            get {
                return ClipTileMinSize / 5;
            }
        }

        public double ClipTileDetailHeight {
            get {
                return ClipTileTitleHeight / 4;
            }
        }

        public double ClipTileContentHeight {
            get {
                return ClipTileMinSize - ClipTileTitleHeight - ClipTileMargin - ClipTileBorderThickness - ClipTileDetailHeight;
            }
        }

        public double ClipTileContentMargin {
            get {
                return (ClipTileMargin * 2) - (ClipTileBorderThickness * 2);
            }
        }

        public double ClipTileContentMinWidth {
            get {
                return ClipTileMinSize - ClipTileContentMargin;
            }
        }

        public double ClipTileContentMinMaxWidth {
            get {
                return ClipTileBorderMinMaxSize - ClipTileContentMargin - ClipTileEditModeContentMargin;
            }
        }

        public double ClipTileSubItemOverlayMargin {
            get {
                return 5;
            }
        }

        public double RtbCompositeItemMinHeight {
            get {
                return ClipTileContentHeight / 5;
            }
        }

        public double RtbCompositeAppIconSize {
            get {
                return ClipTileContentHeight / 14;
            }
        }

        public double RtbCompositeAppIconBorderSizeRatio {
            get {
                return 1.5;
            }
        }

        public double RtbCompositeAppIconBorderSize {
            get {
                return RtbCompositeAppIconSize * RtbCompositeAppIconBorderSizeRatio;
            }
        }

        public double RtbCompositeDragButtonSize {
            get {
                return ClipTileContentHeight / 7;
            }
        }

        public double ClipTileFileListRowHeight {
            get {
                return ClipTileContentHeight / 8;
            }
        }
    }
}
