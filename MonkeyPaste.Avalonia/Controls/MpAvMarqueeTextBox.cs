using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;
using Key = Avalonia.Input.Key;
using Cursor = Avalonia.Input.Cursor;
using Avalonia.VisualTree;
using Gtk;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvMarqueeTextBox : TextBox, IStyleable {
        Type IStyleable.StyleKey => typeof(TextBox);

        #region Private Variables

        private Bitmap _marqueeBitmap { get; set; }

        private MpSize _ftSize;

        private double _offsetX1 { get; set; }
        private double _offsetX2 { get; set; }

        private MpPoint _tb_mp;

        private int _delayMs = 20;

        private int _curLoopWaitMs = 0;
        private double _distTraveled = 0;
        #endregion

        #region Statics

        static MpAvMarqueeTextBox() {

        }
        #endregion

        #region Properties

        #region ReadOnlyForeground AvaloniaProperty

        private IBrush _readOnlyForeground;
        public IBrush ReadOnlyForeground { 
            get {
                if(_readOnlyForeground == null) {
                    _readOnlyForeground = Foreground;
                }
                return _readOnlyForeground;
            }
            set {
                _readOnlyForeground = value;
            }
        }

        public static readonly StyledProperty<IBrush> ReadOnlyForegroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ReadOnlyForeground));

        #endregion

        #region ReadOnlyBackground AvaloniaProperty

        private IBrush _readOnlyBackground;
        public IBrush ReadOnlyBackground {
            get {
                if (_readOnlyBackground == null) {
                    return Background;
                }
                return _readOnlyBackground;
            }
            set {
                _readOnlyBackground = value;
            }
        }

        public static readonly StyledProperty< IBrush> ReadOnlyBackgroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ReadOnlyBackground));

        #endregion

        #region DropShadowOffset AvaloniaProperty
        public MpPoint DropShadowOffset { get; set; } = new MpPoint(1, 1);

        public static readonly StyledProperty<MpPoint> DropShadowOffsetProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, MpPoint>(nameof(DropShadowOffset));

        #endregion

        #region DropShadowBrush AvaloniaProperty
        public IBrush DropShadowBrush { get; set; } = Brushes.Black;

        public static readonly StyledProperty<IBrush> DropShadowBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(DropShadowBrush));

        #endregion

        #region TailPadding AvaloniaProperty
        public double TailPadding { get; set; } = 30.0d;

        public static readonly StyledProperty<double> TailPaddingProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(nameof(TailPadding));

        #endregion

        #region MaxVelocity AvaloniaProperty
        public double MaxVelocity { get; set; } = -5.0d;

        public static readonly StyledProperty<double> MaxVelocityProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(nameof(MaxVelocity));

        #endregion

        #region TotalLoopWaitMs AvaloniaProperty

        public int TotalLoopWaitMs { get; set; } = 1000;

        public static readonly StyledProperty< int> TotalLoopWaitMsProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, int>(nameof(TotalLoopWaitMs));

        #endregion

        #region CanEdit AvaloniaProperty
        public bool CanEdit { get; set; } = true;

        public static readonly StyledProperty< bool> CanEditProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(CanEdit));

        #endregion

        #region BeginEditCommand AvaloniaProperty
        public ICommand BeginEditCommand { get; set; }

        public static readonly StyledProperty< ICommand> BeginEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(BeginEditCommand));

        #endregion

        #region EndEditCommand AvaloniaProperty
        public ICommand EndEditCommand { get; set; }

        public static readonly StyledProperty< ICommand> EndEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(EndEditCommand));

        #endregion

        #region CancelEditCommand AvaloniaProperty
        public ICommand CancelEditCommand { get; set; }

        public static readonly StyledProperty< ICommand> CancelEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(CancelEditCommand));

        #endregion

        #region IsMarqueeEnabled AvaloniaProperty
        public bool IsMarqueeEnabled { get; set; } = true;

        public static readonly StyledProperty<bool> IsMarqueeEnabledProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(IsMarqueeEnabled));

        #endregion

        #endregion

        public MpAvMarqueeTextBox() {
            
            this.GetObservable(MpAvMarqueeTextBox.IsVisibleProperty).Subscribe(value => OnIsVisibleChanged());
            this.GetObservable(MpAvMarqueeTextBox.IsReadOnlyProperty).Subscribe(value => OnIsReadOnlyChanged());
            this.GetObservable(MpAvMarqueeTextBox.TextProperty).Subscribe(value => OnTextChanged());
            this.GetObservable(MpAvMarqueeTextBox.CanEditProperty).Subscribe(value => OnCanEditChanged());
        }

        #region Event Handlers

        protected override void OnGotFocus(GotFocusEventArgs e) {
            base.OnGotFocus(e);
            if(CanEdit) {
                SetValue(IsReadOnlyProperty, false);
                BeginEditCommand?.Execute(null);
            }
        }

        protected override void OnLostFocus(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLostFocus(e);
            SetValue(IsReadOnlyProperty, true);
            
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            if(IsReadOnly) {
                return;
            }
            if(e.Key == Key.Escape) {
                SetValue(IsReadOnlyProperty, true);
                CancelEditCommand?.Execute(null);
                return;
            }
            if(e.Key == Key.Enter) {
                SetValue(IsReadOnlyProperty, true);
                EndEditCommand?.Execute(null);
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnMeasureInvalidated() {
            base.OnMeasureInvalidated();
            Init();
        }

        protected override void OnPointerEnter(PointerEventArgs e) {
            base.OnPointerEnter(e);
            _tb_mp = e.GetClientMousePoint(this);
            AnimateAsync().FireAndForgetSafeAsync();
        }
        protected override void OnPointerLeave(PointerEventArgs e) {
            base.OnPointerLeave(e);
            _tb_mp = null;
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            _tb_mp = e.GetClientMousePoint(this);
        }

        private void OnIsVisibleChanged() {

        }
        private void OnTextChanged() {
            Init();
        }
        private void OnCanEditChanged() {
            if(!CanEdit) {
                SetValue(IsReadOnlyProperty, true);
            }
        }
        private void OnIsReadOnlyChanged() {
            Init();
            this.InvalidateAll();
        }

        public override void Render(DrawingContext context) {
            if(!IsVisible) {
                return;
            }
            if (IsReadOnly) {
                SetTextBoxIsVisible(false);
                RenderMarquee(context);
            } else {
                SetTextBoxIsVisible(true);
                base.Render(context);
            }
        }


        #endregion

        #region Private Methods

        private void Init() {
            _marqueeBitmap = GetMarqueeBitmap(out _ftSize);
            _offsetX1 = 0;
            if (CanMarquee()) {
                _offsetX2 = _marqueeBitmap.Size.Width + TailPadding;
            } else {
                _offsetX2 = 0;
            }

            this.InvalidateVisual();
        }
        private void SetTextBoxIsVisible(bool isVisible) {
            foreach (var c in VisualChildren) {
                c.IsVisible = isVisible;
            }
            if (isVisible) {
                SetValue(BackgroundProperty, Brushes.White);
                SetValue(ForegroundProperty, Brushes.Black);
            } else {
                //_fgBrush = Foreground;
                SetValue(ForegroundProperty, Brushes.Transparent);

                //_bgBrush = Background;
                SetValue(BackgroundProperty, Brushes.Transparent);
            }
        }
        private void RenderMarquee(DrawingContext ctx) {
            if(_marqueeBitmap == null) {
                return;
            }

            ctx.FillRectangle(ReadOnlyBackground, this.Bounds);

            MpRect bmp_rect = new MpRect(MpPoint.Zero, _marqueeBitmap.Size.ToPortableSize());

            MpRect rect1 = bmp_rect;
            rect1.X = _offsetX1;
            ctx.DrawImage(_marqueeBitmap, rect1.ToAvRect());
            
            if(CanMarquee()) {
                MpRect rect2 = bmp_rect;
                rect2.X = _offsetX2;
                ctx.DrawImage(_marqueeBitmap, rect2.ToAvRect());
            }
        }

        private bool CanMarquee() {
            return 
                IsMarqueeEnabled && 
                _marqueeBitmap != null && 
                this.IsVisible && 
                _ftSize.Width > GetRenderWidth();
        }

        private double GetRenderWidth() {
            if(!this.IsVisible) {
                return 0;
            }
            if (this.MaxWidth.HasValue()) {
                return this.MaxWidth;
            }
            if (this.Bounds.Width.HasValue()) {
                return this.Bounds.Width;
            }
            return 100;
        }

        private async Task AnimateAsync() {
            while (true) {

                if (!CanMarquee()) {
                    return;
                }

                double bmp_width = _marqueeBitmap.Size.Width;

                var cmp = _tb_mp;
                cmp = cmp == null ? new MpPoint() : cmp;
                bool isReseting = _tb_mp == null || !new MpRect(MpPoint.Zero,this.Bounds.Size.ToPortableSize()).Contains(cmp);

                //var tb = GetTextBoxFromParent(canvas.Parent as Panel);
                double max_width = GetRenderWidth();
                double velMultiplier = cmp.X / max_width;
                velMultiplier = isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, velMultiplier));

                double deltaX = MaxVelocity * velMultiplier;

                double left1 = _offsetX1;
                double right1 = _offsetX1 + bmp_width;

                double left2 = _offsetX2;
                double right2 = _offsetX2 + bmp_width;

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

                if (isReseting) {
                    if (Math.Abs(nLeft1) < deltaX || Math.Abs(nLeft2) < deltaX) {
                        _offsetX1 = 0;
                        _offsetX2 = bmp_width;
                        this.InvalidateVisual();

                        int test = TotalLoopWaitMs;
                        _curLoopWaitMs = 0;
                        _distTraveled = 0;
                        return;
                    }
                }

                double maxLoopDeltaX = 5;
                bool isInitialLoop = Math.Abs(_distTraveled) < bmp_width;
                bool isLoopDelaying = !isInitialLoop &&
                                        (Math.Abs(nLeft1) < maxLoopDeltaX || Math.Abs(nLeft2) < maxLoopDeltaX);

                if (isLoopDelaying) {
                    //pause this cycle
                    _curLoopWaitMs += _delayMs;
                    // initial loop delay (snap to 0)
                    nLeft1 = 0;
                    nLeft2 = bmp_width;
                }
                if (_curLoopWaitMs > 1000) {
                    //loop delay is over reset elapsed and bump so not caught next pass
                    _curLoopWaitMs = 0;
                    double vel_dir = MaxVelocity > 0 ? 1 : -1;
                    nLeft1 = (maxLoopDeltaX + 0.5) * vel_dir;
                    nLeft2 = nLeft1 + bmp_width;
                }

                _offsetX1 = nLeft1;
                _offsetX2 = nLeft2;

                this.InvalidateVisual();

                await Task.Delay(_delayMs);
            }
        }

        private Bitmap GetMarqueeBitmap(out MpSize ftSize) {
            if(string.IsNullOrEmpty(Text)) {
                ftSize = MpSize.Empty;
                return null;
            }

            var canvas = this;
            int pad = (int)TailPadding;

            double fs = Math.Max(1.0d, FontSize);

            Size textSize = new Size(Text.Length * fs, fs);
            var ft = this.ToFormattedText();
            ft.FontSize = Math.Max(1.0d, FontSize);

            ftSize = ft.Bounds.Size.ToPortableSize();
            ft.Constraint = ftSize.ToAvSize();
            // pixelsPerDip = 1.75
            // pixelsPerInch = 168

            //var dpi = MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelsPerInch.ToAvVector();
            double pixelsPerDip = 1;// MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelDensity;

            var ftBmp = new RenderTargetBitmap(
                new PixelSize(
                    (int)Math.Max(1.0, ft.Bounds.Width * pixelsPerDip) + pad + (int)DropShadowOffset.X,
                    (int)Math.Max(1.0, ft.Bounds.Height * pixelsPerDip) + (int)DropShadowOffset.Y));

            using (var context = ftBmp.CreateDrawingContext(null)) {
                if (ReadOnlyBackground is SolidColorBrush scb) {
                    context.Clear(scb.Color);
                } else {
                    context.Clear(Colors.Transparent);
                }
                
                context.DrawText(DropShadowBrush, DropShadowOffset.ToAvPoint(), ft.PlatformImpl);
                context.DrawText(ReadOnlyForeground, new Point(0, 0), ft.PlatformImpl);
            }

            var bmp = ftBmp.ToAvBitmap();

            //MpFileIo.WriteByteArrayToFile(@"C:\Users\tkefauver\Desktop\text_bmp.png", bmp.ToBytesFromBase64String(), false);

            return bmp;
        }
        #endregion
    }
}
