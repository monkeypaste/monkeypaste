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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
namespace MpWpfApp {    
    public class MpMarqueeTextBoxExtension : DependencyObject {
        #region Properties

        #region Canvas Property

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

        #region IsReadOnly Property

        public static bool GetIsReadOnly(DependencyObject obj) {
            return (bool)obj.GetValue(IsReadOnlyProperty);
        }
        public static void SetIsReadOnly(DependencyObject obj, bool value) {
            obj.SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.RegisterAttached(
            "IsReadOnly",
            typeof(bool),
            typeof(MpMarqueeTextBoxExtension),
            new UIPropertyMetadata(true));

        #endregion

        #region EditOnMouseClick Property

        public static bool GetEditOnMouseClick(DependencyObject obj) {
            return (bool)obj.GetValue(EditOnMouseClickProperty);
        }
        public static void SetEditOnMouseClick(DependencyObject obj, bool value) {
            obj.SetValue(EditOnMouseClickProperty, value);
        }

        public static readonly DependencyProperty EditOnMouseClickProperty =
            DependencyProperty.RegisterAttached(
            "EditOnMouseClick",
            typeof(bool),
            typeof(MpMarqueeTextBoxExtension),
            new UIPropertyMetadata(true));

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
            new PropertyMetadata() {
                DefaultValue = new Point(1,1),
                PropertyChangedCallback = (s, e) => {
                    if (s is TextBoxBase tbb) {
                        if (GetIsEnabled(tbb)) {
                            Init(tbb);
                        }
                    }
                }
            });

        #endregion

        #region MarqueeForegroundBrush Property

