using System;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpMeasurements : MpSingleton<MpMeasurements> {
        public readonly double PHI = 1.618033988749894;

        public MpMeasurements() {
            MpConsole.WriteLine($"Old screen height: {SystemParameters.PrimaryScreenHeight * 0.35} New Height: {MainWindowDefaultHeight}");
        }

        public void Measure() { }

        #region Screen

        public double ScreenWidth { get; private set; } = SystemParameters.PrimaryScreenWidth;
        public double ScreenHeight { get; private set; } = SystemParameters.PrimaryScreenHeight;

        public double TaskBarHeight { get; private set; } = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;
        
        public double WorkAreaBottom { 
            get {
                return ScreenHeight - TaskBarHeight;
            } 
        }

        public double ClipTileExpandedMaxHeightPadding {
            get {
                return 80;
            }
        }

        #endregion

        #region General

        public double ScrollbarWidth {
            get {
                return 20;
            }
        }

        public double MainWindowToScreenHeightRatio { get; set; } = 0.35;


        public Rect DottedBorderRect {
            get {
                return new Rect(0, 0, 50, 50);
            }
        }

        public Rect SolidBorderRect {
            get {
                return new Rect(50, 0, 50, 50);
            }
        }
        #endregion

        #region Drop Canvas



        #endregion

        #region Main Window

        public double MainWindowDefaultHeight {
            get {
                return SystemParameters.PrimaryScreenHeight * MainWindowToScreenHeightRatio;// SystemParameters.PrimaryScreenHeight - (SystemParameters.PrimaryScreenHeight / PHI); 
            }
        }

        public double MainWindowMinHeight {
            get {
                return MainWindowDefaultHeight * 0.5;
            }
        }

        public double MainWindowMaxHeight {
            get {
                return SystemParameters.WorkArea.Height;
            }
        }

        #endregion

        #region Main Menu Rows

        public double TitleMenuHeight {
            get {
                return MainWindowDefaultHeight / 20;
            }
        }


        public double FilterMenuHeight {
            get {
                return MainWindowDefaultHeight / 8;
            }
        }

        public double SearchDetailRowHeight {
            get {
                return MainWindowDefaultHeight / 10;
            }
        }

        public double ClipTrayMinHeight {
            get {
                return MainWindowDefaultHeight - TitleMenuHeight - FilterMenuHeight;
            }
        }

        #endregion

        #region Analytic Menu

        #endregion

        #region App Mode Menu

        public double AppStateButtonPanelWidth {
            get {
                return MainWindowDefaultHeight / 7;
            }
        }

        #endregion

        #region Tag Tray

        public double TagTrayDefaultMaxWidth {
            get {
                return ScreenWidth * 0.333;
            }
        }

        #endregion

        #region Clip Tray

        public int MaxRecentClipTiles {
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

        

        #region Clip Tile

        #region Outer Border

        public double ClipTileMargin {
            get {
                return ClipTrayMinHeight / 55;
            }
        }

        public double ClipTilePadding {
            get {
                return 17;
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

        public double ClipTileInnerBorderSize {
            get {
                return ClipTileMinSize - (ClipTileBorderThickness * 2);
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
