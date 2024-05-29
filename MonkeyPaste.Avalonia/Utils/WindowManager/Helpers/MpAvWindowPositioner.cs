using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvWindowPositioner {
        #region Private Variables
        #endregion

        #region Statics

        public static PixelPoint GetSystemTrayWindowPosition(MpAvWindow w, double pad = 10) {
            Size s = GetWindowSize(w);
            // NOTE this should account for mw show behavior (i think) show 'system tray' is BR of active monitor
            // TODO test when other window behaviors are implemented
            var primaryScreen = w.ScreenInfo;
            if (primaryScreen == null) {
                // happens before loader attached
                return new PixelPoint();
            }

            double x = primaryScreen.WorkingArea.Right - s.Width - pad;
#if MAC || LINUX || MOBILE_OR_WINDOWED
            double y = primaryScreen.WorkingArea.Top + pad;
#else
            double y = primaryScreen.WorkingArea.Bottom - s.Height - pad;
#endif

            x *= primaryScreen.Scaling;
            y *= primaryScreen.Scaling;

            var time_for_this = w.OpenDateTime ?? DateTime.Now;
            double offsetY =
                MpAvWindowManager.ToastNotifications
                .Where(x => x.OpenDateTime < time_for_this && x.WindowState != WindowState.Minimized)
                .Sum(x => (GetWindowSize(x).Height + pad) * primaryScreen.Scaling);
#if MAC || LINUX || MOBILE_OR_WINDOWED
            y += offsetY;
#else
            y -= offsetY;
#endif

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);
            return new PixelPoint((int)x, (int)y);
        }

        public static PixelPoint GetWindowPositionByAnchorVisual(MpAvWindow nw, Visual owner_c) {
            var anchor_s_origin = owner_c.PointToScreen(new Point());
            var anchor_s_size = owner_c.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
            var nw_s_size = nw.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
            double nw_x = anchor_s_origin.X + (anchor_s_size.Width / 2) - (nw_s_size.Width / 2);
            double nw_y = anchor_s_origin.Y + (anchor_s_size.Height / 2) - (nw_s_size.Height / 2);

            if (MpAvWindowManager.GetTopLevel(owner_c) is MpAvWindow owner_w &&
                owner_w.Screens.ScreenFromVisual(owner_w) is { } scr) {
                var s_size = scr.WorkingArea.Size;
                nw_x = Math.Clamp(nw_x, 0, s_size.Width - nw_s_size.Width);
                nw_y = Math.Clamp(nw_y, 0, s_size.Height - nw_s_size.Height);
            }
            PixelPoint pos = new PixelPoint((int)nw_x, (int)nw_y);
            if (pos.X == 0 && owner_c is not MpAvWindow &&
                MpAvWindowManager.GetTopLevel(owner_c) is MpAvWindow owner_w2) {
                // BUG sometimes ntf ends up in bottom corner of screen
                // NOTE ensuring this doesn't get stuck in a loop by checking if owner is window

                // fallback and center to owner window
                return GetWindowPositionByAnchorVisual(nw, owner_w2);
            }
            return pos;
        }

        public static double GetWindowTitleHeight(MpAvWindow w) {
            if (w == null) {
                return 0;
            }
            return w.FrameSize.HasValue ? w.FrameSize.Value.Height - w.ClientSize.Height : 0;
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private static Size GetWindowSize(MpAvWindow w, double fallback_w = 350, double fallbach_h = 150) {
            double th = GetWindowTitleHeight(w);
            //MpConsole.WriteLine($"Window title height: {th}px");
            if (w.Width > 0 && w.Height > 0) {
                return new Size(w.Width, w.Height + th);
            }
            double width = w.Bounds.Width.IsNumber() && w.Bounds.Width != 0 ? w.Bounds.Width : fallback_w;
            double height = w.Bounds.Height.IsNumber() && w.Bounds.Height != 0 ? w.Bounds.Height : fallbach_h;
            if (w.DataContext is MpAvUserActionNotificationViewModel mnvm) {
                width = 350;// mnvm.MessageWindowFixedWidth;
            }
#if MOBILE_OR_WINDOWED
            if(Mp.Services != null && 
                Mp.Services.ScreenInfoCollection != null &&
                Mp.Services.ScreenInfoCollection.Primary != null) {
                th = 0;
                width = Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width;
            }
#endif
            return new Size(width, height + th);
        }


        #endregion

        #region Commands
        #endregion
    }
}
