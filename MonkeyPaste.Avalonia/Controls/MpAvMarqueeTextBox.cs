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
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System.Linq;
using Avalonia.Visuals.Media.Imaging;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvMarqueeTextBox : TextBox, IStyleable {
        Type IStyleable.StyleKey => typeof(TextBox);

        #region Private Variables

        private Bitmap _marqueeBitmap { get; set; }

        private MpSize _ftSize;

        private string _orgText;

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

        private IBrush _readOnlyForeground = Brushes.White;
        public IBrush ReadOnlyForeground { 
            get => _readOnlyForeground;
            set => SetAndRaise(ReadOnlyForegroundProperty, ref _readOnlyForeground, value);
        }

        public static readonly StyledProperty<IBrush> ReadOnlyForegroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ReadOnlyForeground),Brushes.White);

        #endregion

        #region ReadOnlyBackground AvaloniaProperty

        private IBrush _readOnlyBackground = Brushes.Transparent;
        public IBrush ReadOnlyBackground {
            get => _readOnlyBackground;
            set => SetAndRaise(ReadOnlyBackgroundProperty, ref _readOnlyBackground, value);
        }

        public static readonly StyledProperty< IBrush> ReadOnlyBackgroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ReadOnlyBackground), Brushes.Transparent);

        #endregion

        #region EditableForeground AvaloniaProperty

        private IBrush _editableForeground = Brushes.Black;
        public IBrush EditableForeground {
            get => _editableForeground;
            set => SetAndRaise(EditableForegroundProperty, ref _editableForeground, value);
        }

        public static readonly StyledProperty<IBrush> EditableForegroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(EditableForeground), Brushes.Black);

        #endregion

        #region EditableBackground AvaloniaProperty

        private IBrush _editableBackground = Brushes.White;
        public IBrush EditableBackground {
            get => _editableBackground;
            set => SetAndRaise(EditableBackgroundProperty, ref _editableBackground, value);
        }

        public static readonly StyledProperty<IBrush> EditableBackgroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(EditableBackground), Brushes.White);

        #endregion

        #region DropShadowOffset AvaloniaProperty

        private Point _dropShadowOffset = new Point(1, 1);
        public Point DropShadowOffset {
            get => _dropShadowOffset;
            set => SetAndRaise(DropShadowOffsetProperty, ref _dropShadowOffset, value);
        }

        public static readonly StyledProperty<Point> DropShadowOffsetProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, Point>(nameof(DropShadowOffset), new Point(1, 1));

        #endregion

        #region DropShadowBrush AvaloniaProperty

        private IBrush _dropShadowBrush = Brushes.Black;
        public IBrush DropShadowBrush {
            get => _dropShadowBrush;
            set => SetAndRaise(DropShadowBrushProperty, ref _dropShadowBrush, value);
        }

        public static readonly StyledProperty<IBrush> DropShadowBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(DropShadowBrush), Brushes.Black);

        #endregion

        #region TailPadding AvaloniaProperty

        private double _tailPadding = 30.0d;
        public double TailPadding {
            get => _tailPadding;
            set => SetAndRaise(TailPaddingProperty, ref _tailPadding, value);
        }

        public static readonly StyledProperty<double> TailPaddingProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(nameof(TailPadding), 30.0d);

        #endregion

        #region MaxVelocity AvaloniaProperty

        private double _maxVelocity = -5.0d;
        public double MaxVelocity {
            get => _maxVelocity;
            set => SetAndRaise(MaxVelocityProperty, ref _maxVelocity, value);
        }

        public static readonly StyledProperty<double> MaxVelocityProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(nameof(MaxVelocity), -5.0d);

        #endregion

        #region TotalLoopWaitMs AvaloniaProperty
        private int _totalLoopWaitMs = 1000;
        public int TotalLoopWaitMs {
            get => _totalLoopWaitMs;
            set => SetAndRaise(TotalLoopWaitMsProperty, ref _totalLoopWaitMs, value);
        }

        public static readonly StyledProperty<int> TotalLoopWaitMsProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox,int>(nameof(TotalLoopWaitMs), 1000);

        #endregion

        #region CanDisableReadOnly AvaloniaProperty

        private bool _editOnFocus = true;
        public bool EditOnFocus {
            get => _editOnFocus;
            set => SetAndRaise(EditOnFocusProperty, ref _editOnFocus, value);
        }

        public static readonly StyledProperty< bool> EditOnFocusProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(EditOnFocus), true);

        #endregion

        #region BeginEditCommand AvaloniaProperty

        private ICommand _beginEditCommand;
        public ICommand BeginEditCommand {
            get => _beginEditCommand;
            set => SetAndRaise(BeginEditCommandProperty, ref _beginEditCommand, value);
        }

        public static readonly StyledProperty< ICommand> BeginEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(BeginEditCommand), null);

        #endregion

        #region EndEditCommand AvaloniaProperty
        
        private ICommand _endEditCommand;
        public ICommand EndEditCommand {
            get => _endEditCommand;
            set => SetAndRaise(EndEditCommandProperty, ref _endEditCommand, value);
        }

        public static readonly StyledProperty< ICommand> EndEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(EndEditCommand), null);

        #endregion

        #region CancelEditCommand AvaloniaProperty
        private ICommand _cancelEditCommand;
        public ICommand CancelEditCommand {
            get => _cancelEditCommand;
            set => SetAndRaise(CancelEditCommandProperty, ref _cancelEditCommand, value);
        }

        public static readonly StyledProperty< ICommand> CancelEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(CancelEditCommand), null);

        #endregion

        #region IsMarqueeEnabled AvaloniaProperty
        private bool _isMarqueeEnabled = true;
        public bool IsMarqueeEnabled {
            get => _isMarqueeEnabled;
            set => SetAndRaise(IsMarqueeEnabledProperty, ref _isMarqueeEnabled, value);
        }

        public static readonly StyledProperty<bool> IsMarqueeEnabledProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(IsMarqueeEnabled), true);

        #endregion

        #region AutoMarquee AvaloniaProperty
        private bool _autoMarquee = false;
        public bool AutoMarquee {
            get => _autoMarquee;
            set => SetAndRaise(AutoMarqueeProperty, ref _autoMarquee, value);
        }

        public static readonly StyledProperty<bool> AutoMarqueeProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(AutoMarquee), false);

        #endregion

        #endregion

        public MpAvMarqueeTextBox() {
            this.AcceptsReturn = false;
            this.AcceptsTab = false;
            this.TextWrapping = TextWrapping.NoWrap;
            this.ClipToBounds = true;
            this.BorderThickness = new Thickness(0);
            //this.MinHeight = 5;
            this.IsReadOnly = true;
            //this.HorizontalAlignment = HorizontalAlignment.Stretch;
            //this.VerticalAlignment = VerticalAlignment.Stretch;

            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            this.GetObservable(MpAvMarqueeTextBox.IsReadOnlyProperty).Subscribe(value => OnIsReadOnlyChanged());
            this.GetObservable(MpAvMarqueeTextBox.TextProperty).Subscribe(value => OnTextChanged());
            this.GetObservable(MpAvMarqueeTextBox.EditOnFocusProperty).Subscribe(value => OnCanEditChanged());

            this.GetObservable(MpAvMarqueeTextBox.ReadOnlyBackgroundProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ReadOnlyForegroundProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.DropShadowBrushProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.DropShadowOffsetProperty).Subscribe(value => Init());

            this.AddHandler(MpAvMarqueeTextBox.KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
            this.AddHandler(MpAvMarqueeTextBox.KeyUpEvent, HandleKeyUp, RoutingStrategies.Tunnel);
        }

        #region Event Handlers
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if(AutoMarquee) {
                Init();
                AnimateAsync().FireAndForgetSafeAsync();
            }
        }
        protected override void OnGotFocus(GotFocusEventArgs e) {
            base.OnGotFocus(e);
            if (DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
            if (EditOnFocus) {
                SetValue(IsReadOnlyProperty, false);
                BeginEditCommand?.Execute(null);
            }
        }

        protected override void OnLostFocus(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLostFocus(e);
            SetValue(IsReadOnlyProperty, true);            
        }

        //protected override void OnKeyDown(KeyEventArgs e) {
        //    HandleKeyDown(e);
        //    base.OnKeyDown(e);
        //}

        protected override void OnMeasureInvalidated() {
            base.OnMeasureInvalidated();
            Init();
        }
        protected override void OnPointerEnter(PointerEventArgs e) {
            base.OnPointerEnter(e);
            _tb_mp = e.GetClientMousePoint(this);
            AnimateAsync().FireAndForgetSafeAsync();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            if(DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
        }
        protected override void OnPointerLeave(PointerEventArgs e) {
            base.OnPointerLeave(e);
            _tb_mp = null;
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            _tb_mp = e.GetClientMousePoint(this);
        }

        private void OnTextChanged() {
            Init();
        }
        private void OnCanEditChanged() {
            if(!EditOnFocus) {
                SetValue(IsReadOnlyProperty, true);
            }
        }
        private void OnIsReadOnlyChanged() {
            SetTextBoxIsVisible(!IsReadOnly);
            if(IsReadOnly) {
                this.KillFocus();
            } else {
                _orgText = Text;
                SelectAll();
            }
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

        private void HandleKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Space) {
                string pre_str = Text.Substring(0, SelectionStart);
                string post_str = Text.Substring(SelectionEnd);
                string new_text = pre_str + " " + post_str;
                int new_sel_start = SelectionStart + 1;
                SetValue(TextProperty, new_text);
                SelectionStart = new_sel_start;
                SelectionEnd = new_sel_start;
                e.Handled = true;
                return;
            }
        }
        private void HandleKeyUp(object sender, KeyEventArgs e) {
            if (IsReadOnly) {
                return;
            }
            e.Handled = true;
            //if (e.Key == Key.Space) {
            //    string pre_str = Text.Substring(0, SelectionStart);
            //    string post_str = Text.Substring(SelectionEnd);
            //    string new_text = pre_str + " " + post_str;
            //    int new_sel_start = SelectionStart + 1;
            //    SetValue(TextProperty, new_text);
            //    SelectionStart = new_sel_start;
            //    SelectionEnd = new_sel_start;
            //    e.Handled = true;
            //    return;
            //}
            if (e.Key == Key.Escape) {
                SetValue(IsReadOnlyProperty, true);
                CancelEditCommand?.Execute(null);
                if (CancelEditCommand == null &&
                    Text != _orgText &&
                    !string.IsNullOrEmpty(_orgText)) {
                    Text = _orgText;
                }
            } else if (e.Key == Key.Enter) {
                SetValue(IsReadOnlyProperty, true);
                EndEditCommand?.Execute(null);
            }
        }
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
        private void SetTextBoxIsVisible(bool isTextBoxVisible) {
            foreach (var c in VisualChildren) {
                c.IsVisible = isTextBoxVisible;
            }
            if (isTextBoxVisible) {
                SetValue(BackgroundProperty, EditableBackground);
                SetValue(ForegroundProperty, EditableForeground);
            } else {
                SetValue(BackgroundProperty, ReadOnlyBackground);
                SetValue(ForegroundProperty, ReadOnlyForeground);
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
            ctx.DrawImage(_marqueeBitmap, rect1.ToAvRect(),rect1.ToAvRect(), BitmapInterpolationMode.HighQuality);
            
            if(CanMarquee()) {
                MpRect rect2 = bmp_rect;
                rect2.X = _offsetX2;
                ctx.DrawImage(_marqueeBitmap, rect2.ToAvRect(), rect2.ToAvRect(), BitmapInterpolationMode.HighQuality);
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

                var cmp = _tb_mp == null ? new MpPoint() : _tb_mp;
                bool isReseting = _tb_mp == null || !new MpRect(MpPoint.Zero,this.Bounds.Size.ToPortableSize()).Contains(cmp);

                //var tb = GetTextBoxFromParent(canvas.Parent as Panel);
                double max_width = GetRenderWidth();
                double velMultiplier = isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, cmp.X / max_width));

                if(AutoMarquee) {
                    if(_tb_mp != null) {
                        AutoMarquee = false;
                    } else {
                        velMultiplier = 1.0d;
                        isReseting = false;
                    }
                }
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
                
                context.DrawText(DropShadowBrush, DropShadowOffset, ft.PlatformImpl);
                context.DrawText(ReadOnlyForeground, new Point(0, 0), ft.PlatformImpl);
            }

            var bmp = ftBmp.ToAvBitmap();
            //MpFileIo.WriteByteArrayToFile(@"C:\Users\tkefauver\Desktop\text_bmp.png", bmp.ToBytesFromBase64String(), false);

            return bmp;
        }
        #endregion
    }
}
