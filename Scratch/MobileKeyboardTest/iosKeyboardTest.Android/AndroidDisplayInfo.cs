
using Android.Content.Res;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Devices;
using System;
using Size = Avalonia.Size;

namespace iosKeyboardTest.Android
{
    public static class AndroidDisplayInfo
    {
        public static Size UnscaledPortraitSize { get; private set; }
        public static Size UnscaledLandscapeSize { get; private set; }
        public static Size UnscaledSize =>
            IsPortrait ?
                UnscaledPortraitSize : UnscaledLandscapeSize;
        public static Size ScaledPortraitSize { get; private set; }
        public static Size ScaledLandscapeSize { get; private set; }
        public static Size ScaledSize =>
            IsPortrait ?
                ScaledPortraitSize : ScaledLandscapeSize;
        public static double Scaling { get; private set; }
        public static bool IsPortrait { get; private set; }
            
        static AndroidDisplayInfo()
        {
            Scaling = DeviceDisplay.MainDisplayInfo.Density;
            SetProps(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
        }
        public static void Init(Window w, bool isPortrait) {
            Rect? disp_frame = new();
            w.DecorView.GetWindowVisibleDisplayFrame(disp_frame);
            IsPortrait = isPortrait;
            SetProps((double)disp_frame.Width(), (double)disp_frame.Height());
        }

        static void SetProps(double a, double b) {
            double w = Math.Min(a, b);
            double h = Math.Max(a, b);
            UnscaledPortraitSize = new Size(w, h);
            UnscaledLandscapeSize = new Size(h, w);

            ScaledPortraitSize = new Size(w / Scaling, h / Scaling);
            ScaledLandscapeSize = new Size(ScaledPortraitSize.Height, ScaledPortraitSize.Width);
        }
    }
}
