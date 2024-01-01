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

            double x = primaryScreen.WorkArea.Right - s.Width - pad;
#if MAC
            double y = primaryScreen.WorkArea.Top + pad;
#else
            double y = primaryScreen.WorkArea.Bottom - s.Height - pad;
#endif

            x *= primaryScreen.Scaling;
            y *= primaryScreen.Scaling;

            var time_for_this = w.OpenDateTime ?? DateTime.Now;
            double offsetY =
                MpAvWindowManager.ToastNotifications
                .Where(x => x.OpenDateTime < time_for_this)
                .Sum(x => (x.Bounds.Height + pad) * primaryScreen.Scaling);
#if MAC
            y += offsetY;
#else
            y -= offsetY;
#endif

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);
            return new PixelPoint((int)x, (int)y);
        }

        public static PixelPoint GetWindowPositionByAnchorVisual(Window nw, Visual owner_c) {
            var anchor_s_origin = owner_c.PointToScreen(new Point());
            var anchor_s_size = owner_c.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
            var nw_s_size = nw.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
            double nw_x = anchor_s_origin.X + (anchor_s_size.Width / 2) - (nw_s_size.Width / 2);
            double nw_y = anchor_s_origin.Y + (anchor_s_size.Height / 2) - (nw_s_size.Height / 2);
            if (TopLevel.GetTopLevel(owner_c) is Window owner_w &&
                owner_w.Screens.ScreenFromVisual(owner_w) is { } scr) {
                var s_size = scr.WorkingArea.Size;
                nw_x = Math.Clamp(nw_x, 0, s_size.Width - nw_s_size.Width);
                nw_y = Math.Clamp(nw_y, 0, s_size.Height - nw_s_size.Height);
            }
            return new PixelPoint((int)nw_x, (int)nw_y);
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private static Size GetWindowSize(Window w, double fallback_w = 350, double fallbach_h = 150) {
            if (w.Width > 0 && w.Height > 0) {
                double th = GetWindowTitleHeight(w);
                return new Size(w.Width, w.Height + th);
            }
            double width = w.Bounds.Width.IsNumber() && w.Bounds.Width != 0 ? w.Bounds.Width : fallback_w;
            double height = w.Bounds.Height.IsNumber() && w.Bounds.Height != 0 ? w.Bounds.Height : fallbach_h;
            if (w.DataContext is MpAvMessageNotificationViewModel mnvm) {
                width = 350;// mnvm.MessageWindowFixedWidth;
            }
            return new Size(width, height + GetWindowTitleHeight(w));
        }

        private static double GetWindowTitleHeight(Window w) {
            if (w == null) {
                return 0;
            }
            return w.FrameSize.HasValue ? w.FrameSize.Value.Height - w.ClientSize.Height : 0;
        }

        #endregion

        #region Commands
        #endregion
    }
}
