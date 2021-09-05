
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MonkeyPaste.MpIUiLocationFetcher))]
namespace MonkeyPaste.Mac {
    public class MpUiLocationFetcher : MpIUiLocationFetcher {
        public Point GetCoordinates(global::Xamarin.Forms.VisualElement element, bool ignoreDensity = false) {
            //var renderer = Platform.GetRenderer(element);
            //var nativeView = renderer.View;
            //var location = new int[2];
            //var density = ignoreDensity ? 1 : nativeView.Context.Resources.DisplayMetrics.Density;

            //nativeView.GetLocationOnScreen(location);
            //return new Point(location[0] / density, location[1] / density);
            return new Point();
        }

        public double GetDensity(VisualElement element) {
            //var renderer = Platform.GetRenderer(element);
            //var nativeView = renderer.View;
            //return nativeView.Context.Resources.DisplayMetrics.Density;
            return 1;
        }
    }
}