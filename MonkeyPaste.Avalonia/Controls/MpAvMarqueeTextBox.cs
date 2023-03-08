using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Bcpg;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Input;
using Cursor = Avalonia.Input.Cursor;
using Key = Avalonia.Input.Key;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public class MpAvMarqueeTextBox : TextBox, IStyleable {
        Type IStyleable.StyleKey => typeof(TextBox);

        #region Private Variables

        private IBrush _defNavUriBrush = Brushes.Maroon;
        private FormattedText _ft;
        private MpSize _ftSize;
        private MpSize _bmpSize;

        private string _orgText;

        private double _offsetX1 { get; set; }
        private double _offsetX2 { get; set; }

        private MpPoint _tb_mp;

        private int _delayMs = (int)(1000 / 50);

        private int _curLoopWaitMs = 0;
        private double _distTraveled = 0;
        #endregion

        #region Statics

        static MpAvMarqueeTextBox() {
        }
        #endregion

        #region Properties

        private MpTextRange _contentRange;
        public MpTextRange ContentRange {
            get {
                if (_contentRange == null) {
                    _contentRange = new MpTextRange(this);
                }
                return _contentRange;
            }
        }

        #region ReadOnlyForeground AvaloniaProperty

        private static IBrush _defReadOnlyForeground = Brushes.White;
        public IBrush ReadOnlyForeground {
            get => GetValue(ReadOnlyForegroundProperty);
            set => SetValue(ReadOnlyForegroundProperty, value); //SetAndRaise(ReadOnlyForegroundProperty, ref _readOnlyForeground, value);
        }

        public static readonly StyledProperty<IBrush> ReadOnlyForegroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ReadOnlyForeground), _defReadOnlyForeground);


        #endregion

        #region ReadOnlyBackground AvaloniaProperty

        private IBrush _readOnlyBackground = Brushes.Transparent;
        public IBrush ReadOnlyBackground {
            get => _readOnlyBackground;
            set => SetAndRaise(ReadOnlyBackgroundProperty, ref _readOnlyBackground, value);
        }

        public static readonly StyledProperty<IBrush> ReadOnlyBackgroundProperty =
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

        private double _maxVelocity = -3.0d;
        public double MaxVelocity {
            get => _maxVelocity;
            set => SetAndRaise(MaxVelocityProperty, ref _maxVelocity, value);
        }

        public static readonly StyledProperty<double> MaxVelocityProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, double>(nameof(MaxVelocity), -3.0d);

        #endregion

        #region TotalLoopWaitMs AvaloniaProperty
        private int _totalLoopWaitMs = 1000;
        public int TotalLoopWaitMs {
            get => _totalLoopWaitMs;
            set => SetAndRaise(TotalLoopWaitMsProperty, ref _totalLoopWaitMs, value);
        }

        public static readonly StyledProperty<int> TotalLoopWaitMsProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, int>(nameof(TotalLoopWaitMs), 1000);

        #endregion

        #region EditOnFocus AvaloniaProperty

        private bool _editOnFocus = true;
        public bool EditOnFocus {
            get => _editOnFocus;
            set => SetAndRaise(EditOnFocusProperty, ref _editOnFocus, value);
        }

        public static readonly StyledProperty<bool> EditOnFocusProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(EditOnFocus), true);

        #endregion

        #region EnableReadOnlyOnLostFocus AvaloniaProperty

        private bool _enableReadOnlyOnLostFocus = true;
        public bool EnableReadOnlyOnLostFocus {
            get => _enableReadOnlyOnLostFocus;
            set => SetAndRaise(EnableReadOnlyOnLostFocusProperty, ref _enableReadOnlyOnLostFocus, value);
        }

        public static readonly StyledProperty<bool> EnableReadOnlyOnLostFocusProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(EnableReadOnlyOnLostFocus), true);

        #endregion

        #region BeginEditCommand AvaloniaProperty

        private ICommand _beginEditCommand;
        public ICommand BeginEditCommand {
            get => _beginEditCommand;
            set => SetAndRaise(BeginEditCommandProperty, ref _beginEditCommand, value);
        }

        public static readonly StyledProperty<ICommand> BeginEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(BeginEditCommand), null);

        #endregion

        #region EndEditCommand AvaloniaProperty

        private ICommand _endEditCommand;
        public ICommand EndEditCommand {
            get => _endEditCommand;
            set => SetAndRaise(EndEditCommandProperty, ref _endEditCommand, value);
        }

        public static readonly StyledProperty<ICommand> EndEditCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(EndEditCommand), null);

        #endregion

        #region CancelEditCommand AvaloniaProperty
        private ICommand _cancelEditCommand;
        public ICommand CancelEditCommand {
            get => _cancelEditCommand;
            set => SetAndRaise(CancelEditCommandProperty, ref _cancelEditCommand, value);
        }

        public static readonly StyledProperty<ICommand> CancelEditCommandProperty =
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

        #region FocusOnDisableReadOnly AvaloniaProperty
        private bool _focusOnDisableReadOnly = false;
        public bool FocusOnDisableReadOnly {
            get => _focusOnDisableReadOnly;
            set => SetAndRaise(FocusOnDisableReadOnlyProperty, ref _focusOnDisableReadOnly, value);
        }

        public static readonly StyledProperty<bool> FocusOnDisableReadOnlyProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(FocusOnDisableReadOnly), false);

        #endregion

        #region SelectViewModelOnFocus AvaloniaProperty
        private bool _selectViewModelOnFocus = true;
        public bool SelectViewModelOnFocus {
            get => _selectViewModelOnFocus;
            set => SetAndRaise(SelectViewModelOnFocusProperty, ref _selectViewModelOnFocus, value);
        }

        public static readonly StyledProperty<bool> SelectViewModelOnFocusProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(nameof(SelectViewModelOnFocus), true);

        #endregion

        #region HoverBrush AvaloniaProperty
        public IBrush HoverBrush {
            get => GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> HoverBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(HoverBrush));

        #endregion

        #region NavigateUri AvaloniaProperty
        public string NavigateUri {
            get => GetValue(NavigateUriProperty);
            set => SetValue(NavigateUriProperty, value);
        }

        public static readonly StyledProperty<string> NavigateUriProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, string>(nameof(NavigateUri));

        #endregion

        #region NavigateUriCommand AvaloniaProperty
        public ICommand NavigateUriCommand {
            get => GetValue(NavigateUriCommandProperty);
            set => SetValue(NavigateUriCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> NavigateUriCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(nameof(NavigateUriCommand));

        #endregion

        #region NavigateUriCommandParameter AvaloniaProperty
        public object NavigateUriCommandParameter {
            get => GetValue(NavigateUriCommandParameterProperty);
            set => SetValue(NavigateUriCommandParameterProperty, value);
        }

        public static readonly StyledProperty<object> NavigateUriCommandParameterProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, object>(nameof(NavigateUriCommandParameter));

        #endregion

        #region Highlighting

        #region HighlightRanges AvaloniaProperty
        // FORMAT [index,count]

        public ObservableCollection<MpTextRange> HighlightRanges {
            get => GetValue(HighlightRangesProperty);
            set => SetValue(HighlightRangesProperty, value);
        }

        public static readonly StyledProperty<ObservableCollection<MpTextRange>> HighlightRangesProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ObservableCollection<MpTextRange>>(nameof(HighlightRanges), null);

        #endregion

        #region ActiveHighlightIdx AvaloniaProperty

        public int? ActiveHighlightIdx {
            get => GetValue(ActiveHighlightIdxProperty);
            set => SetValue(ActiveHighlightIdxProperty, value);
        }

        public static readonly StyledProperty<int?> ActiveHighlightIdxProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, int?>(nameof(ActiveHighlightIdx), null);

        #endregion

        #region ActiveHighlightBrush AvaloniaProperty

        public IBrush ActiveHighlightBrush {
            get => GetValue(ActiveHighlightBrushProperty);
            set => SetValue(ActiveHighlightBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> ActiveHighlightBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(ActiveHighlightBrush), Brushes.Lime);

        #endregion

        #region InactiveHighlightBrush AvaloniaProperty

        public IBrush InactiveHighlightBrush {
            get => GetValue(InactiveHighlightBrushProperty);
            set => SetValue(InactiveHighlightBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> InactiveHighlightBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(nameof(InactiveHighlightBrush), Brushes.Gold);

        #endregion

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
            if (!this.FontSize.IsNumber() || this.FontSize < 1) {
                this.FontSize = 1;
            }
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

            this.GetObservable(MpAvMarqueeTextBox.HighlightRangesProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ActiveHighlightIdxProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ActiveHighlightBrushProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.InactiveHighlightBrushProperty).Subscribe(value => Init());

            this.AddHandler(MpAvMarqueeTextBox.KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
            this.AddHandler(MpAvMarqueeTextBox.KeyUpEvent, HandleKeyUp, RoutingStrategies.Tunnel);
        }

        #region Event Handlers
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if (AutoMarquee) {
                Init();
                AnimateAsync().FireAndForgetSafeAsync();
            }
        }
        protected override void OnGotFocus(GotFocusEventArgs e) {
            base.OnGotFocus(e);
            if (DataContext is MpISelectableViewModel svm &&
                SelectViewModelOnFocus) {
                svm.IsSelected = true;
            }
            if (EditOnFocus) {
                SetValue(IsReadOnlyProperty, false);
                BeginEditCommand?.Execute(null);
            }
        }

        protected override void OnLostFocus(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLostFocus(e);
            if (EnableReadOnlyOnLostFocus) {
                SetValue(IsReadOnlyProperty, true);
            }
        }

        //protected override void OnKeyDown(KeyEventArgs e) {
        //    HandleKeyDown(e);
        //    base.OnKeyDown(e);
        //}

        protected override void OnMeasureInvalidated() {
            base.OnMeasureInvalidated();
            Init();
        }
        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            if (NavigateUri != null && IsReadOnly) {
                this.Cursor = new Cursor(StandardCursorType.Hand);
                Dispatcher.UIThread.Post(this.InvalidateVisual);
            }
            _tb_mp = e.GetClientMousePoint(this);
            AnimateAsync().FireAndForgetSafeAsync();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            if (DataContext is MpIConditionalSelectableViewModel csvm &&
                !csvm.CanSelect) {
                return;
            }
            if (DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
            if (NavigateUri != null &&
                IsReadOnly &&
                e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                NavigateUriCommand?.Execute(NavigateUriCommandParameter);
            }
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            if (NavigateUri != null && IsReadOnly) {
                this.Cursor = Cursor.Default;
                Dispatcher.UIThread.Post(this.InvalidateVisual);
            }
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
            if (!EditOnFocus) {
                SetValue(IsReadOnlyProperty, true);
            }
        }
        private void OnIsReadOnlyChanged() {
            Dispatcher.UIThread.Post(async () => {
                SetTextBoxIsVisible(!IsReadOnly);
                await Task.Delay(300);

                if (IsReadOnly) {
                    await this.TryKillFocusAsync();
                } else {
                    _orgText = Text;
                    if (FocusOnDisableReadOnly) {
                        bool success = await this.TrySetFocusAsync();
                        if (!success) {
                            //MpDebug.Break("Focus error");
                        }
                    }
                    await Task.Delay(300);
                    SelectAll();
                }
                Init();
                this.InvalidateAll();
            });
        }

        public override void Render(DrawingContext context) {
            if (!IsVisible) {
                return;
            }
            if (IsReadOnly) {
                // NOTE disabling marquee image, its distorted (update SetTextBoxIsVisible to swap out)
                SetTextBoxIsVisible(false);
                RenderMarquee(context);
                //base.Render(context);
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
            if (e.Key == Key.Enter) {
                // prevent avalonia from collapsing treeitem
                e.Handled = true;
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
            ContentRange.Count = Text == null ? 0 : Text.Length;

            _bmpSize = GetScaledTextSize(out _ftSize);
            _offsetX1 = 0;
            if (CanMarquee()) {
                _offsetX2 = _bmpSize.Width + TailPadding;
            } else {
                _offsetX2 = 0;
            }
            MinHeight = _ftSize.Height;
            Dispatcher.UIThread.Post(InvalidateVisual);
        }
        private void SetTextBoxIsVisible(bool isTextBoxVisible) {
            Dispatcher.UIThread.Post(() => {
                foreach (var c in VisualChildren) {
                    c.IsVisible = isTextBoxVisible;
                    //if (c is Control vc) {
                    //    vc.IsHitTestVisible = false;
                    //}

                }
                if (isTextBoxVisible) {
                    SetValue(BackgroundProperty, EditableBackground);
                    SetValue(ForegroundProperty, EditableForeground);
                } else {
                    SetValue(BackgroundProperty, ReadOnlyBackground);
                    SetValue(ForegroundProperty, ReadOnlyForeground);
                }
            });

        }
        private void RenderMarquee(DrawingContext ctx) {
            if (_ft == null) {
                Init();
                if (_ft == null) {
                    return;
                }
            }

            IBrush fg = ReadOnlyForeground;
            if (IsPointerOver) {
                if (HoverBrush != null) {
                    fg = HoverBrush;
                } else if (NavigateUri != null) {
                    fg = _defNavUriBrush;
                }
            }

            // CLEAR BG
            ctx.FillRectangle(ReadOnlyBackground, this.Bounds);
            double hl_x = GetActiveHighlighAdjOffset(ctx, _offsetX1);


            var origin1 = new Point(_offsetX1 + hl_x, 0);
            // DRAW SHADOW 1
            DrawReadOnlyText(ctx, DropShadowBrush, origin1 + DropShadowOffset, false);
            // DRAG FG 1
            DrawReadOnlyText(ctx, fg, origin1, true);

            if (CanMarquee()) {
                var origin2 = new Point(_offsetX2 + hl_x, 0);
                // DRAW SHADOW 2
                DrawReadOnlyText(ctx, DropShadowBrush, origin2 + DropShadowOffset, false);
                // DRAG FG 2
                DrawReadOnlyText(ctx, fg, origin2, true);
            }
        }
        private double GetActiveHighlighAdjOffset(DrawingContext ctx, double x_offset) {
            if (HighlightRanges == null ||
                HighlightRanges.Count == 0 ||
                ActiveHighlightIdx == null) {
                return 0;
            }
            var test_hl_geom = _ft.BuildHighlightGeometry(new Point(x_offset, 0), HighlightRanges[ActiveHighlightIdx.Value].StartIdx, 1);
            if (DataContext is MpAvClipTileViewModel ctvm && ctvm.CopyItemId == 24) {

            }
            double delta_x = 0;
            double max_x = x_offset + GetRenderWidth();
            if (max_x < test_hl_geom.Bounds.Left) {
                // when active off to the right offset so its start is in the middle
                delta_x = max_x - test_hl_geom.Bounds.Left - (GetRenderWidth() / 2);
            } else if (test_hl_geom.Bounds.Left < x_offset) {
                delta_x = test_hl_geom.Bounds.Left - x_offset;
            }
            return delta_x;
        }
        private void DrawReadOnlyText(DrawingContext ctx, IBrush fg, Point offset, bool showHighlight) {
            if (showHighlight &&
                HighlightRanges != null &&
                HighlightRanges.Count > 0) {
                var brl = GetAllBrushes(ReadOnlyBackground, InactiveHighlightBrush, ActiveHighlightBrush, ActiveHighlightIdx, HighlightRanges.ToArray());
                var gl = brl.Select(x => _ft.BuildHighlightGeometry(offset, x.Value.StartIdx, x.Value.Count));
                gl.ForEach((x, idx) => ctx.DrawGeometry(brl[idx].Key, null, x));
            }

            _ft.SetForegroundBrush(fg);
            ctx.DrawText(_ft, offset);
        }

        private KeyValuePair<IBrush, MpTextRange>[] GetAllBrushes(
            IBrush def_brush,
            IBrush hl_brush,
            IBrush active_hl_brush,
            int? active_idx,
            MpTextRange[] hl_ranges) {

            List<KeyValuePair<IBrush, MpTextRange>> brush_tuples = new List<KeyValuePair<IBrush, MpTextRange>>();
            if (string.IsNullOrEmpty(Text)) {
                return brush_tuples.ToArray();
            }
            if (hl_ranges.Length == 0) {
                // no hl return whole range with def
                brush_tuples.Add(new KeyValuePair<IBrush, MpTextRange>(def_brush, new MpTextRange(ContentRange.Document, 0, Text.Length)));
                return brush_tuples.ToArray();
            }
            if (hl_ranges.First().StartIdx > 0) {
                // add pre def
                brush_tuples.Add(new KeyValuePair<IBrush, MpTextRange>(def_brush, new MpTextRange(ContentRange.Document, 0, hl_ranges[0].BeforeStartIdx)));
            }

            foreach (var (hlr, hlr_idx) in hl_ranges.WithIndex()) {
                var cur_brush = active_idx.HasValue && hlr_idx == active_idx ? active_hl_brush : hl_brush;
                brush_tuples.Add(new KeyValuePair<IBrush, MpTextRange>(cur_brush, hlr));
                if (hlr_idx < hl_ranges.Length - 1 &&
                    hlr.AfterEndIdx < Text.Length) {
                    int inner_sidx = hlr.AfterEndIdx;
                    int inner_count = hl_ranges[hlr_idx + 1].StartIdx - inner_sidx;
                    if (inner_count > 0) {
                        // add inner def range
                        brush_tuples.Add(new KeyValuePair<IBrush, MpTextRange>(def_brush, new MpTextRange(ContentRange.Document, inner_sidx, inner_count)));
                    }
                }
            }
            if (hl_ranges.Last().AfterEndIdx < Text.Length - 1) {
                // add post def
                brush_tuples.Add(new KeyValuePair<IBrush, MpTextRange>(def_brush, new MpTextRange(ContentRange.Document, hl_ranges.Last().AfterEndIdx, Text.Length - hl_ranges.Last().AfterEndIdx)));
            }

            var valid = brush_tuples.Where(x => x.Value.StartIdx < 0 || x.Value.EndIdx >= Text.Length);
            if (valid.Any()) {
                string test = Text;
                Debugger.Break();
            }
            return brush_tuples.ToArray();
        }

        private bool CanMarquee() {
            return
                IsMarqueeEnabled &&
                _ft != null &&
                this.IsVisible &&
                _ftSize.Width - TailPadding > GetRenderWidth();
        }

        private double GetRenderWidth() {
            if (!this.IsVisible) {
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

                //double bmp_width = _marqueeBitmap.Size.Width;
                //double bmp_width = _bmpSize.Width;
                double bmp_width = _ftSize.Width;

                var cmp = _tb_mp == null ? new MpPoint() : _tb_mp;
                bool isReseting = _tb_mp == null || !new MpRect(MpPoint.Zero, this.Bounds.Size.ToPortableSize()).Contains(cmp);

                double max_width = Math.Max(1, this.Bounds.Width); //GetRenderWidth();
                double velMultiplier = isReseting ? 1.0 : Math.Min(1.0, Math.Max(0.1, cmp.X / max_width));

                if (AutoMarquee) {
                    if (_tb_mp != null) {
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
                        Dispatcher.UIThread.Post(InvalidateVisual);

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

                Dispatcher.UIThread.Post(InvalidateVisual);

                await Task.Delay(_delayMs);
            }
        }

        private MpSize GetScaledTextSize(out MpSize ftSize) {
            ftSize = MpSize.Empty;

            if (string.IsNullOrEmpty(Text)) {
                return MpSize.Empty;
            }

            _ft = this.ToFormattedText();
            if (NavigateUri != null) {
                _ft.SetTextDecorations(TextDecorations.Underline);
            }
            ftSize = new MpSize(_ft.Width + TailPadding, _ft.Height);

            double pixelsPerDip = MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling;

            double w = Math.Max(1.0, _ft.Width * pixelsPerDip) + TailPadding + Math.Abs(DropShadowOffset.X);
            double h = Math.Max(1.0, _ft.Height * pixelsPerDip) + Math.Abs(DropShadowOffset.Y);
            return new MpSize(w, h);
        }
        #endregion
    }
}
