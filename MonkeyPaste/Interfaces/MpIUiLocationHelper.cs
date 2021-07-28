using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public interface MpIUiLocationFetcher {
        Point GetCoordinates(global::Xamarin.Forms.VisualElement view, bool ignoreDensity = false);
        double GetDensity(global::Xamarin.Forms.VisualElement element);
    }
}
