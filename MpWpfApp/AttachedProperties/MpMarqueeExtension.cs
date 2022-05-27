using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste.Plugin;
using System.Threading.Tasks;

namespace MpWpfApp {    
    public class MpMarqueeTextBoxExtension : DependencyObject {
        #region Properties

        #region TextBox Property

        public static TextBox GetTextBox(DependencyObject obj) {
            return (TextBox)obj.GetValue(TextBoxProperty);
        }
        public static void SetTextBox(DependencyObject obj, TextBox value) {
            obj.SetValue(TextBoxProperty, value);
        }

        public static readonly DependencyProperty TextBoxProperty =
            DependencyProperty.RegisterAttached(
            "TextBox",
            typeof(TextBox),
            typeof(MpMarqueeTextBoxExtension),
            new UIPropertyMetadata(null));

        #endregion

        #region Canvas Property (DependencyObject is TextBox)

        public static Canvas GetCanvas(DependencyObject obj) {
            return (Canvas)obj.GetValue(CanvasProperty);
        }
        public static void SetCanvas(DependencyObject obj, Canvas value) {
            obj.SetValue(CanvasProperty, value);
        }

        public static readonly DependencyProperty CanvasProperty =
            DependencyProperty.RegisterAttached(
            "Canvas",
            typeof(Canvas),
            typeof(MpMarqueeTextBoxExtension),
            new UIPropertyMetadata(null));

        #endregion

        #region DropShadowOffset Property

        public static Point GetDropShadowOffset(DependencyObject obj) {
            return (Point)obj.GetValue(DropShadowOffsetProperty);
        }
        public static void SetDropShadowOffset(DependencyObject obj, Point value) {
            obj.SetValue(DropShadowOffsetProperty, value);
        }

        public static readonly DependencyProperty DropShadowOffsetProperty =
            DependencyProperty.RegisterAttached(
            "DropShadowOffset",
            typeof(Point),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata(new Point(1,1)));

        #endregion

        #region TailPadding Property

        public static double GetTailPadding(DependencyObject obj) {
            return (double)obj.GetValue(TailPaddingProperty);
        }
        public static void SetTailPadding(DependencyObject obj, double value) {
            obj.SetValue(TailPaddingProperty, value);
        }

        public static readonly DependencyProperty TailPaddingProperty =
            DependencyProperty.RegisterAttached(
            "TailPadding",
            typeof(double),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata(20.0));

        #endregion

        #region MaxVelocity Property

        public static double GetMaxVelocity(DependencyObject obj) {
            return (double)obj.GetValue(MaxVelocityProperty);
        }
        public static void SetMaxVelocity(DependencyObject obj, double value) {
            obj.SetValue(MaxVelocityProperty, value);
        }

        public static readonly DependencyProperty MaxVelocityProperty =
            DependencyProperty.RegisterAttached(
            "MaxVelocity",
            typeof(double),
            typeof(MpMarqueeTextBoxExtension),
            new FrameworkPropertyMetadata(-20.0));

        #endregion

        #region LoopDelayMs Property

        public static double GetLoopDelayMs(DependencyObject obj) {
            return (double)obj.GetValue(LoopDelayMsProperty);
        }
        public static void SetLoopDelayMs(DependencyObject obj, double value) {
            obj.SetValue(LoopDelayMsProperty, value);
        }

        public static readonly DependencyProperty LoopDelayMsProperty =
            DependencyProperty.RegisterAttached(
            "LoopDelayMs",
            typeof(double),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata(0.0));

        #endregion

        #region Text Property

        public static string GetText(DependencyObject obj) {
            return (string)obj.GetValue(TextProperty);
        }
        public static void SetText(DependencyObject obj, string value) {
            obj.SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata(string.Empty));

        #endregion

        #region MaxRenderWidth Property

        public static double GetMaxRenderWidth(DependencyObject obj) {
            return (double)obj.GetValue(MaxRenderWidthProperty);
        }
        public static void SetMaxRenderWidth(DependencyObject obj, double value) {
            obj.SetValue(MaxRenderWidthProperty, value);
        }

        public static readonly DependencyProperty MaxRenderWidthProperty =
            DependencyProperty.RegisterAttached(
            "MaxRenderWidth",
            typeof(double),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata(0.0));

        #endregion

