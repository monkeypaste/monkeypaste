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

namespace MpWpfApp {
    
    public class MpMarqueeExtension : DependencyObject {
        internal class MpMarqueeTextImage {
            private bool _isReseting = false;

            private Canvas _canvas;
            private Image _img1, _img2;

            private double _imgWidth;
            private double _maxRenderWidth;

            private double _maxPauseTimeMs;
            private DateTime _pauseStartDateTime;

            private double _maxVelocity = double.MaxValue;

            private DispatcherTimer _timer;

            internal MpMarqueeTextImage(
                Canvas canvas, 
                BitmapSource textBmpSrc, 
                double maxRenderWidth,
                double padWidth,
                double maxVelocity, double pauseTimeMs) {
                _canvas = canvas;

                _imgWidth = textBmpSrc.Width;
                _maxRenderWidth = maxRenderWidth;

                _maxPauseTimeMs = pauseTimeMs;
                _maxVelocity = maxVelocity;

                _img1 = new Image() {
                    Source = textBmpSrc,
                    Width = _imgWidth,
                    Height = textBmpSrc.Height
                };

                _img2 = new Image() {
                    Source = textBmpSrc,
                    Width = _imgWidth,
                    Height = textBmpSrc.Height
                };

                _canvas.Children.Clear();
                _canvas.Children.Add(_img1);
                if(_imgWidth - padWidth > maxRenderWidth) {
                    _canvas.Children.Add(_img2); 
                    _canvas.Width = _imgWidth * 2;
                } else {
                    _canvas.Width = _imgWidth;
                }

                Reset();
            }

            internal void Reset() {
                if(_timer == null) {
                    InitTimer();
                }
                //_timer.Stop();

                
                _isReseting = true;
                _canvas.InvalidateVisual();
            }

            internal void Start() {
                if (_timer == null) {
                    InitTimer();
                }
                _isReseting = false;
                _pauseStartDateTime = DateTime.Now;
                _timer.Start();
            }

            private void Stop() {
                if (_timer == null) {
                    InitTimer();
                }
                _timer.Stop();

                Canvas.SetLeft(_img1, 0);
                Canvas.SetLeft(_img2, _imgWidth);
                _isReseting = false;
            }

            internal void Unload() {
                _timer.Tick -= Timer_Tick;
            }

            private void InitTimer() {
                _timer = new DispatcherTimer(DispatcherPriority.Normal);
                _timer.Interval = TimeSpan.FromMilliseconds(20);
                _timer.Tick += Timer_Tick;
            }

            private void Timer_Tick(object sender, EventArgs e) {
                //if (DateTime.Now - _pauseStartDateTime < TimeSpan.FromMilliseconds(_maxPauseTimeMs) && _maxPauseTimeMs > 0) {
                //    return;
                //}
                if(_canvas.Children.Count < 2) {
                    return;
                }

                double velMultiplier = Mouse.GetPosition(_canvas).X / _canvas.RenderSize.Width;
                velMultiplier = _isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, velMultiplier));
                
                double deltaX = _maxVelocity * velMultiplier;

                double left1 = Canvas.GetLeft(_img1).HasValue() ? Canvas.GetLeft(_img1) : 0;
                double right1 = left1 + _imgWidth;

                double left2 = Canvas.GetLeft(_img2).HasValue() ? Canvas.GetLeft(_img2) : _imgWidth;
                double right2 = left2 + _imgWidth;

                if(_isReseting) {
                    if(Math.Abs(left1) < Math.Abs(left2)) {
                        if(left1 < 0) {
                            deltaX *= -1;
                        }
                    } else {
                        if(left2 < 0) {
                            deltaX *= -1;
                        }
                    }
                }

                double nLeft1 = left1 + deltaX;
                double nRight1 = right1 + deltaX;
                double nLeft2 = left2 + deltaX;
                double nRight2 = right2 + deltaX;

                if(!_isReseting) {
                    if (nLeft1 < nLeft2 && nLeft2 < 0) {
                        nLeft1 = nRight2;
                    } else if (nLeft2 < nLeft1 && nLeft1 < 0) {
                        nLeft2 = nRight1;
                    }
                }

                Canvas.SetLeft(_img1, nLeft1);
                Canvas.SetLeft(_img2, nLeft2);

                if (_isReseting) {
                    if (Math.Abs(nLeft1) < deltaX || Math.Abs(nLeft2) < deltaX) {
                        Stop();
                    }
                }

