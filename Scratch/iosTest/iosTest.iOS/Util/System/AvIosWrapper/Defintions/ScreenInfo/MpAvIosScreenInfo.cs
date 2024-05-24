using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using Microsoft.Maui.Devices;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using AvApplication = Avalonia.Application;

namespace iosTest.iOS{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]

    public class MpAvIosScreenInfo : MpAvScreenInfoBase {
        #region Private Variables
        
        private double _navHeightPortrait = 0;
        private double _navHeightLandscape = 0;
        private double _statusHeight = 0;
        private double _sw;
        private double _sh;

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        bool IsVertical =>
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait;
        public override bool IsPrimary =>
            true;
        #endregion

        #region Constructors
        
        public MpAvIosScreenInfo() {
            Init();
        }
        #endregion

        #region Public Methods

        private void Init() {

            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView is Control mv &&
                mv.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkingArea = Bounds;
                IsPrimary = true;
            } else {
                // Get Metrics
                var di = DeviceDisplay.MainDisplayInfo;

                Scaling = di.Density;
                if(IsVertical) {
                    _sw = di.Width;
                    _sh = di.Height;
                } else {
                    _sw = di.Height;
                    _sh = di.Width;
                }

                var window = UIApplication.SharedApplication.Windows.FirstOrDefault();
                _statusHeight = window?.SafeAreaInsets.Top ?? 0;
                _navHeightPortrait = window?.SafeAreaInsets.Bottom ?? 0;
                Bounds = GetBounds(IsVertical);
                WorkingArea = GetWorkArea(IsVertical);
            }
        }

        public override void Rotate(double angle) {
            bool is_portrait = angle != 270 && angle != 90;
            Bounds = GetBounds(is_portrait);
            WorkingArea = GetWorkArea(is_portrait);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        
        private MpRect GetBounds(bool is_vert) {
            var ps = is_vert ? new PixelSize((int)_sw, (int)_sh) : new PixelSize((int)_sh, (int)_sw);
            return new PixelRect(ps).ToPortableRect(Scaling);
        }
        
        private MpRect GetWorkArea(bool is_vert) {
            var pix_bounds = GetBounds(is_vert).ToAvPixelRect(Scaling);
            int wa_x = pix_bounds.X;
            int wa_y = pix_bounds.Y + (int)_statusHeight;
            int wa_w = pix_bounds.Width;

            int nav_height = (int)(is_vert ? _navHeightPortrait : _navHeightLandscape);
            int bound_diff = (int)(_statusHeight + nav_height) * 1;
            int wa_h = pix_bounds.Height - bound_diff;
            return new PixelRect(wa_x, wa_y, wa_w, wa_h).ToPortableRect(Scaling);
        }
        #endregion

        #region Commands
        #endregion
    }
}
