using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public static class MpScreenHelpers {
        #region Visual

        public static Point GetScreenCoordinates(this VisualElement view) {
            var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            return locationFetcher.GetCoordinates(view);
        }

        public static Rectangle GetScreenRect(this VisualElement view) {
            var origin = view.GetScreenCoordinates();
            return new Rectangle(origin, new Size(view.Width, view.Height));
        }

        public static Point GetScreenPoint(this Point p, VisualElement view) {
            var locationFetcher = DependencyService.Get<MpIUiLocationFetcher>();
            var density = locationFetcher.GetDensity(view);
            return new Point(p.X / density, p.Y / density);
        }

        #endregion
    }
}
