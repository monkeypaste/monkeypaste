using System;
using System.Windows;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMeasurements : MpViewModelBase {
        private static readonly Lazy<MpMeasurements> _Lazy = new Lazy<MpMeasurements>(() => new MpMeasurements());
        public static MpMeasurements Instance { 
            get { 
                return _Lazy.Value; 
            } }

        public double MainWindowToScreenHeightRatio = 0.35;

        private MpMeasurements() {
            if (MpPreferences.Instance.ThisDeviceType == MpUserDeviceType.Windows) {                
                return;
            }
            UpdateMeasurements();
            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged; ;
        }

        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e) {
            UpdateMeasurements();
        }

        public void UpdateMeasurements() {
            // Properties.Settings.Default.MaxRecentClipItems = TotalVisibleClipTiles;
            // Properties.Settings.Default.Save();

            if (MpPreferences.Instance.ThisDeviceType == MpUserDeviceType.Windows) {
                return;
            }
            Device.BeginInvokeOnMainThread(() => {
                // to get display info required for iOS so just always running on main thread

                var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;

                // Width (in pixels)
                _screenWidth = mainDisplayInfo.Width;

                // Height (in pixels)
                _screenHeight = mainDisplayInfo.Height;

                // Screen density
                _screenDensity = mainDisplayInfo.Density;

                // TODO adjust these based on device
                _workAreaHeight = _screenHeight;
                _workAreaWidth = _screenWidth;

                // Orientation (Landscape, Portrait, Square, Unknown)
                var orientation = mainDisplayInfo.Orientation;

                // Rotation (0, 90, 180, 270)
                var rotation = mainDisplayInfo.Rotation;
            });
        }

        private static double _screenWidth = 0;//SystemParameters.PrimaryScreenWidth;
        private static double _screenHeight = 0;//_screenHeight;

        private static double _workAreaHeight = 0;
        private static double _workAreaWidth = 0;

        private static double _screenDensity = 0;

        public double MainWindowMinHeight {
            get {
                return _screenHeight * MainWindowToScreenHeightRatio;
            }
        }

        public double MainWindowMaxHeight {
            get {
                return _workAreaHeight;
            }
        }

        private double _taskBarHeight = _screenHeight - _workAreaHeight;

        public Rect MainWindowRect {
            get {
                return new Rect(
                    0,
                    _workAreaHeight - MainWindowMinHeight,
                    _screenWidth,
                    MainWindowMinHeight);
            }
        }

        public double MaxTagTrayWidth {
            get {
                return MainWindowRect.Width * 0.333;
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
    }
}
