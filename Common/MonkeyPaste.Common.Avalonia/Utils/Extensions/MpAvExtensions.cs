using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvExtensions {
        public static T GetVisualAncestor<T>(this Control control) where T : Control {
            if(control is T) {
                return control as T;
            }
            return control.GetVisualAncestors().FirstOrDefault(x => x is T) as T;
        }
        public static T GetVisualDescendant<T>(this Control control) where T:Control {
            if(control is T) {
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

        public static bool TryGetVisualDescendant<T>(this Control control, out T descendant) where T: Control {
            descendant = control.GetVisualDescendant<T>();
            return descendant != default(T);
        }

        public static bool TryGetVisualDescendants<T>(this Control control, out IEnumerable<T> descendant) where T : Control {
            descendant = control.GetVisualDescendants<T>();
            return descendant.Count() > 0;
        }
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
