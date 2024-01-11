using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvSliderParameterView : MpAvUserControl<MpISliderViewModel> {
        private MpPoint _lastMousePosition;
        private double _oldVal = 0;
        private List<IDisposable> _disposables = new();
        #region Properties

        #region IsSliding Property

        private bool _isSliding = false;

        public static readonly DirectProperty<MpAvSliderParameterView, bool> IsSlidingProperty =
            AvaloniaProperty.RegisterDirect<MpAvSliderParameterView, bool>
            (
                nameof(IsSliding),
                o => o.IsSliding,
                (o, v) => o.IsSliding = v,
                false
            );

        public bool IsSliding {
            get => _isSliding;
            set {
                SetAndRaise(IsSlidingProperty, ref _isSliding, value);
            }
        }

        #endregion

        #region HasTextInput Property

        private bool _hasTextInput = true;

        public static readonly DirectProperty<MpAvSliderParameterView, bool> HasTextInputProperty =
            AvaloniaProperty.RegisterDirect<MpAvSliderParameterView, bool>
            (
                nameof(HasTextInput),
                o => o.HasTextInput,
                (o, v) => o.HasTextInput = v,
                true
            );

        public bool HasTextInput {
            get => _hasTextInput;
            set {
                SetAndRaise(HasTextInputProperty, ref _hasTextInput, value);
            }
        }

        #endregion

        #region FlipTheme Property

        private bool _FlipTheme = true;

        public static readonly DirectProperty<MpAvSliderParameterView, bool> FlipThemeProperty =
            AvaloniaProperty.RegisterDirect<MpAvSliderParameterView, bool>
            (
                nameof(FlipTheme),
                o => o.FlipTheme,
                (o, v) => o.FlipTheme = v,
                true
            );

        public bool FlipTheme {
            get => _FlipTheme;
            set {
                SetAndRaise(FlipThemeProperty, ref _FlipTheme, value);
            }
        }

        #endregion


        #endregion

        public MpAvSliderParameterView() {
            InitializeComponent();

            var sb = this.FindControl<Border>("SliderBorder");
            sb.EffectiveViewportChanged += Sb_EffectiveViewportChanged;
            sb.AddHandler(PointerPressedEvent, Sb_PointerPressed, RoutingStrategies.Tunnel);
            sb.PointerReleased += Sb_PointerReleased;
            sb.PointerMoved += Sb_PointerMoved;

            var disp = this.GetObservable(MpAvSliderParameterView.HasTextInputProperty).Subscribe(value => OnHasTextInputChanged());
            _disposables.Add(disp);
        }
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (this.GetVisualAncestor<ListBoxItem>() is not { } lbi) {
                return;
            }
            var disp = lbi.GetObservable(Control.IsKeyboardFocusWithinProperty).Subscribe(value => OnParamLbiKeyboardFocusWithinChanged(lbi));
            _disposables.Add(disp);
        }
        protected override void OnUnloaded(RoutedEventArgs e) {
            base.OnUnloaded(e);
            _disposables.ForEach(x => x.Dispose());
        }
        private void OnParamLbiKeyboardFocusWithinChanged(ListBoxItem lbi) {
            if (!lbi.IsLoaded || !lbi.IsKeyboardFocusWithin) {
                if (lbi != null) {
                    lbi.KeyDown -= OnLbiKeyDown;
                }
                return;
            }
            void OnLbiKeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
                if (svtb.IsFocused) {
                    return;
                }
                double step = 10;
                double dir =
                    e.Key == Key.Right || e.Key == Key.Up ? 1 :
                    e.Key == Key.Left || e.Key == Key.Down ? -1 :
                    0;
                double amt = (BindingContext.MaxValue - BindingContext.MinValue) / step;
                double new_val = BindingContext.SliderValue + (amt * dir);
                SetSliderValue(new_val);

            }
            lbi.KeyDown += OnLbiKeyDown;
        }

        private void OnHasTextInputChanged() {
            if (HasTextInput) {
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
                svtb.IsVisible = true;
                svtb.GotFocus += Svtb_GotFocus;
                svtb.LostFocus += Svtb_LostFocus;
                svtb.AddHandler(KeyDownEvent, Svtb_KeyDown, RoutingStrategies.Tunnel);
                var disp = svtb.GetObservable(TextBox.TextProperty).Subscribe(value => OnSliderTextChanged());
                _disposables.Add(disp);
            } else {
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
                svtb.IsVisible = false;
                svtb.GotFocus -= Svtb_GotFocus;
                svtb.LostFocus -= Svtb_LostFocus;
                svtb.RemoveHandler(KeyDownEvent, Svtb_KeyDown);
            }
        }

        private void OnSliderTextChanged() {
            UpdateRectWidth();
        }

        private void Sb_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            UpdateRectWidth();
        }

        private async void Sb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (!IsEnabled) {
                return;
            }
            var sb = sender as Control;
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            if (svtb.IsKeyboardFocusWithin) {
                return;
            }

            var tbl = svtb.TranslatePoint(new Point(), sb).Value;
            var tbr = new Rect(tbl, new Size(svtb.Bounds.Width, svtb.Bounds.Height));

            var mp = e.GetPosition(sb);

            if (tbr.Contains(mp) && HasTextInput) {
                return;
            }

            await svtb.TryKillFocusAsync();

            e.Pointer.Capture(sb);
            IsSliding = e.Pointer.Captured != null;
            if (IsSliding) {
                _lastMousePosition = new MpPoint(mp.X, mp.Y); // mp.ToPortablePoint();

                e.Handled = true;
                var sbr = new Rect(new Point(), sb.Bounds.Size);
                if (sbr.Contains(mp)) {
                    double newWidth = mp.X;
                    double widthPercent = newWidth / sb.Bounds.Width;
                    if (BindingContext != null) {
                        double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
                        SetSliderValue(newValue);
                    }
                }
            }
        }


        private void Sb_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (!IsSliding) {
                return;
            }
            var sb = sender as Control;

            if (e.Pointer.Captured == null) {
                IsSliding = false;
                return;
            }
            double newWidth;

            var mp = e.GetPosition(sb);
            newWidth = Math.Clamp(mp.X, 0, sb.Bounds.Width);
            double widthPercent = newWidth / sb.Bounds.Width;
            double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
            SetSliderValue(newValue);
            _lastMousePosition = new MpPoint(mp.X, mp.Y);
        }

        private void Sb_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            IsSliding = false;
            _lastMousePosition = new MpPoint();
            e.Pointer.Capture(null);
            UpdateRectWidth();
        }

        private void Svtb_GotFocus(object sender, GotFocusEventArgs e) {
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            if (double.TryParse(svtb.Text, out var dblVal)) {
                _oldVal = dblVal;
            }
        }

        private void Svtb_LostFocus(object sender, RoutedEventArgs e) {
            OnSliderValueTextBoxValueChanged();
        }

        private void Svtb_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (sender is not TextBox svtb) {
                return;
            }
            if (e.Key == Key.Enter) {
                // trigger lost focus
                e.Handled = true;
                svtb.TryKillFocusAsync().FireAndForgetSafeAsync();
                return;
            }
            if (e.Key == Key.Escape) {
                svtb.Text = _oldVal.ToString();
                svtb.TryKillFocusAsync().FireAndForgetSafeAsync();
            }
        }

        private void OnSliderValueTextBoxValueChanged() {
            if (BindingContext == null) {
                return;
            }
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            if (double.TryParse(svtb.Text, out var dblVal)) {
                SetSliderValue(dblVal);
                _oldVal = dblVal;
            } else {
                if (!_oldVal.IsNumber()) {
                    _oldVal = 0;
                }
                svtb.Text = _oldVal.ToString();
                OnSliderValueTextBoxValueChanged();
            }
        }

        private void SetSliderValue(double newValue) {
            BindingContext.SliderValue = Math.Round(Math.Clamp(newValue, BindingContext.MinValue, BindingContext.MaxValue), BindingContext.Precision);
            UpdateRectWidth();
        }

        private void UpdateRectWidth() {
            if (BindingContext == null) {
                return;
            }
            var sb = this.FindControl<Border>("SliderBorder");
            var svr = this.FindControl<Rectangle>("SliderValueRectangle");
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            double percentFilled = BindingContext.SliderValue / (BindingContext.MaxValue - BindingContext.MinValue);
            svr.Width = sb.Bounds.Width * percentFilled;

            // BUG if width is changed outside ui, bounds
            // won't update immediatly, so using width since its left aligned (left is 0)
            FlipTheme = (svr.Bounds.Left + svr.Width) > svtb.Bounds.Left;
        }
    }
}
