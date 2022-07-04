using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Win32.Interop;
using MonkeyPaste.Common;
using SharpHook;

namespace MonkeyPaste.Avalonia {
    public interface MpIGlobalMouseEvents {
        MpPoint GlobalMouseLocation { get; }

    }
    public static class MpAvGlobalMouseHook {

        public static MpPoint GlobalMouseLocation { get; private set; } = new MpPoint();

        public static double PixelScaling { get; set; } = 1.0d;

        public static event EventHandler<double>? OnGlobalMouseWheelScroll;
        public static event EventHandler<MpPoint>? OnGlobalMouseMove;

        public static void Init(double pixelScaling) {
            PixelScaling = pixelScaling;
            var hook = new SimpleGlobalHook();
            hook.MouseWheel += Hook_MouseWheel;
            hook.MouseMoved += Hook_MouseMoved;
            hook.RunAsync();
        }
        private static void Hook_MouseMoved(object? sender, MouseHookEventArgs e) {
            GlobalMouseLocation = new MpPoint(Math.Max(0,(double)e.Data.X / PixelScaling), Math.Max(0,(double)e.Data.Y / PixelScaling));
            OnGlobalMouseMove?.Invoke(typeof(MpAvGlobalMouseHook).ToString(), GlobalMouseLocation);
        }

        private static void Hook_MouseWheel(object? sender, MouseWheelHookEventArgs e) {
            OnGlobalMouseWheelScroll?.Invoke(typeof(MpAvGlobalMouseHook).ToString(), (double)e.Data.Amount);
        }
    }

}
