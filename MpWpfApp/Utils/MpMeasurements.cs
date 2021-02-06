using System;
using System.Windows;

namespace MpWpfApp {
    public class MpMeasurements : MpViewModelBase {
        private static readonly Lazy<MpMeasurements> _Lazy = new Lazy<MpMeasurements>(() => new MpMeasurements());
        public static MpMeasurements Instance { get { return _Lazy.Value; } }

        private double _mainWindowToScreenHeightRatio = 0.35;

        private MpMeasurements() { }

        private double _screenWidth = SystemParameters.PrimaryScreenWidth;
        private double _screenHeight = SystemParameters.PrimaryScreenHeight;

        private double _mainWindowHeight {
            get {
                return SystemParameters.PrimaryScreenHeight * _mainWindowToScreenHeightRatio;
            }
        }

        private double _taskBarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

        public Rect MainWindowRect {
            get {
                return new Rect(
                    0,
                    SystemParameters.WorkArea.Height - _mainWindowHeight,
                    _screenWidth,
                    _mainWindowHeight);
            }
        }

        public double TitleMenuHeight {
            get {
                return _mainWindowHeight / 20;
            }
        }

        public double FilterMenuHeight {
            get {
                return _mainWindowHeight / 8;
            }
        }

        public double AppStateButtonPanelWidth {
            get {
                return _mainWindowHeight / 7;
            }
        }

        public double ClipTrayWidth {
            get {
                return _screenWidth - AppStateButtonPanelWidth;
            }
        }
        public double ClipTrayHeight {
            get {
                return MainWindowRect.Height - TitleMenuHeight - FilterMenuHeight;
            }
        }

        public double ClipTileMargin {
            get {
                return ClipTrayHeight / 50;
            }
        }

        public double ClipTileContentMargin {
            get {
                return ClipTileMargin / 5;
            }
        }

        public double ClipTilePadding {
            get {
                return 15;
            }
        }

        public double ClipTileSize {
            get {
                return ClipTrayHeight - (ClipTileMargin * 2) - ClipTilePadding;
            }
        }

        public double ClipTileLoadingSpinnerSize {
            get {
                return ClipTileSize * 0.333;
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

        public double ClipTileTitleIconCanvasLeft {
            get {
                return ClipTileBorderSize - ClipTileTitleHeight - ClipTileTitleIconRightMargin;
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

        public double ClipTileBorderSize {
            get {
                return ClipTileSize - ClipTilePadding;
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

        public double ClipTileEditModeContentMinWidth {
            get {
                return ClipTileEditModeMinWidth - ClipTileEditModeContentMargin;
            }
        }

        public double ClipTileEditToolbarIconSize {
            get {
                return 22;
            }
        }

        public double ClipTileTitleHeight {
            get {
                return ClipTileSize / 5;
            }
        }

        public double ClipTileDetailHeight {
            get {
                return ClipTileTitleHeight / 4;
            }
        }

        public double ClipTileContentHeight {
            get {
                return ClipTileSize - ClipTileTitleHeight - ClipTileMargin - ClipTileBorderThickness - ClipTileDetailHeight;
            }
        }

        public double ClipTileContentWidth {
            get {
                return ClipTileSize - (ClipTileMargin * 2) - (ClipTileBorderThickness * 2);
            }
        }
        public double ClipTileFileListRowHeight {
            get {
                return ClipTileContentHeight / 8;
            }
        }
    }
}
