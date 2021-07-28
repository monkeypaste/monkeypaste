using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonkeyPaste.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.Dependency(typeof(MpUiLocationFetcher))]
namespace MonkeyPaste.Droid {
    public class MpUiLocationFetcher : MpIUiLocationFetcher {
        public Point GetCoordinates(global::Xamarin.Forms.VisualElement element, bool ignoreDensity = false) {
            var renderer = Platform.GetRenderer(element);
            var nativeView = renderer.View;
            var location = new int[2];
            var density = ignoreDensity ? 1 : nativeView.Context.Resources.DisplayMetrics.Density;

            nativeView.GetLocationOnScreen(location);
            return new Point(location[0] / density, location[1] / density);
        }

        public double GetDensity(VisualElement element) {
            var renderer = Platform.GetRenderer(element);
            var nativeView = renderer.View;
            return nativeView.Context.Resources.DisplayMetrics.Density;

        }
    }
}