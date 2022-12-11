using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Wpf;
namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvSliderParameterView : MpAvUserControl<MpISliderViewModel> {
        private MpPoint _lastMousePosition;
        private bool _isSliding = false;
        
        public MpAvSliderParameterView() {
            InitializeComponent();

            var sb = this.FindControl<Border>("SliderBorder");
            sb.PointerPressed += Sb_PointerPressed;
            sb.PointerReleased += Sb_PointerReleased;
            sb.PointerMoved += Sb_PointerMoved;


            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            svtb.KeyDown += Svtb_KeyDown;
            svtb.GetObservable(TextBox.TextProperty).Subscribe(value => OnSliderValueTextBoxValueChanged());
            
        }


        private void Sb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
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
            //svtb.KillFocus();

            e.Pointer.Capture(sb);
            _isSliding = e.Pointer.Captured != null;
            if (_isSliding) {
                _lastMousePosition = mp.ToPortablePoint();
                //svtb.IsHitTestVisible = false;
                e.Handled = true;
                var sbr = new Rect(new Point(), sb.Bounds.Size);
                if (sbr.Contains(mp)) {
                    double newWidth = mp.X;
                    double widthPercent = newWidth / sb.Bounds.Width;
                    double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
                    BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);
                }
            }
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
            BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);

            _lastMousePosition = mp.ToPortablePoint();
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
        }


        private void Svtb_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            var keyStr = MpAvKeyboardInputHelpers.GetKeyLiteral(e.Key);
            if (MpRegEx.RegExLookup[MpRegExType.Is_NOT_Number].IsMatch(keyStr) && 
                e.Key != Key.Delete && e.Key != Key.Back) {
                e.Handled = true;
            }
        }

        private void OnSliderValueTextBoxValueChanged() {
            if(BindingContext == null) {
                return;
            }
            var sb = this.FindControl<Border>("SliderBorder");
            var svtb = this.FindControl<TextBox>("SliderValueTextBox");
            var svr = this.FindControl<Rectangle>("SliderValueRectangle");
            try {
                BindingContext.SliderValue = Convert.ToDouble(svtb.Text);
            }
            catch {
            }

            double percentFilled = BindingContext.SliderValue / (BindingContext.MaxValue - BindingContext.MinValue);
            svr.Width = sb.Bounds.Width * percentFilled;
        }
    }
}
