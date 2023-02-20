using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvBadgeNotificationAdorner : MpAvAdornerBase {
        #region Private Variables

        private MpIBadgeNotifier _notifier;

        #endregion

        #region Properties
        public int NotificationCount {
            get {
                if (_notifier == null) {
                    return 0;
                }
                return _notifier.NotificationCount;
            }
        }

        #endregion

        #region Public Methods
        public MpAvBadgeNotificationAdorner(Control control) : base(control) {
            //ClipToBounds = false;
            IsVisible = false;
            if (control != null && control.DataContext is MpIBadgeNotifier) {
                _notifier = control.DataContext as MpIBadgeNotifier;
            }
        }
        #endregion

        #region Overrides


        public override void Render(DrawingContext context) {
            if (AdornedControl == null) {
                return;
            }
            IsVisible = NotificationCount > 0;

            if (!IsVisible) {
                return;
            }
            // DRAW CIRCLE

            double r = 5;
            double lw = 1;
            var b = AdornedControl.Bounds.ToPortableRect();
            MpPoint ellipse_center = b.TopRight;
            ellipse_center.Y = b.Centroid().Y;
            ellipse_center.X -= (r * 2) + 1;
            context.DrawEllipse(Brushes.Red, new Pen(Brushes.White, lw), ellipse_center.ToAvPoint(), r, r);

            // DRAW COUNT

            double fs = Math.Max(10, (r * 2) - (lw * 2) - 1);
            // for more than 9 notifications just draw asterisk
            string count_text = NotificationCount > 9 ? "*" : NotificationCount.ToString();
            FormattedText count_ft = count_text.ToFormattedText(
                fontFamily: "Verdana",
                textAlignment: TextAlignment.Left,
                fontSize: fs,
                foreground: Brushes.OldLace);
            MpPoint text_origin = ellipse_center;
            text_origin.X -= (r / 2);
            text_origin.Y = -(r / 2) + 1;
            context.DrawText(count_ft, text_origin.ToAvPoint());

            base.Render(context);
        }
        #endregion
    }
}
