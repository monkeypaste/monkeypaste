using Avalonia;
using Microsoft.Maui.Devices;
using System;

namespace iosKeyboardTest.Android
{
    public static class AndroidDisplayInfo
    {
        public static Size UnscaledPortraitSize { get; private set; }
        public static Size UnscaledLandscapeSize { get; private set; }
        public static Size UnscaledSize =>
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ?
                UnscaledPortraitSize : UnscaledLandscapeSize;
        public static Size ScaledPortraitSize { get; private set; }
        public static Size ScaledLandscapeSize { get; private set; }
        public static Size ScaledSize =>
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ?
                ScaledPortraitSize : ScaledLandscapeSize;
        public static double Scaling { get; private set; }
        public static bool IsPortrait =>
            ScaledSize.Width < ScaledSize.Height;
        static AndroidDisplayInfo()
        {
            Scaling = DeviceDisplay.MainDisplayInfo.Density;

            double a = DeviceDisplay.MainDisplayInfo.Width;
            double b = DeviceDisplay.MainDisplayInfo.Height;
            double w = Math.Min(a, b);
            double h = Math.Max(a, b);
            UnscaledPortraitSize = new Size(w, h);
            UnscaledLandscapeSize = new Size(h, w);

            ScaledPortraitSize = new Size(w / Scaling, h / Scaling);
            ScaledLandscapeSize = new Size(ScaledPortraitSize.Height, ScaledPortraitSize.Width);
        }
    }
}
