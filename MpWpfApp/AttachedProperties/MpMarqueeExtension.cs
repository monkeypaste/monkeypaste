using MonkeyPaste;
using System;
using System.Collections.Generic;
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
            private Image _imageControl;

            private PixelColor[,] _wrapPixels;

            private double _maxPauseTimeMs;
            private DateTime _pauseStartDateTime;

            private WriteableBitmap _wrapBitmap;

            private double _maxVelocity = double.MaxValue;

            private int _maxTextOffsetX => _textBitmap == null ? 0 : _textBitmap.PixelWidth;

            private double _accumOffset = 0;
            private int _columnOffsetIdx = 0;

            private DispatcherTimer _timer;

            private BitmapSource _textBitmap;

            private int _renderPixelWidth;
            
            internal WriteableBitmap RenderBitmap { get; set; }


            internal MpMarqueeTextImage(Image imageControl, BitmapSource textBmpSrc, int renderPixelWidth, double maxVelocity, double pauseTimeMs) {
                _imageControl = imageControl;

                _renderPixelWidth = renderPixelWidth;
                _maxVelocity = maxVelocity;
                _textBitmap = textBmpSrc;

                Reset();

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(20);
                _timer.Tick += Timer_Tick;
            }

            

            private void UpdateRenderBitmap() {
                BitmapSource renderBmpSrc;
                if (_renderPixelWidth < _wrapBitmap.PixelWidth) {
                    renderBmpSrc = new CroppedBitmap(
                    _wrapBitmap,
                    new Int32Rect(0, 0, _renderPixelWidth, _wrapBitmap.PixelHeight));
                } else {
                    renderBmpSrc = _wrapBitmap;
                }
                RenderBitmap = new WriteableBitmap(renderBmpSrc);

                //string fp_render = MpFileIoHelpers.GetUniqueFileOrDirectoryName(
                //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "marqueeTest"),
                //    "render.png");

                //MpFileIoHelpers.WriteByteArrayToFile(
                //    fp_render,
                //    RenderBitmap.ToByteArray());

                //string fp_wrap = MpFileIoHelpers.GetUniqueFileOrDirectoryName(
                //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "marqueeTest"),
                //    "wrap.png");

                //MpFileIoHelpers.WriteByteArrayToFile(
                //    fp_wrap,
                //    _wrapBitmap.ToByteArray());

                _imageControl.Source = RenderBitmap;
                _imageControl.Width = RenderBitmap.Width;
                _imageControl.Height = RenderBitmap.Height;
                _imageControl.InvalidateVisual();
            }

            internal void Reset() {
                _accumOffset = 0;

                if(_timer != null) {
                    _timer.Stop();
                }

                if (_textBitmap != null && _textBitmap.PixelWidth > 0 && _textBitmap.PixelHeight > 0) {
                    _wrapPixels = MpWpfImagingHelper.GetPixels(_textBitmap);
                    _wrapBitmap = new WriteableBitmap(_textBitmap);

                    UpdateRenderBitmap();
                }
            }

            internal void Start() {
                _pauseStartDateTime = DateTime.Now;
                _timer.Start();
            }

            internal void Stop() {
                _timer.Stop();
            }

            private void Timer_Tick(object sender, EventArgs e) {
                if (DateTime.Now - _pauseStartDateTime < TimeSpan.FromMilliseconds(_maxPauseTimeMs)) {
                    return;
                }

                double velMultiplier = Mouse.GetPosition(_imageControl).X / _imageControl.RenderSize.Width;
                velMultiplier = Math.Min(1.0, Math.Max(0.0, velMultiplier));

                //_accumOffset += (_maxVelocity * velMultiplier);
                //if((int)_accumOffset > _columnOffsetIdx) {
                //    _columnOffsetIdx = (int)_accumOffset;
                //    if(_columnOffsetIdx >= _maxTextOffsetX) {
                //        _accumOffset = 0;
                //        _columnOffsetIdx = 0;
                //    }
                //    int offsetCount = (int)(_maxVelocity * velMultiplier);
                //    for (int i = 0; i < offsetCount; i++) {
                //        OffsetWrapBitmap();
                //    }

                //    _pauseStartDateTime = DateTime.Now;
                //}

                for (int i = 0; i < 3; i++) {
                    OffsetWrapBitmap();
                }
                UpdateRenderBitmap();
            }

            private void OffsetWrapBitmap() {
                if(_wrapBitmap == null) {
                    return;
                }
                PixelColor[] firstColumn = new PixelColor[_wrapBitmap.PixelHeight];
                PixelColor[,] pixel = new PixelColor[1, 1];
                _wrapBitmap.Cop
                for (int c = 0; c < _wrapBitmap.PixelWidth; c++) {
                    //PixelColor[] nextColumn = new PixelColor[_wrapBitmap.PixelHeight];
                    for (int r = 0; r < _wrapBitmap.PixelHeight; r++) {
                        if(c == 0) {
                            firstColumn[r] = _wrapPixels[c, r];
                        }
                        //int offset = _columnOffsetIdx + c;
                        //if(offset >= _wrapBitmap.PixelWidth) {
                        //    offset = MpMathHelpers.WrapValue(offset, 0, _wrapBitmap.PixelWidth - 1);
                        //}
                        
                        _wrapPixels[c, r] = c == _wrapBitmap.PixelWidth - 1 ?
                            firstColumn[r] : _wrapPixels[c + 1, r];

                        pixel[0,0] = _wrapPixels[c, r];
                        MpWpfImagingHelper.PutPixels(_wrapBitmap, pixel, c, r);
                    }
                }
                //UpdateRenderBitmap();
            }

            public void ShiftBitmap() {
                double Width = _wrapBitmap.Width;
                double Height = _wrapBitmap.Height;
                int BytesPerPixel = _wrapBitmap.Format.BitsPerPixel / 8;
                byte[] buffer = new byte[(int)(Width * Height * BytesPerPixel)]; //new byte[(int)(Width * Height * 4)];
                try {
                    unsafe {

                        _wrapBitmap.Lock();
                        int pBackBuffer = (int)_wrapBitmap.BackBuffer;
                        int pBackBuffer2 = (int)_wrapBitmap.BackBuffer;
                        for (int w = (int)Width - 1; w > 2; --w) {
                            pBackBuffer = (int)_wrapBitmap.BackBuffer + (w * BytesPerPixel);
                            pBackBuffer2 = (int)_wrapBitmap.BackBuffer + ((w - 1) * BytesPerPixel);
                            for (int h = 0; h < (int)Height - 2; ++h) {
                                pBackBuffer += _wrapBitmap.BackBufferStride;
                                pBackBuffer2 += _wrapBitmap.BackBufferStride;
                                *((int*)pBackBuffer) = *((int*)pBackBuffer2);
                            }
                        }
                        _wrapBitmap.Unlock();
                    }

                    _wrapBitmap.CopyPixels(new Int32Rect(0, 0, (int)(Width - 1), (int)Height), buffer,
                                     _wrapBitmap.BackBufferStride - BytesPerPixel, 0);

                    _wrapBitmap.Lock();
                    _wrapBitmap.WritePixels(new Int32Rect(1, 0, (int)Width - 1, (int)Height), buffer,
                                     _wrapBitmap.BackBufferStride - BytesPerPixel, 1, 0);

                    _wrapBitmap.AddDirtyRect(new Int32Rect(0, 0, (int)Width, (int)Height));
                    _wrapBitmap.Unlock();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private static Dictionary<DependencyObject, MpMarqueeTextImage> _TextImageLookup = new Dictionary<DependencyObject, MpMarqueeTextImage>();
        
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
        public static void SetTailPadding(DependencyObject obj, Point value) {
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
        public static void SetMaxVelocity(DependencyObject obj, Point value) {
            obj.SetValue(MaxVelocityProperty, value);
        }

        public static readonly DependencyProperty MaxVelocityProperty =
            DependencyProperty.RegisterAttached(
            "MaxVelocity",
            typeof(double),
            typeof(MpMarqueeExtension),
            new PropertyMetadata(3.0));

        #endregion

        #region LoopDelayMs Property

        public static double GetLoopDelayMs(DependencyObject obj) {
            return (double)obj.GetValue(LoopDelayMsProperty);
        }
        public static void SetLoopDelayMs(DependencyObject obj, Point value) {
            obj.SetValue(LoopDelayMsProperty, value);
        }

        public static readonly DependencyProperty LoopDelayMsProperty =
            DependencyProperty.RegisterAttached(
            "LoopDelayMs",
            typeof(double),
            typeof(MpMarqueeExtension),
            new PropertyMetadata(1000.0));

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
                        var img = obj as Image;
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
                                img.Loaded += Img_Loaded;
                            }
                            img.IsVisibleChanged += Img_IsVisibleChanged;
                            img.MouseEnter += Img_MouseEnter;
                            img.MouseLeave += Img_MouseLeave;
                            img.Unloaded += Img_Unloaded;
                        } else {
                            Img_Unloaded(img, null);
                        }
                    }
                }
            });


        #endregion

        #region Event Handlers

        private static void Img_Unloaded(object sender, RoutedEventArgs e) {
            var img = sender as Image;
            if(img == null) {
                return;
            }
            if(_TextImageLookup.ContainsKey(img)) {
                _TextImageLookup.Remove(img);
            }

            img.Loaded -= Img_Loaded;
            img.Unloaded -= Img_Unloaded;
        }

        private static void Img_Loaded(object sender, RoutedEventArgs e) {
            var img = sender as Image;
            if(img == null) {
                return;
            }
            Init(img);
        }

        private static void Img_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var img = sender as Image;
            if(img == null) {
                return;
            }
            Init(img);
        }


        private static void Img_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            var img = sender as Image;
            if (img == null || !_TextImageLookup.ContainsKey(img)) {
                return;
            }
            _TextImageLookup[img].Stop();
        }

        private static void Img_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            var img = sender as Image;
            if (img == null || !_TextImageLookup.ContainsKey(img)) {
                return;
            }
            _TextImageLookup[img].Start();
        }

        #endregion

        private static void Init(Image img) {           
            TextBox tb = GetTextBox(img);

            if(tb == null) {
                return;
            }

            var dpiInfo = VisualTreeHelper.GetDpi(Application.Current.MainWindow);

            var ft = new FormattedText(
                string.IsNullOrEmpty(tb.Text) ? "Title is empty so what's up T?" : tb.Text, //bitmap must have w/h > 0
                CultureInfo.CurrentCulture,
                tb.FlowDirection,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch, new FontFamily("Arial")),
                tb.FontSize,
                Brushes.White,
                dpiInfo.PixelsPerDip);

            int pad = (int)GetTailPadding(img);

            var rtb = new RenderTargetBitmap(
                (int)Math.Max(1.0,ft.Width * dpiInfo.PixelsPerDip) + pad + (int)GetDropShadowOffset(img).X, 
                (int)Math.Max(1.0,ft.Height * dpiInfo.PixelsPerDip) + (int)GetDropShadowOffset(img).Y,
                dpiInfo.PixelsPerInchX, dpiInfo.PixelsPerInchY, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                ft.SetForegroundBrush(Brushes.Black);
                dc.DrawText(ft, GetDropShadowOffset(img));
                ft.SetForegroundBrush(Brushes.White);
                dc.DrawText(ft, new Point(0,0));
            }

            rtb.Render(dv);
            var bmpSrc = MpWpfImagingHelper.ConvertRenderTargetBitmapToBitmapSource(rtb);

            MpMarqueeTextImage mti = new MpMarqueeTextImage(img,bmpSrc, (int)MpMeasurements.Instance.ClipTileTitleTextGridMaxWidth, GetMaxVelocity(img), GetLoopDelayMs(img));
            _TextImageLookup.AddOrReplace(img, mti);

            //img.Source = bmpSrc;
            //img.Width = rtb.Width;
            //img.Height = rtb.Height;

            //string fp = MpFileIoHelpers.GetUniqueFileOrDirectoryName(
            //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "marqueeTest"),
            //    "textBoxImageTest.png");

            //string fp2 = MpFileIoHelpers.GetUniqueFileOrDirectoryName(
            //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "marqueeTest"),
            //    "textBoxImageTest_otherSave.png");

            //MpFileIoHelpers.WriteByteArrayToFile(
            //    fp,
            //    bmpSrc.ToByteArray());

            //var encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(rtb));

            //using (var file = File.OpenWrite(fp2)) {
            //    encoder.Save(file);
            //}
        }
    }
}