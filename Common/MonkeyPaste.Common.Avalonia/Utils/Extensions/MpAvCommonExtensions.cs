using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Avalonia.VisualExtensions;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvCommonExtensions {

        #region Collections
        public static T GetVisualAncestor<T>(this IVisual visual, bool includeSelf = true) where T : IVisual? {
            if (includeSelf && visual is T) {
                return (T)visual;
            }
            var visualResult = (T)visual.GetVisualAncestors().FirstOrDefault(x => x is T);
            //if (visualResult == null && visual is Control control && control.TemplatedParent is Control templatedParent) {
            //    return templatedParent.GetVisualAncestor<T>(includeSelf);
            //}
            return visualResult;
        }
        public static IEnumerable<T> GetVisualAncestors<T>(this IVisual visual, bool includeSelf = true) where T : IVisual {
            IEnumerable<T> visualResult;
            visualResult = visual.GetVisualAncestors().Where(x => x is T).Cast<T>();

            //if ((visualResult == null || visualResult.Count() == 0) && 
            //    visual is Control control && control.TemplatedParent is Control templatedParent) {
            //    return templatedParent.GetVisualAncestors<T>(includeSelf);
            //}
            if (includeSelf && visual is T ct) {
                visualResult.Append(ct);
            }

            return visualResult;
        }
        public static T GetVisualDescendant<T>(this IVisual control, bool includeSelf = true) where T : IVisual {
            if (includeSelf && control is T) {
                return (T)control;
            }
            return (T)control.GetVisualDescendants().FirstOrDefault(x => x is T);
        }
        public static IEnumerable<T> GetVisualDescendants<T>(this IVisual control, bool includeSelf = true) where T : IVisual {
            IEnumerable<T> result;
            result = control.GetVisualDescendants().Where(x => x is T).Cast<T>();
            if (includeSelf && control is T ct) {
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

        public static MpRect RelativeBounds(this Control control, IVisual relTo) {
            var relative_origin = control.TranslatePoint(new Point(0, 0), relTo).Value.ToPortablePoint();
            var observed_size = control.Bounds.Size.ToPortableSize();
            return new MpRect(relative_origin, observed_size);
        }

        #endregion

        #region MainWindow

        public static Window MainWindow(this Application? app) {
            if(app != null && app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                return desktop.MainWindow;
            }
            return null;
        }

        #endregion

        #region Screens

        public static double VisualPixelDensity(this IVisual visual, Window w = null) {
            if(w == null && 
                Application.Current.MainWindow() is Window) {
                w = Application.Current.MainWindow();
            }
            
            if(w == null) {
                return 1;
            }
            if(visual == null) {
                return w.Screens.Primary.PixelDensity;
            }
            var scr = w.Screens.ScreenFromVisual(visual);
            if(scr == null) {
                scr = Application.Current.MainWindow().Screens.Primary;
                if(scr == null) {
                    Debugger.Break();
                    return 1;
                }
            }
            return scr.PixelDensity;
        }

        #endregion

        #region FormattedText

        public static FormattedText ToFormattedText(
            this string text,
            string fontFamily = FontFamily.DefaultFontFamilyName,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight fontWeight = FontWeight.Normal,
            double fontSize = 12.0d,
            TextAlignment textAlignment = TextAlignment.Left,
            TextWrapping textWrapping = TextWrapping.NoWrap,
            MpSize constraint = null) {
            var ft = new FormattedText(
                    text,
                    new Typeface(fontFamily, fontStyle, fontWeight),
                    fontSize,
                    textAlignment,
                    textWrapping,
                    new Size());
            if(constraint == null) {
                // size to text
                ft.Constraint = ft.Bounds.Size;
            } else {
                ft.Constraint = constraint.ToAvSize();
            }
            return ft;
        }
        #endregion

        #region Shape Rendering

        public static IPen GetPen(this MpShape shape) {
            IPen pen = new Pen(
                shape.StrokeOctColor.ToAvBrush(),
                shape.StrokeThickness,
                new DashStyle(shape.StrokeDashStyle, shape.StrokeDashOffset),
                shape.StrokeLineCap.ToEnum<PenLineCap>(),
                shape.StrokeLineJoin.ToEnum<PenLineJoin>(),
                shape.StrokeMiterLimit);
            return pen;
        }

        public static void DrawRect(this MpRect rect, DrawingContext dc) {
            IBrush brush = rect.FillOctColor.ToAvBrush();
            IPen pen = rect.GetPen();
            BoxShadows bs = string.IsNullOrEmpty(rect.BoxShadows) ? default : BoxShadows.Parse(rect.BoxShadows);
            dc.DrawRectangle(
                    brush,
                    pen,
                    rect.ToAvRect(),
                    rect.RadiusX,
                    rect.RadiusY,
                    bs);
        }

        public static void DrawLine(this MpLine line, DrawingContext dc) {
            IPen pen = line.GetPen();
            dc.DrawLine(
                    pen,
                    line.P1.ToAvPoint(),
                    line.P2.ToAvPoint());
        }

        public static void DrawEllipse(this MpEllipse ellipse, DrawingContext dc) {
            IBrush brush = ellipse.FillOctColor.ToAvBrush();
            IPen pen = ellipse.GetPen();

            dc.DrawEllipse(
                   brush,
                   pen,
                   ellipse.Center.ToAvPoint(),
                   ellipse.Size.Width / 2,
                   ellipse.Size.Height / 2);
        }

        public static void DrawShape(this MpShape shape, DrawingContext dc) {
            if (shape is MpLine dl) {
                dl.DrawLine(dc);
            } else if (shape is MpEllipse de) {
                de.DrawEllipse(dc);
            } else if (shape is MpRect dr) {
                dr.DrawRect(dc);
            }
        }

        #endregion

        #region Adorner

        public static async Task<AdornerLayer> GetAdornerLayerAsync(this Control adornedControl, int timeout_ms = 1000) {
            // used to simplify lifecycle issues w/ visual attach and adding adorner
            Dispatcher.UIThread.VerifyAccess();

            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedControl);

            Stopwatch sw = null;
            while(adornerLayer == null) {
                if(sw == null && timeout_ms >= 0) {
                    sw = Stopwatch.StartNew();
                }
                if(sw.ElapsedMilliseconds >= timeout_ms) {
                    Debugger.Break();
                    break;
                }
                await Task.Delay(100);
                adornerLayer = AdornerLayer.GetAdornerLayer(adornedControl);
            }
            return adornerLayer;
        }

        public static async Task<bool> AddOrReplaceAdornerAsync(this Control adornedControl, Control adorner, int timeout_ms = 1000) {
            // returns false if layer not found within timeout
            Dispatcher.UIThread.VerifyAccess();
            var adornerLayer = await adornedControl.GetAdornerLayerAsync(timeout_ms);
            if(adornerLayer == null) {
                return false;
            }
            var cur_adorner = adornerLayer.Children.FirstOrDefault(x => x == adorner);
            if(cur_adorner != null) {
                // why twice?
                Debugger.Break();
                adornerLayer.Children.Remove(cur_adorner);
            }
            adornerLayer.Children.Add(adorner);
            AdornerLayer.SetAdornedElement(adorner, adornedControl);
            return true;
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
        public static FormattedText ToFormattedText(this TextBlock tb) {
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
            if(sv == null) {
                return null;

            }

            var sbl = sv.GetVisualDescendants<ScrollBar>();
            return sbl.FirstOrDefault(x => x.Orientation == orientation);

            //if (orientation == Orientation.Vertical) {
            //    //return sv.Template..FindName("PART_VerticalScrollBar", sv) as ScrollBar;

            //    var vresult = sv.FindControl<ScrollBar>("PART_VerticalScrollBar");
                
            //    return vresult;
            //}
            ////return sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
            //var hresult = sv.FindControl<ScrollBar>("PART_HorizontalScrollBar");
            //return hresult;
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
            if(sv == null) {
                return;
            }
            var hsb = sv.GetScrollBar(Orientation.Horizontal);
            var vsb = sv.GetScrollBar(Orientation.Vertical);

            var new_offset = sv.Offset.ToPortablePoint();
            if(hsb != null) {
                new_offset.X = Math.Max(0, Math.Min(sv.Offset.X + delta.X, hsb.Maximum));
            }
            if(vsb != null) {
                new_offset.Y = Math.Max(0, Math.Min(sv.Offset.Y + delta.Y, vsb.Maximum));
            }

            sv.ScrollToPoint(new_offset);
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
        public static MpPoint ToPortablePoint(this Vector v) {
            return new MpPoint(v.X, v.Y);
        }

       
        public static MpPoint ToPortablePoint(this PixelPoint p, double pixelDensity) {
            return new MpPoint(((double)p.X/pixelDensity), ((double)p.Y/pixelDensity));
        }

        public static Point ToAvPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

        public static PixelPoint ToAvPixelPoint(this MpPoint p, double pixelDensity) {
            return new PixelPoint((int)(p.X* pixelDensity), (int)(p.Y*pixelDensity));
        }

        public static MpPoint TranslatePoint(this MpPoint p, Control relativeTo = null, bool toScreen = false) {
            // NOTE when toScreen is FALSE p is assumed to be a screen point

            if (relativeTo == null) {
                relativeTo = Application.Current.MainWindow();
                if (relativeTo == null) {
                    return p;
                }
            }

            var pd = relativeTo.VisualPixelDensity();
            if(toScreen) {
                MpPoint origin = relativeTo.PointToScreen(new Point()).ToPortablePoint(pd);
                p.X = origin.X;
                p.Y = origin.Y;
                return p;
            }
            return relativeTo.PointToClient(p.ToAvPixelPoint(pd)).ToPortablePoint();
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

        public static void TranslateOrigin(this MpRect rect, Control relativeTo = null, bool toScreen = false) {
            MpPoint origin = rect.Location.TranslatePoint(relativeTo, toScreen);
            rect.X = origin.X;
            rect.Y = origin.Y;
        }

        public static MpRect ToPortableRect(this Rect rect, Control relativeTo = null, bool toScreen = false) {
            var prect = new MpRect(rect.Position.ToPortablePoint(), rect.Size.ToPortableSize());
            if(relativeTo == null) {
                return prect;
            }
            prect.TranslateOrigin(relativeTo, toScreen);
            return prect;
        }

        public static Rect ToAvRect(this MpRect rect) {
            return new Rect(rect.Location.ToAvPoint(), rect.Size.ToAvSize());
        }

        public static MpRect ToPortableRect(this PixelRect rect, double pixelDensity) {
            return new MpRect(rect.Position.ToPortablePoint(pixelDensity), rect.Size.ToPortableSize());
        }

        public static PixelRect ToAvPixelRect(this MpRect rect, double pixelDensity) {
            return new PixelRect(rect.Location.ToAvPixelPoint(pixelDensity), rect.Size.ToAvPixelSize());
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
