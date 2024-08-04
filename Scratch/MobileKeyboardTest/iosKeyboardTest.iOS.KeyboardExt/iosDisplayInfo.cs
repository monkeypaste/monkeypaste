using Avalonia;
//using Microsoft.Maui.Devices;
using System;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt
{
    public static class iosDisplayInfo
    {
        public static Size ScaledPortraitSize { get; private set; }
        public static Size ScaledLandscapeSize { get; private set; }
        public static Size ScaledSize =>
            IsPortrait ?
                ScaledPortraitSize : ScaledLandscapeSize;
        public static double Scaling =>
            //DeviceDisplay.MainDisplayInfo.Density;
            UIScreen.MainScreen.Scale;

        public static bool IsPortrait =>
            //DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Portrait
            UIScreen.MainScreen.Bounds.Width < UIScreen.MainScreen.Bounds.Height;
        static iosDisplayInfo()
        {
            
            //double scaling = DeviceDisplay.MainDisplayInfo.Density;
            //double a = DeviceDisplay.MainDisplayInfo.Width;
            //double b = DeviceDisplay.MainDisplayInfo.Height;
            //double w = Math.Min(a, b);
            //double h = Math.Max(a, b);
            //ScaledPortraitSize = new Size(w / scaling, h / scaling);
            //ScaledLandscapeSize = new Size(ScaledPortraitSize.Height, ScaledPortraitSize.Width);
            
            double scaling = (double)UIScreen.MainScreen.Scale;
            double a = (double)UIScreen.MainScreen.Bounds.Width;
            double b = (double)UIScreen.MainScreen.Bounds.Height;
            double w = Math.Min(a, b);
            double h = Math.Max(a, b);
            ScaledPortraitSize = new Size(w / scaling, h / scaling);
            ScaledLandscapeSize = new Size(ScaledPortraitSize.Height, ScaledPortraitSize.Width);
        }
    }
}
