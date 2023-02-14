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

    public partial class MpAvSliderParameterView : MpAvUserControl<MpAvSliderParameterViewModel> {
        private MpPoint _lastMousePosition;
        private bool _isSliding = false;
        private double _oldVal = 0;

        public MpAvSliderParameterView() {
            InitializeComponent();

            var sb = this.FindControl<Border>("SliderBorder");
            sb.EffectiveViewportChanged += Sb_EffectiveViewportChanged;
            sb.PointerPressed += Sb_PointerPressed;
            sb.PointerReleased += Sb_PointerReleased;
            sb.PointerMoved += Sb_PointerMoved;

            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            svtb.GotFocus += Svtb_GotFocus;
            svtb.LostFocus += Svtb_LostFocus;
            svtb.KeyDown += Svtb_KeyDown;
            svtb.GetObservable(TextBox.TextProperty).Subscribe(value => OnSliderTextChanged());
        }

        private void OnSliderTextChanged() {
            UpdateRectWidth();
        }

        private void Sb_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            UpdateRectWidth();
        }

        private void Sb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {

            Dispatcher.UIThread.Post(async () => {
                if (!IsEnabled) {
                    return;
                }
                var sb = sender as Control;
                var svtb = this.FindControl<TextBox>("SliderValueTextBox");
                var tbl = svtb.TranslatePoint(new Point(), sb).Value;
                var tbr = new Rect(tbl, new Size(svtb.Bounds.Width, svtb.Bounds.Height));

                var mp = e.GetPosition(sb);

                if (tbr.Contains(mp)) {
                    return;
                }

                await svtb.TryKillFocusAsync();

                e.Pointer.Capture(sb);
                _isSliding = e.Pointer.Captured != null;
                if (_isSliding) {
                    _lastMousePosition = new MpPoint(mp.X, mp.Y); // mp.ToPortablePoint();

                    e.Handled = true;
                    var sbr = new Rect(new Point(), sb.Bounds.Size);
                    if (sbr.Contains(mp)) {
                        double newWidth = mp.X;
                        double widthPercent = newWidth / sb.Bounds.Width;
                        double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
                        BindingContext.CurrentValue = Math.Round(newValue, BindingContext.Precision).ToString();
                    }
                    UpdateRectWidth();
                }
            });

        }


        private void Sb_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (!_isSliding) {
                return;
            }
            var sb = sender as Control;

            if (e.Pointer.Captured == null) {
                _isSliding = false;
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
            BindingContext.CurrentValue = Math.Round(newValue, BindingContext.Precision).ToString();

            _lastMousePosition = new MpPoint(mp.X, mp.Y); //mp.ToPortablePoint();
            UpdateRectWidth();
        }

        private void Sb_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {

            var sb = sender as Control;
            if (_isSliding) {
                //SliderValueTextBox.IsHitTestVisible = true;
                _isSliding = false;
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
            Dispatcher.UIThread.Post(async () => {
                var svtb = sender as TextBox;
                if (e.Key == Key.Enter) {
                    // trigger lost focus
                    //this.FindControl<Border>("SliderBorder").Focus();
                    svtb.TryKillFocusAsync();
                    return;
                }
                if (e.Key == Key.Escape) {
                    // avoid breaking the binding?
                    TextBox.TextProperty.Setter.Invoke(svtb, _oldVal.ToString());
                    // trigger lost focus
                    //this.FindControl<Border>("SliderBorder").Focus();
                    svtb.TryKillFocusAsync();
                }
            });

        }

        private void OnSliderValueTextBoxValueChanged() {
            if (BindingContext == null) {
                return;
            }
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            if (double.TryParse(svtb.Text, out var dblVal)) {
                BindingContext.CurrentValue = dblVal.ToString();
                _oldVal = dblVal;
                UpdateRectWidth();
            } else {
                if (!_oldVal.IsNumber()) {
                    _oldVal = 0;
                }
                // avoid breaking the binding?
                TextBox.TextProperty.Setter.Invoke(svtb, _oldVal.ToString());
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
