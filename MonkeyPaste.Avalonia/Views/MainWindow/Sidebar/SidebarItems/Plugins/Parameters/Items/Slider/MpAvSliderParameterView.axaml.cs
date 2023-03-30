using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
namespace MonkeyPaste.Avalonia {

    public partial class MpAvSliderParameterView : MpAvUserControl<MpISliderViewModel> {
        private MpPoint _lastMousePosition;
        private double _oldVal = 0;

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

        #endregion

        public MpAvSliderParameterView() {
            InitializeComponent();

            var sb = this.FindControl<Border>("SliderBorder");
            sb.EffectiveViewportChanged += Sb_EffectiveViewportChanged;
            sb.AddHandler(PointerPressedEvent, Sb_PointerPressed, RoutingStrategies.Tunnel);
            sb.PointerReleased += Sb_PointerReleased;
            sb.PointerMoved += Sb_PointerMoved;

            this.GetObservable(MpAvSliderParameterView.HasTextInputProperty).Subscribe(value => OnHasTextInputChanged());


        }

        private void OnHasTextInputChanged() {
            if (HasTextInput) {
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
                svtb.IsVisible = true;
                svtb.GotFocus += Svtb_GotFocus;
                svtb.LostFocus += Svtb_LostFocus;
                svtb.AddHandler(KeyDownEvent, Svtb_KeyDown, RoutingStrategies.Tunnel);
                svtb.GetObservable(TextBox.TextProperty).Subscribe(value => OnSliderTextChanged());
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

        private void Sb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {

            Dispatcher.UIThread.Post((Action)(async () => {
                if (!IsEnabled) {
                    return;
                }
                var sb = sender as Control;
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
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
                            BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);
                        }
                    }
                    UpdateRectWidth();
                }
            }));

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
            var sbr = new Rect(new Point(), sb.Bounds.Size);
            if (sbr.Contains(mp)) {
                newWidth = mp.X;
            } else {
                double deltaX = mp.X - _lastMousePosition.X;

                var svr = this.FindControl<Rectangle>("SliderValueRectangle");
                newWidth = svr.Bounds.Width + deltaX;
                newWidth = Math.Min(Math.Max(0, newWidth), sb.Bounds.Width);
            }

            double widthPercent = newWidth / sb.Bounds.Width;
            double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
            BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);

            _lastMousePosition = new MpPoint(mp.X, mp.Y);
            UpdateRectWidth();
        }

        private void Sb_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (IsSliding) {
                IsSliding = false;
                _lastMousePosition = new MpPoint();
                if (e.Pointer.Captured != null) {
                    e.Pointer.Capture(null);
                }
            }
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
            Dispatcher.UIThread.Post(() => {
                var svtb = sender as TextBox;
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
            });

        }

        private void OnSliderValueTextBoxValueChanged() {
            if (BindingContext == null) {
                return;
            }
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            if (double.TryParse(svtb.Text, out var dblVal)) {
                BindingContext.SliderValue = dblVal;
                _oldVal = dblVal;
                UpdateRectWidth();
            } else {
                if (!_oldVal.IsNumber()) {
                    _oldVal = 0;
                }
                svtb.Text = _oldVal.ToString();
                OnSliderValueTextBoxValueChanged();
            }
        }

        private void UpdateRectWidth() {
            if (BindingContext == null) {
                return;
            }
            var sb = this.FindControl<Border>("SliderBorder");
            var svr = this.FindControl<Rectangle>("SliderValueRectangle");

            double percentFilled = BindingContext.SliderValue / (BindingContext.MaxValue - BindingContext.MinValue);
            svr.Width = sb.Bounds.Width * percentFilled;
        }
    }
}
