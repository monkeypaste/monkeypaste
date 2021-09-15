using MonkeyPaste.UWP;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MpUiLocationFetcher))]
namespace MonkeyPaste.UWP {
    public class MpUiLocationFetcher : MpIUiLocationFetcher {
        public Point GetCoordinates(VisualElement element, bool ignoreDensity = false) {
            var location = new Point(element.X, element.Y);
            double density = ignoreDensity ? 1 : GetDensity(element);
            location = new Point(location.X / density, location.Y / density);
            return location;
        }

        public double GetDensity(VisualElement element) {
            return MonkeyPaste.MpMainDisplayInfo.MainDisplayInfo.Density;
        }
    }
}