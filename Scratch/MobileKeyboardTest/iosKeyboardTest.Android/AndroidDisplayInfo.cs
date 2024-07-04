using Avalonia;
using Microsoft.Maui.Devices;
using System;

namespace iosKeyboardTest.Android
{
    public static class AndroidDisplayInfo
    {
        public static Size ScaledPortraitSize { get; private set; }
        public static Size ScaledLandscapeSize { get; private set; }
        public static Size ScaledSize =>
            DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait ?
                ScaledPortraitSize : ScaledLandscapeSize;
        public static double Scaling =>
            DeviceDisplay.MainDisplayInfo.Density;
        static AndroidDisplayInfo()
        {
            double scaling = DeviceDisplay.MainDisplayInfo.Density;
            double a = DeviceDisplay.MainDisplayInfo.Width;
            double b = DeviceDisplay.MainDisplayInfo.Height;
            double w = Math.Min(a, b);
            double h = Math.Max(a, b);
            ScaledPortraitSize = new Size(w / scaling, h / scaling);
            ScaledLandscapeSize = new Size(ScaledPortraitSize.Height, ScaledPortraitSize.Width);
        }
    }
}
