using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cursor = Avalonia.Input.Cursor;
using Key = Avalonia.Input.Key;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace MonkeyPaste.Avalonia {
    public interface MpIOverrideRender {
        bool IgnoreRender { get; set; }
    }
    public interface MpITextDocumentContainer {
        MpTextRange ContentRange { get; }
    }

    [DoNotNotify]
    public class MpAvMarqueeTextBox : TextBox, MpIOverrideRender, MpITextDocumentContainer {

        #region Private Variables
        private bool _unloaded;
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

        #endregion

        #region Interfaces

        #region MpITextDocumentContainer Implementation
        private MpTextRange _contentRange;
        public MpTextRange ContentRange {
            get {
                if (_contentRange == null) {
                    _contentRange = new MpTextRange(this);
                }
                return _contentRange;
            }
        }
        #endregion

        #region MpIOverrideRender Implementation
        public bool IgnoreRender { get; set; }
        #endregion

        #endregion

        #region Properties

        #region Overrides
        protected override Type StyleKeyOverride => typeof(TextBox);
        #endregion


        #region ReadOnlyForeground AvaloniaProperty

        public IBrush ReadOnlyForeground {
            get { return GetValue(ReadOnlyForegroundProperty); }
            set { SetValue(ReadOnlyForegroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> ReadOnlyForegroundProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(
                name: nameof(ReadOnlyForeground),
                defaultValue: Brushes.White); //Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString()));

        #endregion

        #region ReadOnlyBackground AvaloniaProperty

        private IBrush _readOnlyBackground = Brushes.Transparent;
        public IBrush ReadOnlyBackground {
            get => _readOnlyBackground;
            set => SetAndRaise(ReadOnlyBackgroundProperty, ref _readOnlyBackground, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> ReadOnlyBackgroundProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(ReadOnlyBackground),
                o => o.ReadOnlyBackground,
                (o, v) => o.ReadOnlyBackground = v,
                Brushes.Transparent
            );
        #endregion

        #region EditableForeground AvaloniaProperty

        private IBrush _editableForeground = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor.ToString());
        public IBrush EditableForeground {
            get => _editableForeground;
            set => SetAndRaise(EditableForegroundProperty, ref _editableForeground, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> EditableForegroundProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(EditableForeground),
                o => o.EditableForeground,
                (o, v) => o.EditableForeground = v,
                Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor.ToString())
            );

        #endregion

        #region EditableBackground AvaloniaProperty

        private IBrush _editableBackground = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString());
        public IBrush EditableBackground {
            get => _editableBackground;
            set => SetAndRaise(EditableBackgroundProperty, ref _editableBackground, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> EditableBackgroundProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(EditableBackground),
                o => o.EditableBackground,
                (o, v) => o.EditableBackground = v,
                Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString())
            );

        #endregion

        #region DropShadowBrush AvaloniaProperty

        public IBrush DropShadowBrush {
            get { return GetValue(DropShadowBrushProperty); }
            set { SetValue(DropShadowBrushProperty, value); }
        }

        public static readonly StyledProperty<IBrush> DropShadowBrushProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, IBrush>(
                name: nameof(DropShadowBrush),
                defaultValue: Brushes.Black); //Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor.ToString()));

        #endregion

        #region HoverBrush AvaloniaProperty

        private IBrush _hoverBrush = null;
        public IBrush HoverBrush {
            get => _hoverBrush;
            set => SetAndRaise(HoverBrushProperty, ref _hoverBrush, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> HoverBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(HoverBrush),
                o => o.HoverBrush,
                (o, v) => o.HoverBrush = v,
                null
            );
        #endregion

        #region NavigatedBrush AvaloniaProperty

        private IBrush _NavigatedBrush =
            Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeCompliment4DarkColor.ToString());
        public IBrush NavigatedBrush {
            get => _NavigatedBrush;
            set => SetAndRaise(NavigatedBrushProperty, ref _NavigatedBrush, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> NavigatedBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(NavigatedBrush),
                o => o.NavigatedBrush,
                (o, v) => o.NavigatedBrush = v,
                Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeCompliment4DarkColor.ToString())
            );
        #endregion

        #region ActiveHighlightBrush AvaloniaProperty

        private IBrush _activeHighlightBrush = MpAvHighlightTextExtension.DefaultActiveHighlightBrush;
        public IBrush ActiveHighlightBrush {
            get => _activeHighlightBrush;
            set => SetAndRaise(ActiveHighlightBrushProperty, ref _activeHighlightBrush, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> ActiveHighlightBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(ActiveHighlightBrush),
                o => o.ActiveHighlightBrush,
                (o, v) => o.ActiveHighlightBrush = v,
                Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeAccent3Color.ToString())
            );
        #endregion

        #region InactiveHighlightBrush AvaloniaProperty
        private IBrush _inactiveHighlightBrush = MpAvHighlightTextExtension.DefaultInactiveHighlightBrush;
        public IBrush InactiveHighlightBrush {
            get => _inactiveHighlightBrush;
            set => SetAndRaise(InactiveHighlightBrushProperty, ref _inactiveHighlightBrush, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, IBrush> InactiveHighlightBrushProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, IBrush>
            (
                nameof(InactiveHighlightBrush),
                o => o.InactiveHighlightBrush,
                (o, v) => o.InactiveHighlightBrush = v,
                Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeAccent1BgColor.ToString())
            );
        #endregion

        #region DropShadowOffset AvaloniaProperty

        public Point DropShadowOffset {
            get { return GetValue(DropShadowOffsetProperty); }
            set { SetValue(DropShadowOffsetProperty, value); }
        }

        public static readonly StyledProperty<Point> DropShadowOffsetProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, Point>(
                name: nameof(DropShadowOffset),
                defaultValue: new Point(1, 1));

        #endregion        

        #region TailPadding AvaloniaProperty

        private double _tailPadding = 30.0d;
        public double TailPadding {
            get => _tailPadding;
            set => SetAndRaise(TailPaddingProperty, ref _tailPadding, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, double> TailPaddingProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, double>
            (
                nameof(TailPadding),
                o => o.TailPadding,
                (o, v) => o.TailPadding = v,
                30.0d
            );
        #endregion

        #region MaxVelocity AvaloniaProperty

        private double _maxVelocity = -1.5d;
        public double MaxVelocity {
            get => _maxVelocity;
            set => SetAndRaise(MaxVelocityProperty, ref _maxVelocity, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, double> MaxVelocityProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, double>
            (
                nameof(MaxVelocity),
                o => o.MaxVelocity,
                (o, v) => o.MaxVelocity = v,
                -1.5
            );

        #endregion

        #region TotalLoopWaitMs AvaloniaProperty
        private int _totalLoopWaitMs = 1000;
        public int TotalLoopWaitMs {
            get => _totalLoopWaitMs;
            set => SetAndRaise(TotalLoopWaitMsProperty, ref _totalLoopWaitMs, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, int> TotalLoopWaitMsProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, int>
            (
                nameof(TotalLoopWaitMs),
                o => o.TotalLoopWaitMs,
                (o, v) => o.TotalLoopWaitMs = v,
                1000
            );
        #endregion

        #region EditOnFocus AvaloniaProperty

        public bool EditOnFocus {
            get { return GetValue(EditOnFocusProperty); }
            set { SetValue(EditOnFocusProperty, value); }
        }

        public static readonly StyledProperty<bool> EditOnFocusProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, bool>(
                name: nameof(EditOnFocus));

        #endregion

        #region EnableReadOnlyOnLostFocus AvaloniaProperty

        private bool _enableReadOnlyOnLostFocus = true;
        public bool EnableReadOnlyOnLostFocus {
            get => _enableReadOnlyOnLostFocus;
            set => SetAndRaise(EnableReadOnlyOnLostFocusProperty, ref _enableReadOnlyOnLostFocus, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, bool> EnableReadOnlyOnLostFocusProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, bool>
            (
                nameof(EnableReadOnlyOnLostFocus),
                o => o.EnableReadOnlyOnLostFocus,
                (o, v) => o.EnableReadOnlyOnLostFocus = v,
                true
            );
        #endregion

        #region BeginEditCommand AvaloniaProperty

        private ICommand _beginEditCommand;
        public ICommand BeginEditCommand {
            get => _beginEditCommand;
            set => SetAndRaise(BeginEditCommandProperty, ref _beginEditCommand, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, ICommand> BeginEditCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, ICommand>
            (
                nameof(BeginEditCommand),
                o => o.BeginEditCommand,
                (o, v) => o.BeginEditCommand = v,
                null
            );
        #endregion

        #region EndEditCommand AvaloniaProperty

        private ICommand _endEditCommand;
        public ICommand EndEditCommand {
            get => _endEditCommand;
            set => SetAndRaise(EndEditCommandProperty, ref _endEditCommand, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, ICommand> EndEditCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, ICommand>
            (
                nameof(EndEditCommand),
                o => o.EndEditCommand,
                (o, v) => o.EndEditCommand = v,
                null
            );
        #endregion

        #region CancelEditCommand AvaloniaProperty
        private ICommand _cancelEditCommand;
        public ICommand CancelEditCommand {
            get => _cancelEditCommand;
            set => SetAndRaise(CancelEditCommandProperty, ref _cancelEditCommand, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, ICommand> CancelEditCommandProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, ICommand>
            (
                nameof(CancelEditCommand),
                o => o.CancelEditCommand,
                (o, v) => o.CancelEditCommand = v,
                null
            );
        #endregion

        #region IsMarqueeEnabled AvaloniaProperty
        private bool _isMarqueeEnabled = true;
        public bool IsMarqueeEnabled {
            get => _isMarqueeEnabled;
            set => SetAndRaise(IsMarqueeEnabledProperty, ref _isMarqueeEnabled, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, bool> IsMarqueeEnabledProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, bool>
            (
                nameof(IsMarqueeEnabled),
                o => o.IsMarqueeEnabled,
                (o, v) => o.IsMarqueeEnabled = v,
                true
            );
        #endregion

        #region AutoMarquee AvaloniaProperty
        private bool _autoMarquee = false;
        public bool AutoMarquee {
            get => _autoMarquee;
            set => SetAndRaise(AutoMarqueeProperty, ref _autoMarquee, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, bool> AutoMarqueeProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, bool>
            (
                nameof(AutoMarquee),
                o => o.AutoMarquee,
                (o, v) => o.AutoMarquee = v,
                false
            );
        #endregion

        #region FocusOnDisableReadOnly AvaloniaProperty
        private bool _focusOnDisableReadOnly = false;
        public bool FocusOnDisableReadOnly {
            get => _focusOnDisableReadOnly;
            set => SetAndRaise(FocusOnDisableReadOnlyProperty, ref _focusOnDisableReadOnly, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, bool> FocusOnDisableReadOnlyProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, bool>
            (
                nameof(FocusOnDisableReadOnly),
                o => o.FocusOnDisableReadOnly,
                (o, v) => o.FocusOnDisableReadOnly = v,
                false
            );
        #endregion

        #region SelectViewModelOnFocus AvaloniaProperty
        private bool _selectViewModelOnFocus = true;
        public bool SelectViewModelOnFocus {
            get => _selectViewModelOnFocus;
            set => SetAndRaise(SelectViewModelOnFocusProperty, ref _selectViewModelOnFocus, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, bool> SelectViewModelOnFocusProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, bool>
            (
                nameof(SelectViewModelOnFocus),
                o => o.SelectViewModelOnFocus,
                (o, v) => o.SelectViewModelOnFocus = v,
                true
            );
        #endregion

        #region NavigateUriCommand AvaloniaProperty

        public ICommand NavigateUriCommand {
            get { return GetValue(NavigateUriCommandProperty); }
            set { SetValue(NavigateUriCommandProperty, value); }
        }

        public static readonly StyledProperty<ICommand> NavigateUriCommandProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, ICommand>(
                name: nameof(NavigateUriCommand),
                defaultValue: null);

        #endregion

        #region NavigateUriCommandParameter AvaloniaProperty
        public object NavigateUriCommandParameter {
            get { return GetValue(NavigateUriCommandParameterProperty); }
            set { SetValue(NavigateUriCommandParameterProperty, value); }
        }

        public static readonly StyledProperty<object> NavigateUriCommandParameterProperty =
            AvaloniaProperty.Register<MpAvMarqueeTextBox, object>(
                name: nameof(NavigateUriCommandParameter),
                defaultValue: null);
        #endregion

        #region NavigateUriRequiredKeyString AvaloniaProperty
        private string _NavigateUriRequiredKeyString = "Control";
        public string NavigateUriRequiredKeyString {
            get => _NavigateUriRequiredKeyString;
            set => SetAndRaise(NavigateUriRequiredKeyStringProperty, ref _NavigateUriRequiredKeyString, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, string> NavigateUriRequiredKeyStringProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, string>
            (
                nameof(NavigateUriRequiredKeyString),
                o => o.NavigateUriRequiredKeyString,
                (o, v) => o.NavigateUriRequiredKeyString = v,
                "Control"
            );
        #endregion

        #region Highlighting

        #region HighlightRanges AvaloniaProperty
        // FORMAT [index,count]
        private ObservableCollection<MpTextRange> _highlightRanges;
        public ObservableCollection<MpTextRange> HighlightRanges {
            get => _highlightRanges;
            set => SetAndRaise(HighlightRangesProperty, ref _highlightRanges, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, ObservableCollection<MpTextRange>> HighlightRangesProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, ObservableCollection<MpTextRange>>
            (
                nameof(HighlightRanges),
                o => o.HighlightRanges,
                (o, v) => o.HighlightRanges = v,
                null
            );
        #endregion

        #region ActiveHighlightIdx AvaloniaProperty
        private int? _activeHighlightIdx = null;
        public int? ActiveHighlightIdx {
            get => _activeHighlightIdx;
            set => SetAndRaise(ActiveHighlightIdxProperty, ref _activeHighlightIdx, value);
        }

        public static readonly DirectProperty<MpAvMarqueeTextBox, int?> ActiveHighlightIdxProperty =
            AvaloniaProperty.RegisterDirect<MpAvMarqueeTextBox, int?>
            (
                nameof(ActiveHighlightIdx),
                o => o.ActiveHighlightIdx,
                (o, v) => o.ActiveHighlightIdx = v,
                null
            );
        #endregion        

        #endregion

        #endregion

        public MpAvMarqueeTextBox() {
#if MOBILE
            this.Classes.Add("mobile");
#endif
            this.AcceptsReturn = false;
            this.AcceptsTab = false;
            this.TextWrapping = TextWrapping.NoWrap;
            this.ClipToBounds = true;
            this.BorderThickness = new Thickness(0);
            this.Background = Brushes.Transparent;
            //this.MinHeight = 5;
            this.IsReadOnly = true;
            if (!this.FontSize.IsNumber() || this.FontSize < 1) {
                this.FontSize = 1;
            }

            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            this.GetObservable(MpAvMarqueeTextBox.IsReadOnlyProperty).Subscribe(value => OnIsReadOnlyChanged());
            this.GetObservable(MpAvMarqueeTextBox.TextProperty).Subscribe(value => OnTextChanged());
            this.GetObservable(MpAvMarqueeTextBox.EditOnFocusProperty).Subscribe(value => OnCanEditChanged());

            this.GetObservable(MpAvMarqueeTextBox.MaxWidthProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ReadOnlyBackgroundProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ReadOnlyForegroundProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.DropShadowBrushProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.DropShadowOffsetProperty).Subscribe(value => Init());

            this.GetObservable(MpAvMarqueeTextBox.HighlightRangesProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ActiveHighlightIdxProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.ActiveHighlightBrushProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.InactiveHighlightBrushProperty).Subscribe(value => Init());
            this.GetObservable(MpAvMarqueeTextBox.FontSizeProperty).Subscribe(value => Init());

            this.AddHandler(MpAvMarqueeTextBox.KeyDownEvent, HandleKeyDown, RoutingStrategies.Tunnel);
            this.AddHandler(MpAvMarqueeTextBox.KeyUpEvent, HandleKeyUp, RoutingStrategies.Tunnel);
            this.AddHandler(MpAvMarqueeTextBox.HoldingEvent, HandleHold, RoutingStrategies.Tunnel);

        }

        #region Event Handlers
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            _unloaded = false;
        }
        protected override void OnUnloaded(RoutedEventArgs e) {
            base.OnUnloaded(e);
            _unloaded = true;
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            if (AutoMarquee) {
                Init();
                AnimateAsync().FireAndForgetSafeAsync();
            }
        }
        protected override void OnGotFocus(GotFocusEventArgs e) {
            base.OnGotFocus(e);
            //if (DataContext is MpISelectableViewModel svm &&
            //    SelectViewModelOnFocus) {
            //    svm.IsSelected = true;
            //}
            if (EditOnFocus) {
                SetValue(IsReadOnlyProperty, false);
                BeginEditCommand?.Execute(null);
            }
        }

        protected override void OnLostFocus(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLostFocus(e);

            if (EnableReadOnlyOnLostFocus) {

                if (ContextMenu != null && ContextMenu.IsOpen) {
                    // when context menu opens keep editable,
                    // and if not refocused on closed put back to readonly
                    void OnContextMenuClosed(object sender, RoutedEventArgs e) {
                        if (sender is not ContextMenu cm) {
                            return;
                        }
                        cm.Closed -= OnContextMenuClosed;
                        if (!this.IsKeyboardFocusWithin) {
                            SetValue(IsReadOnlyProperty, true);
                        }
                    }
                    ContextMenu.Closed += OnContextMenuClosed;
                    return;
                }
                SetValue(IsReadOnlyProperty, true);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
        }

        protected override void OnMeasureInvalidated() {
            base.OnMeasureInvalidated();
            Init();
        }
        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            if (NavigateUriCommand != null &&
                IsReadOnly) {
                if (NavigateUriCommand.CanExecute(NavigateUriCommandParameter)) {
                    this.Cursor = new Cursor(StandardCursorType.Hand);
                    if (!string.IsNullOrEmpty(NavigateUriRequiredKeyString)) {
                        string tt_prefix = string.IsNullOrEmpty(NavigateUriRequiredKeyString) ?
                        string.Empty :
                        $"[{NavigateUriRequiredKeyString}] + ";
                        string tt_text = $"{tt_prefix}Click to follow...";
                        ToolTip.SetTip(this, new MpAvToolTipView() {
                            ToolTipText = tt_text
                        });
                    }
                } else {
                    this.Cursor = new Cursor(StandardCursorType.No);
                }

                this.Redraw();
            }
            _tb_mp = e.GetClientMousePoint(this);
            AnimateAsync().FireAndForgetSafeAsync();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);

            if (e.IsLeftPress(this)) {
                if (e.ClickCount == 2 && !IsReadOnly) {
                    // BUG for some reason dbl click/select all doesn't work so handling here
                    SelectAll();
                    return;
                }
                if (NavigateUriCommand != null &&
                    IsReadOnly) {
                    var req_keys =
                    Mp.Services.KeyConverter.ConvertStringToKeySequence<Key>(NavigateUriRequiredKeyString);
                    if (e.KeyModifiers.HasAllFlags(req_keys.ToAvKeyModifiers())) {
                        NavigateUriCommand?.Execute(NavigateUriCommandParameter);
                    } else {
                        MpConsole.WriteLine($"Cannot exec nav cmd w/ param '{NavigateUriCommandParameter}'. Mods '{NavigateUriRequiredKeyString}' not pressed.");
                    }
                }

            } else if (e.IsRightPress(this) && !IsReadOnly) {
                // BUG suppressing context menu on marquee, weird bugs
                // 1. it'll loose focus and go back to read only
                // 2. on tag name it shows a blank menu
                e.Handled = true;
            }

        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (e.Handled || !e.IsLeftRelease(this)) {
                return;
            }

            if (DataContext is MpISelectableViewModel svm) {
                if (svm is MpIConditionalSelectableViewModel csvm && csvm.CanSelect ||
                    svm is not MpIConditionalSelectableViewModel) {
                    svm.IsSelected = true;
                }
            }
        }
        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            this.Cursor = new Cursor(StandardCursorType.Arrow);
            if (NavigateUriCommand != null && IsReadOnly) {
                this.Redraw();
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
                //await Task.Delay(300);

                if (IsReadOnly) {
                    await this.TryKillFocusAsync();
                } else {
                    _orgText = Text;
                    if (FocusOnDisableReadOnly && !IsKeyboardFocusWithin) {
                        bool success = await this.TrySetFocusAsync(focusMethod: NavigationMethod.Pointer);
                        if (!success) {
                            //MpDebug.Break("Focus error");
                        }
                    }
                    //await Task.Delay(300);
                    SelectAll();
                    if (!IsFocused) {
                        Focus(NavigationMethod.Pointer);
                    }
                }
                Init();
            });
        }

        public override void Render(DrawingContext context) {
            if (IgnoreRender || _unloaded) {
                // BUG workaround for 'https://github.com/AvaloniaUI/Avalonia/issues/10057'
                return;
            }
            if (context.IsUnsetValue() || !IsEffectivelyVisible) {
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

        private void HandleHold(object sender, HoldingRoutedEventArgs e) {
            //e.Handled = IsReadOnly;
        }
        private void HandleKeyDown(object sender, KeyEventArgs e) {
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
            if (_unloaded) {
                return;
            }
            ContentRange.Count = Text == null ? 0 : Text.Length;

            _bmpSize = GetScaledTextSize(out _ftSize);
            _offsetX1 = 0;
            if (CanMarquee()) {
                _offsetX2 = _bmpSize.Width + TailPadding;
            } else {
                _offsetX2 = 0;
            }
            MinHeight = _ftSize.Height;
            if (this.GetVisualAncestor<MpAvTagTrayView>() != null &&
                this.GetVisualAncestor<MpAvTagView>() is MpAvTagView tv &&
                tv.FindControl<Control>("TagCountContainer") is Control count_border &&
                this.DataContext is MpAvTagTileViewModel ttvm) {
                //tv.Width = count_border.Bounds.Width +
                //    Math.Max(this.MinWidth, Math.Min(Math.Min(_ftSize.Width, _bmpSize.Width), this.MaxWidth));
                ttvm.TagNameWidth = _ftSize.Width;

            }
            this.Redraw();
        }
        private void SetTextBoxIsVisible(bool isTextBoxVisible) {
            Dispatcher.UIThread.Post(() => {
                if (!VisualChildren.Any()) {
                    VisualChildren.Add(new TextBox());
                }
                foreach (var c in VisualChildren) {
                    //if (c is TextBox tb && FocusOnDisableReadOnly && isTextBoxVisible) {
                    //    // if textbox is new need to wait for it to show up to focus it
                    //    async void Tb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
                    //        if (!this.IsLoaded) {
                    //            return;
                    //        }
                    //        await tb.TrySetFocusAsync(NavigationMethod.Pointer);
                    //        tb.SelectAll();
                    //        tb.AttachedToVisualTree -= Tb_AttachedToVisualTree;

                    //    }
                    //    if (!was_added) {
                    //        Tb_AttachedToVisualTree(tb, null);
                    //    } else {
                    //        tb.AttachedToVisualTree += Tb_AttachedToVisualTree;
                    //    }

                    //}
                    c.IsVisible = isTextBoxVisible;
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
            if (_unloaded) {
                return;
            }
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
                } else if (NavigateUriCommand != null) {
                    fg = NavigatedBrush;
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
                MpDebug.Break();
            }
            return brush_tuples.ToArray();
        }

        private bool CanMarquee() {
            return
                !_unloaded &&
                IsMarqueeEnabled &&
                _ft != null &&
                this.IsVisible &&
                _ftSize.Width - TailPadding > GetRenderWidth();
        }

        private double GetRenderWidth() {
            if (Text == "All" &&
                this.GetVisualAncestor<MpAvTagTrayView>() != null) {

            }
            if (!this.IsVisible) {
                return 0;
            }

            if (this.Bounds.Width.HasValue()) {
                return this.Bounds.Width;
            }
            if (this.MaxWidth.HasValue()) {
                return this.MaxWidth;
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
                        this.Redraw();

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
                if (IsEffectivelyVisible) {
                    this.Redraw();
                }

                await Task.Delay(_delayMs);
            }
        }

        private MpSize GetScaledTextSize(out MpSize ftSize) {
            ftSize = MpSize.Empty;

            if (string.IsNullOrEmpty(Text)) {
                return MpSize.Empty;
            }

            _ft = this.ToFormattedText();
            if (NavigateUriCommand != null) {
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