        #region IsEnabled Property

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpMarqueeTextBoxExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback =  (obj, e) => {
                    if (e.NewValue is bool isEnabled) {
                        var canvas = obj as Canvas;
                        if (isEnabled) {
                            if(canvas.IsLoaded) {
                                Canvas_Loaded(canvas, null);
                            } else {
                                canvas.Loaded += Canvas_Loaded;
                            }
                        } else {
                            Canvas_Unloaded(canvas, null);
                        }
                    }
                }
            });

        #endregion

        #endregion

        #region Event Handlers
        private static void Canvas_Loaded(object sender, RoutedEventArgs e) {
            var canvas = sender as Canvas;
            if (canvas == null) {
                return;
            }
            TextBox tb = GetTextBox(canvas);

            if (tb == null) {
                return;
            }

            SetCanvas(tb, canvas);

            canvas.MouseEnter += Canvas_MouseEnter;
            canvas.Loaded += Canvas_Loaded;
            canvas.Unloaded += Canvas_Unloaded;
            canvas.SizeChanged += Canvas_SizeChanged;
            canvas.IsVisibleChanged += Canvas_IsVisibleChanged;
            canvas.DataContextChanged += Canvas_DataContextChanged;

            tb.TextChanged += Tb_TextChanged;

            Init(canvas);
        }

        private static void Canvas_Unloaded(object sender, RoutedEventArgs e) {
            var canvas = sender as Canvas;
            if(canvas == null) {
                return;
            }
            canvas.MouseEnter -= Canvas_MouseEnter;
            canvas.Loaded -= Canvas_Loaded;
            canvas.Unloaded -= Canvas_Unloaded;
            canvas.SizeChanged -= Canvas_SizeChanged;
            canvas.IsVisibleChanged -= Canvas_IsVisibleChanged;
            canvas.DataContextChanged -= Canvas_DataContextChanged;
            
            TextBox tb = GetTextBox(canvas);

            if (tb == null) {
                return;
            }
            tb.TextChanged -= Tb_TextChanged;
        }        

        private static void Canvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var canvas = sender as Canvas;
            if (canvas == null) {
                return;
            }
            Animate(canvas).FireAndForgetSafeAsync(MpClipTrayViewModel.Instance);
        }

        private static void Canvas_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if((bool)e.NewValue == false) {
                return;
            }
            Canvas_MouseEnter(sender, null);
        }
        private static void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) {
            var canvas = sender as Canvas;
            if(canvas == null) {
                return;
            }
            Init(canvas);
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e) {
            var tb = sender as TextBox;
            if (tb == null) {
                return;
            }
            Init(GetCanvas(tb));
        }


        private static void Canvas_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var canvas = sender as Canvas;
            if (canvas == null) {
                return;
            }
            Init(canvas);
        }
        #endregion

        public static void Init(Canvas canvas) {
            if(canvas == null) {
                return;
            }
            TextBox tb = GetTextBox(canvas);

            if(tb == null) {
                return;
            }
            var dpiInfo = VisualTreeHelper.GetDpi(Application.Current.MainWindow);

            var ft = new FormattedText(
                GetText(canvas),
                CultureInfo.CurrentCulture,
                tb.FlowDirection,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, new FontFamily("Arial")),
                tb.FontSize,
                Brushes.White,
                dpiInfo.PixelsPerDip);

            int pad = (int)GetTailPadding(canvas);

            var ftBmpSrc = new RenderTargetBitmap(
                (int)Math.Max(1.0, ft.Width * dpiInfo.PixelsPerDip) + pad + (int)GetDropShadowOffset(canvas).X,
                (int)Math.Max(1.0, ft.Height * dpiInfo.PixelsPerDip) + (int)GetDropShadowOffset(canvas).Y,
                dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                ft.SetForegroundBrush(Brushes.Black);
                dc.DrawText(ft, GetDropShadowOffset(canvas));
                ft.SetForegroundBrush(Brushes.White);
                dc.DrawText(ft, new Point(0, 0));
            }

            ftBmpSrc.Render(dv);
            var textBmpSrc = MpWpfImagingHelper.ConvertRenderTargetBitmapToBitmapSource(ftBmpSrc);

            var img1 = new Image() {
                Source = textBmpSrc,
                Width = textBmpSrc.Width,
                Height = textBmpSrc.Height
            };

            var img2 = new Image() {
                Source = textBmpSrc,
                Width = textBmpSrc.Width,
                Height = textBmpSrc.Height
            };

            canvas.Children.Clear();
            canvas.Children.Add(img1);
            if (textBmpSrc.Width - pad > GetMaxRenderWidth(canvas)) {
                canvas.Children.Add(img2);
                canvas.Width = textBmpSrc.Width * 2;
            } else {
                canvas.Width = textBmpSrc.Width;
            }

            canvas.InvalidateMeasure();
            canvas.InvalidateVisual();
            img1.InvalidateMeasure();
            img1.InvalidateVisual();
            img2.InvalidateMeasure();
            img2.InvalidateVisual();
            (canvas.Parent as FrameworkElement).UpdateLayout();
        }

        private static async Task Animate(Canvas canvas) {
            while (true) {
                if (!canvas.IsVisible || canvas.Children.Count < 2) {
                    return;
                }
                var img1 = VisualTreeHelper.GetChild(canvas, 0) as Image;
                var img2 = VisualTreeHelper.GetChild(canvas, 1) as Image;

                var cmp = Mouse.GetPosition(canvas);
                bool isReseting = !canvas.Bounds().Contains(cmp);

                double velMultiplier = cmp.X / canvas.RenderSize.Width;
                velMultiplier = isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, velMultiplier));

                double deltaX = GetMaxVelocity(canvas) * velMultiplier;

                double left1 = Canvas.GetLeft(img1).HasValue() ? Canvas.GetLeft(img1) : 0;
                double right1 = left1 + img1.Width;

                double left2 = Canvas.GetLeft(img2).HasValue() ? Canvas.GetLeft(img2) : img1.Width;
                double right2 = left2 + img2.Width;

                if (isReseting) {
                    if (Math.Abs(left1) < Math.Abs(left2)) {
                        if (left1 < 0) {
                            deltaX *= -1;
                        }
                    } else {
                        if (left2 < 0) {
                            deltaX *= -1;
                        }
                    }
                }

                double nLeft1 = left1 + deltaX;
                double nRight1 = right1 + deltaX;
                double nLeft2 = left2 + deltaX;
                double nRight2 = right2 + deltaX;

                if (!isReseting) {
                    if (nLeft1 < nLeft2 && nLeft2 < 0) {
                        nLeft1 = nRight2;
                    } else if (nLeft2 < nLeft1 && nLeft1 < 0) {
                        nLeft2 = nRight1;
                    }
                }

                Canvas.SetLeft(img1, nLeft1);
                Canvas.SetLeft(img2, nLeft2);

                if (isReseting) {
                    if (Math.Abs(nLeft1) < deltaX || Math.Abs(nLeft2) < deltaX) {
                        Canvas.SetLeft(img1, 0);
                        Canvas.SetLeft(img2, img1.Width);
                        canvas.InvalidateVisual();
                        return;
                    }
                }

                canvas.InvalidateVisual();

                await Task.Delay(20);
            }
        }
    }
}