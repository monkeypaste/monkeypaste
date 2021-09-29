using System;
using System.Windows;

namespace MpWpfApp {
    public class MpMeasurements {
        private static readonly Lazy<MpMeasurements> _Lazy = new Lazy<MpMeasurements>(() => new MpMeasurements());
        public static MpMeasurements Instance { get { return _Lazy.Value; } }

        public MpMeasurements() { }

        public void Measure() { }

        #region Screen

        public double ScreenWidth { get; private set; } = SystemParameters.PrimaryScreenWidth;
        public double ScreenHeight { get; private set; } = SystemParameters.PrimaryScreenHeight;

        public double TaskBarHeight { get; private set; } = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;
        #endregion

        #region General

        public double ScrollbarWidth {
            get {
                return 20;
            }
        }

        public double MainWindowToScreenHeightRatio { get; set; } = 0.35;

        #endregion

        #region Main Window

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

        public Rect MainWindowRect {
            get {
                return new Rect(
                    0,
                    SystemParameters.WorkArea.Height - MainWindowMinHeight,
                    ScreenWidth,
                    MainWindowMinHeight);
            }
        }

        #endregion

        #region Drop Canvas



        #endregion

        #region Title Menu

        public double TitleMenuHeight {
            get {
                return MainWindowMinHeight / 20;
            }
        }

        #endregion

        #region Filter Menu

        public double FilterMenuHeight {
            get {
                return MainWindowMinHeight / 8;
            }
        }

        #endregion

        #region App Mode Menu

        public double AppStateButtonPanelWidth {
            get {
                return MainWindowMinHeight / 7;
            }
        }

        #endregion

        #region Tag Tray

        public double MaxTagTrayWidth {
            get {
                return MainWindowRect.Width * 0.333;
            }
        }

        #endregion

        #region Clip Tray

        public int MaxRecentClipItems {
            get {
                return TotalVisibleClipTiles;
            }
        }

        public int TotalVisibleClipTiles {
            get {
                return (int)(ClipTrayWidth / ClipTileMinSize) + 1;
            }
        }

        public double ClipTrayWidth {
            get {
                return ScreenWidth - AppStateButtonPanelWidth;
            }
        }

        public double ClipTrayMinHeight {
            get {
                return MainWindowRect.Height - TitleMenuHeight - FilterMenuHeight;
            }
        }

        #region Clip Tile

        #region Outer Border

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

        public double ClipTileMinSize {
            get {
                return ClipTrayMinHeight - (ClipTileMargin * 2) - ClipTilePadding;
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

        public double ClipTileEditModeMinWidth {
            get {
                return 700;
            }
        }

        #endregion

        #region Title

        #region Outer Border

        public double ClipTileTitleHeight {
            get {
                return ClipTileMinSize / 5;
            }
        }

        #endregion

        #region Icon

        public double ClipTileTitleIconSize {
            get {
                return ClipTileTitleHeight * 0.75;
            }
        }

        public double ClipTileTitleFavIconSize {
            get {
                return ClipTileTitleIconSize * 1.0;
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

        public double ClipTileTitleFavIconBorderSize {
            get {
                return ClipTileTitleFavIconSize * ClipTileTitleIconBorderSizeRatio;
            }
        }

        public double ClipTileTitleIconRightMargin {
            get {
                return 10;
            }
        }

        public double ClipTileTitleIconCanvasLeft {
            get {
                return ClipTileBorderMinSize - ClipTileTitleHeight - ClipTileTitleIconRightMargin;
            }
        }

        #endregion

        #region Title Text

        public double ClipTileTitleDefaultFontSize {
            get {
                return 14;
            }
        }

        public double ClipTileTitleTextGridCanvasTop {
            get {
                return 2;
            }
        }


        public double ClipTileTitleFontSize {
            get {
                return 20;
            }
        }


        public double ClipTileTitleTextGridCanvasRight {
            get {
                return ClipTileTitleIconCanvasLeft - 5;
            }
        }


        public double ClipTileTitleTextGridCanvasLeft {
            get {
                return 5;
            }
        }

        public double ClipTileTitleTextGridMaxWidth {
            get {
                return ClipTileBorderMinSize - ClipTileTitleIconBorderSize;
            }
        }

        public double ClipTileTitleTextGridMinWidth {
            get {
                return 10;
            }
        }

        #endregion

        #endregion

        #region Content

        public double ClipTileScrollViewerWidth {
            get {
                return ClipTileContentMinWidth - ScrollbarWidth;
            }
        }

        public double ClipTileEditModeContentMargin {
            get {
                return 10;
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

        #region Content Items

        public double ClipTileContentItemBorderThickness {
            get {
                return 1;
            }
        }

        public double ClipTileContentItemRtbViewPadding {
            get {
                return 5;
            }
        }

        public double ClipTileContentItemMinHeight {
            get {
                return ClipTileContentHeight / 5;
            }
        }


        public double ClipTileContentItemDragButtonSize {
            get {
                return ClipTileContentItemMinHeight * 0.5;
            }
        }

        public double ClipTileFileListRowHeight {
            get {
                return ClipTileContentHeight / 8;
            }
        }

        #endregion

        #endregion

        #region Detail

        public double ClipTileDetailHeight {
            get {
                return ClipTileTitleHeight / 4;
            }
        }

        #endregion

        #region Busy Overlay

        public double ClipTileLoadingSpinnerSize {
            get {
                return ClipTileMinSize * 0.333;
            }
        }

        #endregion

        #region Toolbars

        #region Edit Content
        public double ClipTileEditToolbarHeight {
            get {
                return 40;
            }
        }

        public double ClipTileEditToolbarIconSize {
            get {
                return 22;
            }
        }
        #endregion

        #region Edit Template
        public double ClipTileEditTemplateToolbarHeight {
            get {
                return 40;
            }
        }
        #endregion

        #region Paste Template
        public double ClipTilePasteTemplateToolbarHeight {
            get {
                return 40;
            }
        }
        #endregion

        #endregion

        #endregion

        #endregion
    }
}