        public static Brush GetMarqueeForegroundBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(MarqueeForegroundBrushProperty);
        }
        public static void SetMarqueeForegroundBrush(DependencyObject obj, Brush value) {
            obj.SetValue(MarqueeForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty MarqueeForegroundBrushProperty =
            DependencyProperty.RegisterAttached(
            "MarqueeForegroundBrush",
            typeof(Brush),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata() {
                DefaultValue = Brushes.White,
                PropertyChangedCallback = (s, e) => {
                    if (s is TextBoxBase tbb) {
                        if (GetIsEnabled(tbb)) {
                            Init(tbb);
                        }
                    }
                }
            });

        #endregion

        #region DropShadowBrush Property

        public static Brush GetDropShadowBrush(DependencyObject obj) {
            return (Brush)obj.GetValue(DropShadowBrushProperty);
        }
        public static void SetDropShadowBrush(DependencyObject obj, Brush value) {
            obj.SetValue(DropShadowBrushProperty, value);
        }

        public static readonly DependencyProperty DropShadowBrushProperty =
            DependencyProperty.RegisterAttached(
            "DropShadowBrush",
            typeof(Brush),
            typeof(MpMarqueeTextBoxExtension),
            new PropertyMetadata() {
                DefaultValue = Brushes.Black,
                PropertyChangedCallback = (s,e) => {
                    if(s is TextBoxBase tbb) {
                        if(GetIsEnabled(tbb)) {
                            Init(tbb);
                        }
                    }
                }
            });

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
            new PropertyMetadata() {
                DefaultValue = 30.0d,
                PropertyChangedCallback = (s, e) => {
                    if (s is TextBoxBase tbb) {
                        if (GetIsEnabled(tbb)) {
                            Init(tbb);
                        }
                    }
                }
            });

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
            new PropertyMetadata() {
                DefaultValue = -5.0d,
                PropertyChangedCallback = (s, e) => {
                    if (s is TextBoxBase tbb) {
                        if (GetIsEnabled(tbb)) {
                            Init(tbb);
                        }
                    }
                }
            });

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
                        var tbb = obj as TextBoxBase;
                        if (isEnabled) {
                            if(tbb.IsLoaded) {
                                TextBoxBase_Loaded(tbb, null);
                            } else {
                                tbb.Loaded += TextBoxBase_Loaded;
                            }
                        } else {
                            TextBoxBase_Unloaded(tbb, null);
                        }
                    }
                }
            });

        #endregion

        #endregion

        #region Event Handlers
        private static void TextBoxBase_Loaded(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            if (tbb == null) {
                return;
            }
            tbb.Visibility = Visibility.Hidden;

            Panel parentPanel = null;
            var canvas = new Canvas();
            if (tbb.Parent is Panel) {
                parentPanel = tbb.Parent as Panel;
                var childrenToRemove = new List<UIElement>();
                foreach(UIElement cfe in parentPanel.Children) {
                    if(cfe == tbb) {
                        continue;
                    }
                    childrenToRemove.Add(cfe);
                }
                for (int i = 0; i < childrenToRemove.Count; i++) {
                    parentPanel.Children.Remove(childrenToRemove[i]);
                }

                int tbbIdx = parentPanel.Children.IndexOf(tbb);
                parentPanel.Children.Insert(tbbIdx, canvas);
            } else {
                // unknown parent type
                Debugger.Break();
            }
            canvas.Visibility = Visibility.Visible;

            SetCanvas(tbb, canvas);

            MpHelpers.CreateBinding(
                tbb, 
                new PropertyPath(nameof(tbb.ActualHeight)), 
                canvas,
                FrameworkElement.HeightProperty);

            MpHelpers.CreateBinding(
                tbb,
                new PropertyPath(nameof(tbb.IsReadOnly)),
                canvas,
                UIElement.VisibilityProperty,
                System.Windows.Data.BindingMode.OneWay,
                new MpBoolToVisibilityConverter());

            MpHelpers.CreateBinding(
                tbb,
                new PropertyPath(nameof(tbb.IsReadOnly)),
                tbb,
                UIElement.VisibilityProperty,
                System.Windows.Data.BindingMode.OneWay,
                new MpBoolToVisibilityFlipConverter(),
                Application.Current.Resources["Hide"] as string);

            parentPanel.SizeChanged += ParentPanel_SizeChanged;   
            parentPanel.PreviewMouseLeftButtonDown += ParentPanel_PreviewMouseLeftButtonDown;
            parentPanel.Unloaded += ParentPanel_Unloaded;

            canvas.MouseEnter += Canvas_MouseEnter;                        
            canvas.IsVisibleChanged += Canvas_IsVisibleChanged;
            canvas.Unloaded += Canvas_Unloaded;
            
            tbb.DataContextChanged += TextBoxBase_DataContextChanged;
            tbb.Loaded += TextBoxBase_Loaded;
            tbb.Unloaded += TextBoxBase_Unloaded;
            tbb.TextChanged += Tb_TextChanged;
            tbb.LostFocus += Tbb_LostFocus;
            tbb.IsVisibleChanged += Tbb_IsVisibleChanged;

            Init(tbb);
        }

        private static void Canvas_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is Canvas canvas) {
                canvas.PreviewMouseLeftButtonDown -= ParentPanel_PreviewMouseLeftButtonDown;
                canvas.MouseEnter -= Canvas_MouseEnter;
                canvas.SizeChanged -= ParentPanel_SizeChanged;
                canvas.IsVisibleChanged -= Canvas_IsVisibleChanged;
            }
        }

        private static void ParentPanel_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is Panel p) {
                //p.Children.Clear();
                p.SizeChanged -= ParentPanel_SizeChanged;
                p.PreviewMouseLeftButtonDown -= ParentPanel_PreviewMouseLeftButtonDown;
                p.Unloaded -= ParentPanel_Unloaded;
            }            
        }

        private static void TextBoxBase_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is TextBoxBase tbb) {
                tbb.Loaded -= TextBoxBase_Loaded;
                tbb.Unloaded -= TextBoxBase_Unloaded;
                tbb.TextChanged -= Tb_TextChanged;
                tbb.DataContextChanged -= TextBoxBase_DataContextChanged;
                tbb.LostFocus -= Tbb_LostFocus;
                tbb.IsVisibleChanged -= Tbb_IsVisibleChanged;
            } 
        }

        private static void Tbb_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                return;
            }
            if((bool)e.NewValue) {
                var tbb = sender as TextBoxBase;
                if(!tbb.IsFocused) {
                    tbb.Focus();
                }
                if(!tbb.IsKeyboardFocused) {
                    Keyboard.Focus(tbb);
                }
                
                tbb.SelectAll();
            }
        }

        private static void Tbb_LostFocus(object sender, RoutedEventArgs e) {
            var tbb = sender as TextBoxBase;
            if(tbb == null) {
                return;
            }
            SetIsReadOnly(tbb, true);
        }

        private static void ParentPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(sender is Panel p) {
                var tbb = GetTextBoxBaseFromParent(p);
                if(tbb.IsVisible || !GetEditOnMouseClick(tbb)) {
                    return;
                }
                e.Handled = true;
                SetIsReadOnly(tbb, false);
            }            
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

        private static void ParentPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
            var parentPanel = sender as Panel;
            if(parentPanel == null) {
                return;
            }
            Init(GetTextBoxBaseFromParent(parentPanel));
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e) {
            var tbb = sender as TextBoxBase;
            if (tbb == null) {
                return;
            }
            Init(tbb);
        }


        private static void TextBoxBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var tbb = sender as TextBoxBase;
            if (tbb == null) {
                return;
            }
            Init(tbb);
        }
        #endregion

        public static void Init(TextBoxBase tbb) {
            if(tbb == null || tbb.DataContext == null) {
                return;
            }
            Canvas canvas = GetCanvas(tbb);

            if(canvas == null) {
                return;
            } else if(canvas.Parent == null) {
                //TextBoxBase_Loaded(tbb,null);
                Debugger.Break();
            }

            var textBmpSrc = GetMarqueeBitmapSource(tbb);

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

            double maxRenderWidth = tbb.ActualWidth;
            if(tbb.Parent is Panel p) {
                maxRenderWidth = p.ActualWidth;
            }
            if (textBmpSrc.Width > maxRenderWidth) {
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
        private static string GetText(TextBoxBase tbb) {
            if(tbb is TextBox tb) {
                return tb.Text;
            } else if(tbb is RichTextBox rtb) {
                return rtb.Document.ToPlainText();
            }
            //Unknown tbb
            Debugger.Break();
            return string.Empty;
        }

        private static BitmapSource GetMarqueeBitmapSource(TextBoxBase tbb) {
            var canvas = GetCanvas(tbb);
            var dpiInfo = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            int pad = (int)GetTailPadding(canvas);

            if (tbb is TextBox) {
                var ft = new FormattedText(
                    GetText(tbb),
                    CultureInfo.CurrentCulture,
                    tbb.FlowDirection,
                    new Typeface(tbb.FontFamily, tbb.FontStyle, tbb.FontWeight, tbb.FontStretch, new FontFamily("Arial")),
                    tbb.FontSize,
                    GetMarqueeForegroundBrush(tbb),
                    dpiInfo.PixelsPerDip);


                var ftBmpSrc = new RenderTargetBitmap(
                    (int)Math.Max(1.0, ft.Width * dpiInfo.PixelsPerDip) + pad + (int)GetDropShadowOffset(canvas).X,
                    (int)Math.Max(1.0, ft.Height * dpiInfo.PixelsPerDip) + (int)GetDropShadowOffset(canvas).Y,
                    dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen()) {
                    ft.SetForegroundBrush(GetDropShadowBrush(tbb));
                    dc.DrawText(ft, GetDropShadowOffset(canvas));
                    ft.SetForegroundBrush(GetMarqueeForegroundBrush(tbb));
                    dc.DrawText(ft, new Point(0, 0));
                }

                ftBmpSrc.Render(dv);
                return MpWpfImagingHelper.ConvertRenderTargetBitmapToBitmapSource(ftBmpSrc);
            } else if(tbb is RichTextBox rtb) {
                var fgFd = rtb.Document.Clone();
                var bgFd = rtb.Document.Clone();

                new TextRange(fgFd.ContentStart, fgFd.ContentEnd).ApplyPropertyValue(TextElement.ForegroundProperty, GetMarqueeForegroundBrush(tbb));
                new TextRange(bgFd.ContentStart, bgFd.ContentEnd).ApplyPropertyValue(TextElement.ForegroundProperty, GetDropShadowBrush(tbb));

                var fgBmpSrc = fgFd.ToBitmapSource();
                var bgBmpSrc = bgFd.ToBitmapSource();

                var outBmpSrc = new RenderTargetBitmap(
                    (int)Math.Max(1.0, rtb.ActualWidth * dpiInfo.PixelsPerDip) + pad + (int)GetDropShadowOffset(canvas).X,
                    (int)Math.Max(1.0, rtb.ActualHeight * dpiInfo.PixelsPerDip) + (int)GetDropShadowOffset(canvas).Y,
                    dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen()) {
                    dc.DrawImage(bgBmpSrc, rtb.Bounds());
                    dc.DrawImage(fgBmpSrc, rtb.Bounds());
                }

                outBmpSrc.Render(dv);
                return MpWpfImagingHelper.ConvertRenderTargetBitmapToBitmapSource(outBmpSrc);
            }
            return null;
        }

        private static TextBoxBase GetTextBoxBaseFromParent(Panel p) {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(p); i++) {
                if (p.Children[i] is TextBoxBase tbb && GetCanvas(tbb) != null) {
                    return tbb;
                }
            }
            return null;
        }
    }
}