using Android.App;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using Xamarin.Essentials;
using AvApplication = Avalonia.Application;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdScreenInfo : MpAvScreenInfoBase {
        #region Private Variables
        
        private double _navHeightPortrait;
        private double _navHeightLandscape;
        private double _statusHeight;
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
        
        public MpAvAdScreenInfo() {
            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkingArea = Bounds;
                IsPrimary = true;
            }
        }
        #endregion

        #region Public Methods
        public MpAvAdScreenInfo(Activity activity) {
            Init(activity);
        }

        private void Init(Activity activity) {
            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkingArea = Bounds;
                IsPrimary = true;
            } else {
                // s9 info:
                // nav height: 144
                // status height: 72
                // scaling: 3
                // dim: 1080 x 2220
                // scaled:
                // nav height: 48
                // status height: 24
                // dim: 360 x 740
                DisplayInfo di = DeviceDisplay.MainDisplayInfo;
                
                int nid = activity.ApplicationContext.Resources.GetIdentifier("navigation_bar_height","dimen","android");
                if (nid > 0) {
                    // s9 is 144
                    _navHeightPortrait = (double)activity.ApplicationContext.Resources.GetDimension(nid);
                }
                nid = activity.ApplicationContext.Resources.GetIdentifier("navigation_bar_height_landscape","dimen","android");
                if (nid > 0) {
                    _navHeightLandscape = (double)activity.ApplicationContext.Resources.GetDimension(nid);
                }
                int sid = activity.ApplicationContext.Resources.GetIdentifier("status_bar_height", "dimen", "android");
                if (sid > 0) {
                    // s9 72
                    _statusHeight = (double)activity.ApplicationContext.Resources.GetDimension(sid);
                }
                // s9 is 3
                Scaling = di.Density;

                _sw = IsVertical ? di.Width : di.Height;
                _sh = IsVertical ? di.Height : di.Width;
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
