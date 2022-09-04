using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;

using static Avalonia.VisualExtensions;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvExtensions {
        #region Collections
        public static T GetVisualAncestor<T>(this IVisual control) where T : IVisual? {
            if (control is T) {
                return (T)control;
            }
            return (T)control.GetVisualAncestors().FirstOrDefault(x => x is T);
        }
        public static IEnumerable<T> GetVisualAncestors<T>(this IVisual control) where T : IVisual {
            IEnumerable<T> result;
            result = control.GetVisualAncestors().Where(x => x is T).Cast<T>();
            if (control is T ct) {
                result.Append(ct);
            }
            return result;
        }
        public static T GetVisualDescendant<T>(this IVisual control) where T : IVisual {
            if (control is T) {
                return (T)control;
            }
            return (T)control.GetVisualDescendants().FirstOrDefault(x => x is T);
        }
        public static IEnumerable<T> GetVisualDescendants<T>(this IVisual control) where T : IVisual {
            IEnumerable<T> result;
            result = control.GetVisualDescendants().Where(x => x is T).Cast<T>();
            if (control is T ct) {
                result.Append(ct);
            }
            return result;
        }

        public static bool TryGetVisualAncestor<T>(this IVisual control, out T ancestor) where T : IVisual {
            ancestor = control.GetVisualAncestor<T>();
            return ancestor != null;
        }

        public static bool TryGetVisualDescendant<T>(this IVisual control, out T descendant) where T : IVisual {
            descendant = control.GetVisualDescendant<T>();
            return descendant != null;
        }

        public static bool TryGetVisualDescendants<T>(this IVisual control, out IEnumerable<T> descendant) where T : IVisual {
            descendant = control.GetVisualDescendants<T>();
            return descendant.Count() > 0;
        }
        #endregion

        #region Control
        public static void InvalidateAll(this Control control) {
            control?.InvalidateArrange();
            control?.InvalidateMeasure();
            control?.InvalidateVisual();            
        }

        #endregion

        #region TextBox

        public static FormattedText ToFormattedText(this TextBox tb) {
            var ft = new FormattedText(
                    tb.Text,
                    new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight),
                    tb.FontSize,
                    tb.TextAlignment,
                    tb.TextWrapping,
                    new Size());
            return ft;
        }


        #endregion

        #region Scroll Viewer
        public static ScrollBar GetScrollBar(this ScrollViewer sv, Orientation orientation) {
            if (orientation == Orientation.Vertical) {
                //return sv.Template..FindName("PART_VerticalScrollBar", sv) as ScrollBar;

                var vresult = sv.FindControl<ScrollBar>("PART_VerticalScrollBar");
                return vresult;
            }
            //return sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
            var hresult = sv.FindControl<ScrollBar>("PART_HorizontalScrollBar");
            return hresult;
        }

        public static void ScrollToHorizontalOffset(this ScrollViewer sv, double xOffset) {
            var newOffset = new Vector(
                xOffset,//Math.Max(0, Math.Min(sv.Extent.Width, xOffset)),
                sv.Offset.Y);

            sv.Offset = newOffset;
        }

        public static void ScrollToVerticalOffset(this ScrollViewer sv, double yOffset) {
            var newOffset = new Vector(
                sv.Offset.X,
                yOffset);//Math.Max(0, Math.Min(sv.Extent.Height, yOffset)));

            sv.Offset = newOffset;
        }

        public static void ScrollByPointDelta(this ScrollViewer sv, MpPoint delta) {
            var hsb = sv.GetScrollBar(Orientation.Horizontal);
            var vsb = sv.GetScrollBar(Orientation.Vertical);

            double new_x_offset = Math.Max(0, Math.Min(sv.Offset.X + delta.X, hsb.Maximum));
            double new_y_offset = Math.Max(0, Math.Min(sv.Offset.Y + delta.Y, vsb.Maximum));

            sv.ScrollToPoint(new MpPoint(new_x_offset, new_y_offset));
        }

        public static void ScrollToPoint(this ScrollViewer sv, MpPoint p) {
            sv.ScrollToHorizontalOffset(p.X);
            sv.ScrollToVerticalOffset(p.Y);

            sv.InvalidateMeasure();
            sv.InvalidateArrange();
        }
        #endregion

        #region Events

        public static bool IsLeftPress(this PointerPressedEventArgs ppea, IVisual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed;
        }

        public static bool IsRightPress(this PointerPressedEventArgs ppea, IVisual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed;
        }

        public static bool IsLeftRelease(this PointerReleasedEventArgs ppea, IVisual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed;
        }

        public static bool IsRightRelease(this PointerReleasedEventArgs ppea, IVisual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed;
        }

        public static bool IsLeftDown(this PointerEventArgs e, IVisual? control) {
            return e.GetCurrentPoint(control).Properties.IsLeftButtonPressed;
        }

        public static MpPoint GetClientMousePoint(this PointerEventArgs e, IVisual? control) {
            return e.GetPosition(control).ToPortablePoint();
        }

        #endregion

        #region PropertyChanged

        public static (T oldValue, T newValue) GetOldAndNewValue<T>(this AvaloniaPropertyChangedEventArgs e) {
            var ev = (AvaloniaPropertyChangedEventArgs<T>)e;
            return (ev.OldValue.GetValueOrDefault()!, ev.NewValue.GetValueOrDefault()!);
        }

        #endregion

        #region Point

        public static MpPoint ToPortablePoint(this MpSize size) {
            return new MpPoint(size.Width, size.Height);
        }
        public static MpPoint ToPortablePoint(this Point p) {
            return new MpPoint(p.X, p.Y);
        }

        public static MpPoint ToPortablePoint(this PixelPoint p) {
            return new MpPoint(p.X, p.Y);
        }

        public static Point ToAvPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

        public static PixelPoint ToAvPixelPoint(this MpPoint p) {
            return new PixelPoint((int)p.X, (int)p.Y);
        }

        public static PixelPoint ToAvPixelPoint(this PixelSize p) {
            return new PixelPoint(p.Width, p.Height);
        }

        public static Point ToAvPoint(this PixelPoint p) {
            return new Point(p.X, p.Y);
        }

        public static PixelPoint ToAvPixelPoint(this Point p) {
            return new PixelPoint((int)p.X, (int)p.Y);
        }

        #endregion

        #region Vector
        public static Vector ToAvVector(this MpPoint p) {
            return new Vector(p.X, p.Y);
        }

        #endregion

        #region Size

        public static MpSize ToPortableSize(this MpPoint p) {
            return new MpSize(p.X, p.Y);
        }

        public static MpSize ToPortableSize(this Size size) {
            return new MpSize(size.Width, size.Height);
        }

        public static Size ToAvSize(this MpSize size) {
            return new Size(size.Width, size.Height);
        }

        public static Size ToAvSize(this PixelSize size) {
            return new Size(size.Width, size.Height);
        }



        public static MpSize ToPortableSize(this PixelSize size) {
            return new MpSize(size.Width, size.Height);
        }

        public static PixelSize ToAvPixelSize(this MpSize size) {
            return new PixelSize((int)size.Width, (int)size.Height);
        }

        public static PixelSize ToAvPixelSize(this Size size) {
            return new PixelSize((int)size.Width, (int)size.Height);
        }

        public static PixelSize ToAvPixelSize(this Point point) {
            return new PixelSize((int)point.X, (int)point.Y);
        }

        public static PixelSize ToAvPixelSize(this PixelPoint point) {
            return new PixelSize(point.X, point.Y);
        }
        #endregion

        #region Rect

        public static MpRect ToPortableRect(this Rect rect) {
            return new MpRect(rect.Position.ToPortablePoint(), rect.Size.ToPortableSize());
        }

        public static Rect ToAvRect(this MpRect rect) {
            return new Rect(rect.Location.ToAvPoint(), rect.Size.ToAvSize());
        }

        public static MpRect ToPortableRect(this PixelRect rect) {
            return new MpRect(rect.Position.ToPortablePoint(), rect.Size.ToPortableSize());
        }

        public static PixelRect ToAvPixelRect(this MpRect rect) {
            return new PixelRect(rect.Location.ToAvPixelPoint(), rect.Size.ToAvPixelSize());
        }



        #endregion

        #region Color


        #endregion

        #region Strings

        public static bool IsAvResourceString(this string str) {
            if(str.IsNullOrEmpty()) {
                return false;
            }
            return str.ToLower().StartsWith("avares://");
        }
        #endregion
    }
}