                _canvas.InvalidateVisual();
            }
        }


        private static Dictionary<DependencyObject, MpMarqueeTextImage> _TextImageLookup = new Dictionary<DependencyObject, MpMarqueeTextImage>();

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
            typeof(MpMarqueeExtension),
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
            typeof(MpMarqueeExtension),
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
            typeof(MpMarqueeExtension),
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
            typeof(MpMarqueeExtension),
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
            typeof(MpMarqueeExtension),
            new PropertyMetadata(0.0));

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
            typeof(MpMarqueeExtension),
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
            typeof(MpMarqueeExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback =  (obj, e) => {
                    if (e.NewValue is bool isEnabled) {
                        var img = obj as Canvas;
                        if (img == null) {
                            if (obj == null) {
                                return;
                            }
                            throw new System.Exception("This extension must be attach to an image control");
                        }
                        if (isEnabled) {
                            if (img.IsLoaded) {
                                Init(img);
                            } else {
                                img.Loaded += Canvas_Loaded;
                            }
                            img.MouseEnter += Canvas_MouseEnter;
                            img.MouseLeave += Canvas_MouseLeave;
                            img.Unloaded += Canvas_Unloaded;
                        } else {
                            Canvas_Unloaded(img, null);
                        }
                    }
                }
            });


        #endregion

        #endregion

        #region Event Handlers

        private static void Canvas_Unloaded(object sender, RoutedEventArgs e) {
            var canvas = sender as Canvas;
            if(canvas == null) {
                return;
            }
            if(_TextImageLookup.ContainsKey(canvas)) {
                _TextImageLookup[canvas].Unload();
                _TextImageLookup.Remove(canvas);
            }

            canvas.Loaded -= Canvas_Loaded;
            canvas.Unloaded -= Canvas_Unloaded;
        }

        private static void Canvas_Loaded(object sender, RoutedEventArgs e) {
            var canvas = sender as Canvas;
            Init(canvas);
        }

        private static void Canvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var canvas = sender as Canvas;
            Reset(canvas);
        }

        private static void Canvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var canvas = sender as Canvas;
            if (canvas == null) {
                return;
            }
            if(!_TextImageLookup.ContainsKey(canvas)) {
                Init(canvas);
            }
            _TextImageLookup[canvas].Start();
        }

        private static void Tb_LostFocus(object sender, RoutedEventArgs e) {
            var tb = sender as TextBox;
            if(tb == null) {
                return;
            }
            var canvas = tb.Tag as Canvas;
            Init(canvas);

            if(canvas != null && 
               new Rect(0,0,GetMaxRenderWidth(canvas),canvas.ActualHeight).Contains(Mouse.GetPosition(canvas))) {
                _TextImageLookup[canvas].Start();
            }
        }

        #endregion

        public static void Reset(Canvas canvas) {
            if (canvas == null || !_TextImageLookup.ContainsKey(canvas)) {
                return;
            }
            _TextImageLookup[canvas].Reset();
        }

        public static void Init(Canvas canvas) {
            if(canvas == null) {
                return;
            }
            TextBox tb = GetTextBox(canvas);

            if(tb == null) {
                return;
            }

            tb.LostFocus -= Tb_LostFocus;
            tb.LostFocus += Tb_LostFocus;

            var dpiInfo = VisualTreeHelper.GetDpi(Application.Current.MainWindow);

            var ft = new FormattedText(
                string.IsNullOrEmpty(tb.Text) ? "Title is empty so what's up T?" : tb.Text, //bitmap must have w/h > 0
                CultureInfo.CurrentCulture,
                tb.FlowDirection,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, new FontFamily("Arial")),
                tb.FontSize,
                Brushes.White,
                dpiInfo.PixelsPerDip);


            int pad = (int)GetTailPadding(canvas);

            var rtb = new RenderTargetBitmap(
                (int)Math.Max(1.0,ft.Width * dpiInfo.PixelsPerDip) + pad + (int)GetDropShadowOffset(canvas).X, 
                (int)Math.Max(1.0,ft.Height * dpiInfo.PixelsPerDip) + (int)GetDropShadowOffset(canvas).Y,
                dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                ft.SetForegroundBrush(Brushes.Black);
                dc.DrawText(ft, GetDropShadowOffset(canvas));
                ft.SetForegroundBrush(Brushes.White);
                dc.DrawText(ft, new Point(0,0));
            }

            rtb.Render(dv);
            var textBmpSrc = MpWpfImagingHelper.ConvertRenderTargetBitmapToBitmapSource(rtb);

            textBmpSrc.Freeze();

            MpMarqueeTextImage mti = new MpMarqueeTextImage(
                canvas, 
                textBmpSrc, 
                GetMaxRenderWidth(canvas), 
                GetTailPadding(canvas),
                GetMaxVelocity(canvas), 
                GetLoopDelayMs(canvas));

            _TextImageLookup.AddOrReplace(canvas, mti);
        }

    }
}