using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Avalonia.VisualExtensions;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvCommonExtensions {
        #region Storage

        public static async Task<IStorageItem[]> ToAvFilesObjectAsync(this IEnumerable<string> fpl) {
            var files = await Task.WhenAll(fpl.Where(x => x.IsFileOrDirectory()).Select(x => x.ToFileOrFolderStorageItemAsync()));
            return files.ToArray();
        }
        public static async Task<IStorageItem> ToFileOrFolderStorageItemAsync(this string path) {
            if (!path.IsFileOrDirectory()) {
                return null;
            }
            IStorageItem si = null;
            var mw = Application.Current.GetMainTopLevel();
            var storageProvider = TopLevel.GetTopLevel(mw)!.StorageProvider;
            if (storageProvider != null) {
                if (path.IsFile()) {
                    si = await storageProvider.TryGetFileFromPathAsync(path);
                } else {
                    si = await storageProvider.TryGetFolderFromPathAsync(path);
                }
            }
            return si;
        }
        #endregion

        #region Environment

        public static Screen AsScreen(this IRenderRoot rr) {
            return new Screen(
                            1,
                            new PixelRect(rr.ClientSize.ToAvPixelSize(1)),
                            new PixelRect(rr.ClientSize.ToAvPixelSize(1)),
                            true);
        }

        #endregion

        #region Focus

        public static IInputElement GetFocusableAncestor(this Visual visual, bool includeSelf = true) {
            if (visual == null) {
                return null;
            }
            if (includeSelf && visual is IInputElement ie) {
                return ie;
            }
            return visual
                .GetVisualAncestors()
                .Where(x => x is IInputElement)
                .Cast<IInputElement>()
                .FirstOrDefault(x => x.Focusable);
        }
        public static IInputElement GetFocusableDescendant(this Visual visual, bool includeSelf = true) {
            if (visual == null) {
                return null;
            }
            if (includeSelf && visual is IInputElement ie) {
                return ie;
            }
            return visual
                .GetVisualDescendants()
                .Where(x => x is IInputElement)
                .Cast<IInputElement>()
                .FirstOrDefault(x => x.Focusable);
        }

        public static async Task<bool> TryKillFocusAsync(this Control control) {
            if (control == null) {
                return false;
            }
            if (control.GetFocusableAncestor(false) is IInputElement ie &&
                ie != null) {
                bool success = await ie.TrySetFocusAsync();
                return success;
            }
            return false;
        }

        public static async Task<bool> TrySetFocusAsync(this IInputElement ie, NavigationMethod focusMethod = NavigationMethod.Unspecified, int time_out_ms = 1000) {
            if (ie == null) {
                return false;
            }
            var sw = Stopwatch.StartNew();
            while (true) {
                if (sw.ElapsedMilliseconds >= time_out_ms) {
                    break;
                }
                if (ie.IsFocused) {
                    return true;
                }
                ie.Focus(focusMethod);
                await Task.Delay(100);
            }
            return false;
        }

        #endregion

        #region Visual Tree

        public static Visual FindVisualDescendantWithHashCode(this Visual visual, int hashCode, bool printInfo = false) {
            var target = visual.GetSelfAndVisualDescendants().FirstOrDefault(x => x.GetHashCode() == hashCode);
            if (target != null && printInfo) {
                var control_up = target.GetVisualAncestors();
                MpConsole.WriteLine("UP:");
                control_up.ForEach(x => MpConsole.WriteLine($"Type: '{x.GetType()}' Name: '{x.Name}'"));

                var control_down = target.GetVisualDescendants();
                MpConsole.WriteLine("DOWN:");
                control_down.ForEach(x => MpConsole.WriteLine($"Type: '{x.GetType()}' Name: '{x.Name}'"));
            }
            return target;
        }

        public static ILogical FindLogicalDescendantWithHashCode(this ILogical logical, int hashCode, bool printInfo = false) {
            var logical_tree = logical.GetSelfAndLogicalDescendants();
            var target = logical_tree.FirstOrDefault(x => x.GetHashCode() == hashCode);
            if (target != null && printInfo) {
                var control_up = target.GetLogicalAncestors();
                MpConsole.WriteLine("UP:");
                control_up.ForEach(x => MpConsole.WriteLine($"Type: '{x.GetType()}' Name: '{(x is Visual v && !string.IsNullOrEmpty(v.Name) ? v.Name : x.ToString())}'"));

                var control_down = target.GetLogicalAncestors();
                MpConsole.WriteLine("DOWN:");
                control_down.ForEach(x => MpConsole.WriteLine($"Type: '{x.GetType()}' Name: '{(x is Visual v && !string.IsNullOrEmpty(v.Name) ? v.Name : x.ToString())}'"));
            }
            return target;
        }

        public static async Task<T> GetVisualDescendantAsync<T>(this Visual visual, bool includeSelf = true, int timeOutMs = 3_000) where T : Visual? {
            T result = visual.GetVisualDescendant<T>(includeSelf);
            if (result != null) {
                return result;
            }
            var sw = Stopwatch.StartNew();
            while (result == default) {
                result = visual.GetVisualDescendant<T>(includeSelf);
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds >= timeOutMs) {
                    MpConsole.WriteLine($"GetVisualDescendantAsync from {visual.GetType()} to {typeof(T)} timeout {timeOutMs} reached.");
                    return null;
                }
            }
            return result;
        }

        public static async Task<T> GetVisualAncestorAsync<T>(this Visual visual, bool includeSelf = true, int timeOutMs = 3_000) where T : Visual? {
            T result = visual.GetVisualAncestor<T>(includeSelf);
            if (result != null) {
                return result;
            }
            var sw = Stopwatch.StartNew();
            while (result == default) {
                result = visual.GetVisualAncestor<T>(includeSelf);
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds >= timeOutMs) {
                    MpConsole.WriteLine($"GetVisualAncestorAsync from {visual.GetType()} to {typeof(T)} timeout {timeOutMs} reached.");
                    return null;
                }
            }
            return result;
        }
        public static T GetVisualAncestor<T>(this Visual visual, bool includeSelf = true) where T : Visual? {
            if (includeSelf && visual is T) {
                return (T)visual;
            }
            var visualResult = (T)visual.GetVisualAncestors().FirstOrDefault(x => x is T);
            return visualResult;
        }
        public static IEnumerable<T> GetVisualAncestors<T>(this Visual visual, bool includeSelf = true) where T : Visual {
            IEnumerable<T> visualResult = visual.GetVisualAncestors().Where(x => x is T).Cast<T>();
            if (includeSelf && visual is T ct) {
                visualResult.Append(ct);
            }

            return visualResult;
        }
        public static T GetVisualDescendant<T>(this Visual control, bool includeSelf = true) where T : Visual {
            if (includeSelf && control is T) {
                return (T)control;
            }
            return (T)control.GetVisualDescendants().FirstOrDefault(x => x is T);
        }
        public static IEnumerable<T> GetVisualDescendants<T>(this Visual control, bool includeSelf = true) where T : Visual {
            IEnumerable<T> result = control.GetVisualDescendants().OfType<T>();
            if (includeSelf && control is T ct) {
                result.Prepend(ct);
            }
            return result;
        }
        public static IEnumerable<T> GetLogicalDescendants<T>(this ILogical logical, bool includeSelf = true) where T : ILogical {
            IEnumerable<T> result = logical.GetLogicalDescendants().OfType<T>();
            if (includeSelf && logical is T t) {
                result.Prepend(t);
            }
            return result;
        }
        public static bool TryGetVisualAncestor<T>(this Visual control, out T ancestor) where T : Visual {
            ancestor = control.GetVisualAncestor<T>();
            return ancestor != null;
        }

        public static bool TryGetVisualDescendant<T>(this Visual control, out T descendant) where T : Visual {
            descendant = control.GetVisualDescendant<T>();
            return descendant != null;
        }

        public static bool TryGetVisualDescendants<T>(this Visual control, out IEnumerable<T> descendant) where T : Visual {
            descendant = control.GetVisualDescendants<T>();
            return descendant.Count() > 0;
        }

        #endregion

        #region Control
        public static RenderTargetBitmap RenderToBitmap(this Control target) {
            if (target is TemplatedControl tc) {
                MpDebug.Assert(tc.Background != null, $"Needs bg");
                if (tc.Background is SolidColorBrush scb) {
                    MpDebug.Assert(scb.Opacity >= 1, $"Needs solid bg");
                }
            }
            var pixelSize = new PixelSize((int)target.Width, (int)target.Height);
            var size = new Size(target.Width, target.Height);
            using (RenderTargetBitmap bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96))) {
                target.Measure(size);
                target.Arrange(new Rect(size));
                bitmap.Render(target);
                return bitmap;
            }
        }
        public static void RenderToFile(this Control target, string path) {
            if (target.RenderToBitmap() is RenderTargetBitmap rtb) {
                rtb.Save(path);
                rtb.Dispose();
            }
        }
        public static void ReloadDataContext(this Control c) {
            // this is useful to reapply template 
            if (c == null || c.DataContext == null) {
                // nothing to reload
                return;
            }
            object temp = c.DataContext;
            c.DataContext = null;
            c.DataContext = temp;
        }
        public static void RefreshTopLevel(this Control control) {
            if (TopLevel.GetTopLevel(control) is TopLevel tl) {
                // HACK i think this a preview7 bug but items collections don't update
                // right but triggering a window resize fixes it so increasing then decreasing slightly
                //double tickle_dist = 1.003;
                //tl.Width += tickle_dist;
                //tl.Width -= tickle_dist;
                tl?.InvalidateAll();
            }
        }

        public static void AnimateSize(this Control control, MpSize new_size, Func<bool> onComplete = null) {
            double zeta, omega, fps;

            double cw = control.Bounds.Width;
            double ch = control.Bounds.Height;
            double nw = new_size.Width;
            double nh = new_size.Height;

            if (!control.Width.IsNumber()) {
                control.Width = cw;
            }
            if (!control.Height.IsNumber()) {
                control.Height = ch;
            }

            if (nw > cw || nh > ch) {
                zeta = 0.5d;
                omega = 30.0d;
                fps = 40.0d;
            } else {
                zeta = 1.0d;
                omega = 30.0d;
                fps = 40.0d;
            }

            int delay_ms = (int)(1000 / fps);
            double dw = nw - cw;
            double dh = nh - ch;
            double step_w = dw / delay_ms;
            double step_h = dh / delay_ms;
            double vx = 0;
            double vy = 0;
            Dispatcher.UIThread.Post(async () => {
                while (true) {
                    MpAnimationHelpers.Spring(ref cw, ref vx, nw, delay_ms / 1000.0d, zeta, omega);
                    MpAnimationHelpers.Spring(ref ch, ref vy, nh, delay_ms / 1000.0d, zeta, omega);
                    control.Width = cw;
                    control.Height = ch;

                    await Task.Delay(delay_ms);

                    bool is_v_zero = Math.Abs(vx) < 0.1d;
                    if (is_v_zero) {
                        break;
                    }
                }
                control.Width = nw;
                control.Height = nh;

                onComplete?.Invoke();
            });
        }

        public static void InvalidateAll(this Control control) {
            control?.InvalidateArrange();
            control?.InvalidateMeasure();
            control?.InvalidateVisual();
        }

        public static MpRect RelativeBounds(this Control control, Visual relTo) {
            var relative_origin = control.TranslatePoint(new Point(0, 0), relTo).Value.ToPortablePoint();
            var observed_size = control.Bounds.Size.ToPortableSize();
            return new MpRect(relative_origin, observed_size);
        }

        public static void BringSelfAndAllAncestorsIntoView(this Control control) {
            if (control == null) {
                return;
            }
            control
                .GetVisualAncestors()
                .OfType<Control>()
                .Reverse()
                .ForEach(x => x.BringIntoView());
        }

        public static bool HasVisibleBackground(this Control c) {
            if (c == null ||
                c.GetBackground() is not IBrush bg ||
                bg.Opacity == 0 ||
                Brushes.Transparent.Equals(bg) ||
                !c.IsVisible) {
                return false;
            }
            return true;
        }
        public static IBrush GetBackground(this Control control) {
            if (control is TemplatedControl tc) {
                return tc.Background;
            }
            if (control is Border b) {
                return b.Background;
            }
            return null;
        }
        public static IBrush GetEffectiveBackground(this Control control, out Control backgroundControl) {
            backgroundControl = null;
            Control cur_control = control;
            while (cur_control != null) {
                if (cur_control.HasVisibleBackground()) {
                    // NOTE ignore transparent backgrounds
                    backgroundControl = cur_control;
                    return cur_control.GetBackground();
                }
                if (cur_control.Parent is not Control pc) {
                    return null;
                }
                cur_control = pc;
            }
            return null;
        }
        #endregion

        #region Text Box

        public static int SelectionLength(this TextBox tb) {
            return tb.LiteralSelectionEnd() - tb.LiteralSelectionStart();
        }

        public static int LiteralSelectionStart(this TextBox tb) {
            return Math.Min(tb.SelectionEnd, tb.SelectionStart);
        }
        public static int LiteralSelectionEnd(this TextBox tb) {
            return Math.Max(tb.SelectionEnd, tb.SelectionStart);
        }


        public static bool IsPointInTextBoxSelection(this TextBox tb, Point p) {
            int mp_tb_idx = tb.GetTextIndexFromTextBoxPoint(p);
            if (mp_tb_idx < tb.LiteralSelectionStart() ||
                mp_tb_idx > tb.LiteralSelectionEnd()) {
                // press not over selection
                return false;
            }
            return true;
        }
        #endregion

        #region Window

        public static MpRect ScaledScreenRect(this Window w) {
            if (w == null) {
                return MpRect.Empty;
            }

            MpPoint origin = w.PointToScreen(new Point()).ToPortablePoint(w.VisualPixelDensity());
            MpSize size = w.PointToScreen(w.Bounds.BottomRight).ToPortablePoint(w.VisualPixelDensity()).ToPortableSize();
            return new MpRect(origin, size);
        }
        #endregion

        #region MainWindow

        public static TopLevel GetMainTopLevel(this Application? app) {
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdsal) {
                return cdsal.MainWindow;
            }
            if (app.ApplicationLifetime is ISingleViewApplicationLifetime sval &&
                TopLevel.GetTopLevel(sval.MainView) is TopLevel tl) {
                return tl;
            }
            return null;
        }
        public static Window SetMainWindow(this Application? app, Window w) {
            // return old MainWindow
            Window last_main_window = app.GetMainTopLevel() as Window;
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdsal) {
                cdsal.MainWindow = w;
            }
            if (app.ApplicationLifetime is ISingleViewApplicationLifetime sval &&
                sval.MainView != null) {
                MpDebug.Break("dunno how to deal w/ window management yet");
            }
            return last_main_window;
        }

        public static IntPtr GetMainWindowHandle(this Application? app) {
            if (app.GetMainTopLevel() is Window w &&
                w.TryGetPlatformHandle() is IPlatformHandle ph) {
                return ph.Handle;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region Screens

        public static Screen GetScreen(this Visual v) {
            if (TopLevel.GetTopLevel(v) is not WindowBase tl) {
                return null;
            }
            return tl.Screens.ScreenFromVisual(v);
        }
        public static double VisualPixelDensity(this Visual visual, Window w = null) {

            if (w == null &&
                TopLevel.GetTopLevel(visual) is Window tlw) {
                //Application.Current.GetMainTopLevel() is Window mw) {
                w = tlw;
            }

            if (w == null) {
                return 1;
            }
            if (visual == null) {
                //return w.Screens.Primary.PixelDensity;
                return w.Screens.Primary.Scaling;
            }
            var scr = w.Screens.ScreenFromVisual(visual);
            if (scr == null && Application.Current.GetMainTopLevel() is Window mw2) {
                scr = mw2.Screens.Primary;
                if (scr == null) {
                    if (mw2.Screens.All.FirstOrDefault() is { } mw3) {
                        scr = mw3;
                    } else {
                        MpDebug.Break();
                        return 1;
                    }
                }
            }
            //return scr.PixelDensity;
            return scr.Scaling;
        }

        #endregion

        #region FormattedText

        public static FormattedText ToFormattedText(
            this string text,
            double fontSize = 12.0d,
            string fontFamily = FontFamily.DefaultFontFamilyName,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight fontWeight = FontWeight.Normal,
            TextAlignment textAlignment = TextAlignment.Left,
            TextWrapping textWrapping = TextWrapping.NoWrap,
            FlowDirection flowDirection = FlowDirection.LeftToRight,
            IBrush foreground = null,
            MpSize constraint = null) {
            foreground = foreground ?? Brushes.Black;
            var ft = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    flowDirection,
                    new Typeface(fontFamily, fontStyle, fontWeight),
                    Math.Max(1, fontSize),
                    foreground);
            return ft;
        }
        public static Typeface ToTypeface(this TextBox tb) {
            return new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
        }

        public static FormattedText ToFormattedText(this TextBox tb, bool inheritBounds = false) {
            var ft = new FormattedText(
                    tb.Text ?? string.Empty,
                    CultureInfo.CurrentCulture,
                    tb.FlowDirection,
                    tb.ToTypeface(),
                    Math.Max(1, tb.FontSize),
                    tb.Foreground);
            ft.TextAlignment = tb.TextAlignment;
            if (inheritBounds) {
                SetBoundConstraints(ft, tb.Bounds, tb.FontSize);
            }
            return ft;
        }
        public static FormattedText ToFormattedText(this TextBlock tb, bool inheritBounds = false) {
            var ft = new FormattedText(
                    tb.Text ?? string.Empty,
                    CultureInfo.CurrentCulture,
                    tb.FlowDirection,
                    new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                    Math.Max(1, tb.FontSize),
                    tb.Foreground);
            ft.TextAlignment = tb.TextAlignment;
            if (inheritBounds) {
                SetBoundConstraints(ft, tb.Bounds, tb.FontSize);
            }
            return ft;
        }
        public static FormattedText ToFormattedText(this AutoCompleteBox acb, bool inheritBounds = false) {
            if (acb.FindNameScope().Find("PART_TextBox") is not TextBox tb) {
                return null;
            }
            return tb.ToFormattedText(inheritBounds);
        }

        public static TextLayout ToTextLayout(this TextBox tb) {
            var ft = tb.ToFormattedText();
            var tl = new TextLayout(
                tb.Text ?? string.Empty,
                tb.ToTypeface(),
                Math.Max(1, tb.FontSize),
                tb.Foreground,
                textAlignment: tb.TextAlignment,
                textWrapping: tb.TextWrapping,
                textTrimming: ft.Trimming,
                flowDirection: tb.FlowDirection,
                maxWidth: tb.Bounds.Width,
                maxHeight: tb.Bounds.Height,
                lineHeight: tb.LineHeight,
                letterSpacing: tb.LetterSpacing,
                maxLines: tb.MaxLines);
            return tl;
        }

        private static void SetBoundConstraints(FormattedText ft, Rect r, double fs) {
            // based on docs should add fontsize to max width for wrapping
            ft.MaxTextWidth = Math.Max(1, r.Width + fs);
            ft.MaxTextHeight = Math.Max(1, r.Height);
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
            while (adornerLayer == null) {
                if (sw == null && timeout_ms >= 0) {
                    sw = Stopwatch.StartNew();
                }
                if (sw != null && sw.ElapsedMilliseconds >= timeout_ms) {
                    break;
                }
                //if (adornedControl.GetVisualRoot() == null) {
                //    // control detached (i think)
                //    break;
                //}
                await Task.Delay(100);
                adornerLayer = AdornerLayer.GetAdornerLayer(adornedControl);
            }
            return adornerLayer;
        }

        public static async Task<bool> AddOrReplaceAdornerAsync(this Control adornedControl, Control adorner, int timeout_ms = 1000) {
            // returns false if layer not found within timeout
            Dispatcher.UIThread.VerifyAccess();
            var adornerLayer = await adornedControl.GetAdornerLayerAsync(timeout_ms);
            if (adornerLayer == null) {
                // timeout (removed infinite from hl ext is this ok?)
                return false;
            }
            var cur_adorner = adornerLayer.Children.FirstOrDefault(x => x == adorner);
            if (cur_adorner != null) {
                // why twice?
                MpDebug.Break();
                adornerLayer.Children.Remove(cur_adorner);
            }
            adornerLayer.Children.Add(adorner);
            AdornerLayer.SetAdornedElement(adorner, adornedControl);
            return true;
        }

        #endregion

        #region TextBox

        public static int GetTextIndexFromTextBoxPoint(this TextBox tb, Point p) {
            TextLayout tl = tb.ToTextLayout();
            TextHitTestResult htt = tl.HitTestPoint(p);
            return htt.TextPosition;
        }

        #endregion

        #region Scroll Viewer
        public static ScrollBar GetScrollBar(this ScrollViewer sv, Orientation orientation) {
            if (sv == null) {
                return null;

            }

            var sbl = sv.GetVisualDescendants<ScrollBar>();
            return sbl.FirstOrDefault(x => x.Orientation == orientation);
        }

        public static void ScrollByPointDelta(this ScrollViewer sv, MpPoint delta) {
            if (sv == null) {
                return;
            }
            var hsb = sv.GetScrollBar(Orientation.Horizontal);
            var vsb = sv.GetScrollBar(Orientation.Vertical);

            var new_offset = sv.Offset.ToPortablePoint();
            if (hsb != null) {
                //new_offset.X = Math.Max(0, Math.Min(sv.Offset.X + delta.X, hsb.Maximum));
                new_offset.X = sv.Offset.X + delta.X;
            }
            if (vsb != null) {
                // NOTE willy nilly changing y to MINUS cause i don't think i've tested before now
                // and it was negative but accurate so flipping
                //new_offset.Y = Math.Max(0, Math.Min(sv.Offset.Y + delta.Y, vsb.Maximum));
                new_offset.Y = sv.Offset.Y + delta.Y;
            }

            sv.ScrollToPoint(new_offset);
        }

        public static void ScrollToPoint(this ScrollViewer sv, MpPoint p) {
            sv.ScrollToHorizontalOffset(p.X);
            sv.ScrollToVerticalOffset(p.Y);

            sv.InvalidateMeasure();
            sv.InvalidateArrange();
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
        #endregion

        #region Events

        public static bool IsLeftPress(this PointerPressedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed;
        }

        public static bool IsMiddlePress(this PointerPressedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed;
        }

        public static bool IsRightPress(this PointerPressedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed;
        }

        public static bool IsLeftRelease(this PointerReleasedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased;
        }
        public static bool IsMiddleRelease(this PointerReleasedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonReleased;
        }
        public static bool IsRightRelease(this PointerReleasedEventArgs ppea, Visual? control) {
            return ppea.GetCurrentPoint(control)
                            .Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased;
        }

        public static bool IsLeftDown(this PointerEventArgs e, Visual? control) {
            return e.GetCurrentPoint(control).Properties.IsLeftButtonPressed;
        }
        public static bool IsMiddleDown(this PointerEventArgs e, Visual? control) {
            return e.GetCurrentPoint(control).Properties.IsMiddleButtonPressed;
        }

        public static bool IsRightDown(this PointerEventArgs e, Visual? control) {
            return e.GetCurrentPoint(control).Properties.IsRightButtonPressed;
        }
        public static bool IsAnyButtonDown(this PointerEventArgs e, Visual? control) {
            return e.IsLeftDown(control) || e.IsRightDown(control);
        }
        public static MpPoint GetClientMousePoint(this PointerEventArgs e, Visual? control) {
            return e.GetPosition(control).ToPortablePoint();
        }
        public static MpPoint GetScreenMousePoint(this PointerEventArgs e) {
            //var c_mp = e.GetPosition(control).ToPortablePoint();
            //var c_origin = control.PointToScreen(new Point()).ToPortablePoint(control.VisualPixelDensity());
            //return c_mp + c_origin;
#if MOBILE
            return e.GetPosition(App.MainView).ToPortablePoint();
#endif
            if (e.Source is not Control c ||
                c.GetScreen() is not Screen scr) {
                return MpPoint.Zero;
            }
            var c_mp = e.GetPosition(c);
            var scr_mp = c.PointToScreen(c_mp);
            var result = scr_mp.ToPortablePoint(scr.Scaling);
            return result;
        }

        #endregion

        #region PropertyChanged

        //public static (T oldValue, T newValue) GetOldAndNewValue<T>(this AvaloniaPropertyChangedEventArgs e) {
        //    var ev = (AvaloniaPropertyChangedEventArgs<T>)e;
        //    return (ev.OldValue.GetValueOrDefault()!, ev.NewValue.GetValueOrDefault()!);
        //}

        #endregion

        #region Layout

        public static Thickness ToAvThickness(this double[] dblArr) {
            if (dblArr == null || dblArr.Length == 0) {
                return new Thickness();
            }
            if (dblArr.Length == 1) {
                return new Thickness(dblArr[0]);
            }
            if (dblArr.Length != 4) {
                throw new Exception("Thickness must be 0, 1 or 4 length");
            }
            return new Thickness(dblArr[0], dblArr[1], dblArr[2], dblArr[3]);
        }

        #endregion

        #region Point

        public static MpPoint ToPortablePoint(this Point p) {
            return new MpPoint(p.X, p.Y);
        }
        public static MpPoint ToPortablePoint(this Vector v) {
            return new MpPoint(v.X.IsNumber() ? v.X : 0, v.Y.IsNumber() ? v.Y : 0);
        }


        public static MpPoint ToPortablePoint(this PixelPoint p, double pixelDensity) {
            return new MpPoint(((double)p.X / pixelDensity), ((double)p.Y / pixelDensity));
        }

        public static Size ToAvSize(this MpPoint p) {
            return p.ToPortableSize().ToAvSize();
        }
        public static Point ToAvPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

        public static PixelPoint ToAvPixelPoint(this MpPoint p, double pixelDensity) {
            return new PixelPoint((int)(p.X * pixelDensity), (int)(p.Y * pixelDensity));
        }

        public static MpPoint TranslatePoint(this MpPoint p, Control relativeTo = null, bool toScreen = false) {
            // NOTE when toScreen is FALSE p is assumed to be a screen point

            if (relativeTo == null) {
                relativeTo = Application.Current.GetMainTopLevel();
                if (relativeTo == null) {
                    return p;
                }
            }

            var pd = relativeTo.VisualPixelDensity();
            if (toScreen) {
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



        public static MpSize ToPortableSize(this PixelSize size, double scaling) {
            return new MpSize(size.Width / scaling, size.Height / scaling);
        }

        public static PixelSize ToAvPixelSize(this MpSize size, double scaling) {
            return new PixelSize((int)(size.Width * scaling), (int)(size.Height * scaling));
        }

        public static PixelSize ToAvPixelSize(this Size size, double scaling) {
            return new PixelSize((int)(size.Width * scaling), (int)(size.Height * scaling));
        }

        public static PixelSize ToAvPixelSize(this Point point, double scaling) {
            return new PixelSize((int)(point.X * scaling), (int)(point.Y * scaling));
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
            if (relativeTo == null) {
                return prect;
            }
            prect.TranslateOrigin(relativeTo, toScreen);
            return prect;
        }

        public static Rect ToAvRect(this MpRect rect) {
            return new Rect(rect.Location.ToAvPoint(), rect.Size.ToAvSize());
        }

        public static MpRect ToPortableRect(this PixelRect rect, double pixelDensity) {
            return new MpRect(rect.Position.ToPortablePoint(pixelDensity), rect.Size.ToPortableSize(pixelDensity));
        }

        public static PixelRect ToAvPixelRect(this MpRect rect, double pixelDensity) {
            return new PixelRect(rect.Location.ToAvPixelPoint(pixelDensity), rect.Size.ToAvPixelSize(pixelDensity));
        }

        public static PixelRect ToAvScreenPixelRect(this MpRect rect, double pixelDensity) {
            return new PixelRect(rect.Location.ToAvPixelPoint(pixelDensity), rect.Size.ToAvPixelSize(pixelDensity));
        }



        #endregion
    }
}
