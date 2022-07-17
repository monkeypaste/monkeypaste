using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvExtensions {
        #region Collections
        public static T GetVisualAncestor<T>(this Control control) where T : Control {
            if (control is T) {
                return control as T;
            }
            return control.GetVisualAncestors().FirstOrDefault(x => x is T) as T;
        }
        public static T GetVisualDescendant<T>(this Control control) where T : Control {
            if (control is T) {
                return control as T;
            }
            return control.GetVisualDescendants().FirstOrDefault(x => x is T) as T;
        }
        public static IEnumerable<T> GetVisualDescendants<T>(this Control control) where T : Control {
            IEnumerable<T> result;
            result = control.GetVisualDescendants().Where(x => x is T).Cast<T>();
            if (control is T ct) {
                result.Append(ct);
            }
            return result;
        }

        public static bool TryGetVisualAncestor<T>(this Control control, out T ancestor) where T : Control {
            ancestor = control.GetVisualAncestor<T>();
            return ancestor != default(T);
        }

        public static bool TryGetVisualDescendant<T>(this Control control, out T descendant) where T : Control {
            descendant = control.GetVisualDescendant<T>();
            return descendant != default(T);
        }

        public static bool TryGetVisualDescendants<T>(this Control control, out IEnumerable<T> descendant) where T : Control {
            descendant = control.GetVisualDescendants<T>();
            return descendant.Count() > 0;
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
