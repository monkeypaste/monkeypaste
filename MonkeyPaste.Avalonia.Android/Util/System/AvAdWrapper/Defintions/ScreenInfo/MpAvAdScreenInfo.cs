using Android.App;
using Android.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using AvApplication = Avalonia.Application;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdScreenInfo : MpAvScreenInfoBase {
        public override bool IsPrimary =>
            true;

        public MpAvAdScreenInfo() {
            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkArea = Bounds;
                IsPrimary = true;
            }
        }
        public MpAvAdScreenInfo(Activity activity) {
            Name = "Main Display";
            Init(activity);

        }

        private void Init(Activity activity) {
            if (AvApplication.Current is ISingleViewApplicationLifetime mobile &&
                mobile.MainView.GetVisualRoot() is IRenderRoot rr) {
                Scaling = 1;
                Bounds = new MpRect(MpPoint.Zero, rr.ClientSize.ToPortableSize());
                WorkArea = Bounds;
                IsPrimary = true;
            } else {
                DisplayInfo di = DeviceDisplay.MainDisplayInfo;

                int nid = activity.ApplicationContext.Resources.GetIdentifier(
                        DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ?
                        "navigation_bar_height" : "navigation_bar_height_landscape",
                        "dimen", "android");
                float nav_height = 0;
                if (nid > 0) {
                    nav_height = activity.ApplicationContext.Resources.GetDimension(nid);
                }


                int sid = activity.ApplicationContext.Resources.GetIdentifier("status_bar_height", "dimen", "android");
                float status_height = 0;
                if (sid > 0) {
                    status_height = activity.ApplicationContext.Resources.GetDimension(sid);
                }
                Scaling = di.Density;
                Bounds = new PixelRect(new PixelSize((int)di.Width, (int)di.Height - (int)nav_height - (int)status_height)).ToPortableRect(Scaling);
                WorkArea = Bounds;
            }

        }

        private MpRect _baseBounds;
        public override void Rotate(double angle) {
            if (_baseBounds == null) {
                _baseBounds = Bounds;
            }
            MpRect nb = _baseBounds;
            if (angle == 270 || angle == 90) {
                nb = new MpRect(0, 0, _baseBounds.Height, _baseBounds.Width);
            }
            Bounds = nb;
            WorkArea = nb;
        }
    }
}
