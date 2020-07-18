using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpMeasurements : MpViewModelBase {
        private static readonly Lazy<MpMeasurements> lazy = new Lazy<MpMeasurements>(() => new MpMeasurements());
        public static MpMeasurements Instance { get { return lazy.Value; } }

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
                    _mainWindowHeight
                );
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
        public double TrayHeight {
            get {
                return MainWindowRect.Height - TitleMenuHeight - FilterMenuHeight;
            }
        }
        public double ClipTileMargin {
            get {
                return TrayHeight / 50;
            }
        }
        public double ClipTileDropShadowRadius {
            get {
                return 10;
            }
        }
        public double ClipTileSize {
            get {
                return TrayHeight - (ClipTileMargin * 2) - ClipTileDropShadowRadius - 30;
            }
        }
        public double ClipTileTitleIconSize {
            get {
                return ClipTileTitleHeight * 0.75;
            }
        }
        public double ClipTileBorderSize {
            get {
                return ClipTileSize - ClipTileDropShadowRadius;
            }
        }
        public double ClipTileBorderThickness {
            get {
                return 5;
            }
        }
        public double ClipTileTitleHeight {
            get {
                return ClipTileSize / 5;
            }
        }
        public double TileContentHeight {
            get {
                return ClipTileSize - ClipTileTitleHeight - (ClipTileMargin*2);
            }
        }
    }
}
