using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvExtensions {
        public static MpPoint ToPortablePoint(this Point p) {
            return new MpPoint(p.X, p.Y);
        }

        public static Point ToAvPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

        public static void ScrollToHorizontalOffset(this ScrollViewer sv, double xOffset) {
            var newOffset = new Vector(
                Math.Max(0, Math.Min(sv.Extent.Width, xOffset)),
                sv.Offset.Y);

            sv.Offset = newOffset;
        }

        public static void ScrollToVerticalOffset(this ScrollViewer sv, double yOffset) {
            var newOffset = new Vector(
                sv.Offset.X,
                Math.Max(0, Math.Min(sv.Extent.Height, yOffset)));

            sv.Offset = newOffset;
        }

        public static (T oldValue, T newValue) GetOldAndNewValue<T>(this AvaloniaPropertyChangedEventArgs e) {
            var ev = (AvaloniaPropertyChangedEventArgs<T>)e;
            return (ev.OldValue.GetValueOrDefault()!, ev.NewValue.GetValueOrDefault()!);
        }

    }
}
