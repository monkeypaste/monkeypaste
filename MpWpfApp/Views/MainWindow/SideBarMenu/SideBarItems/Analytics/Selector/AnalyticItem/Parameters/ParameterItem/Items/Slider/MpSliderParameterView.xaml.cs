using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpSliderParameterView : MpUserControl<MpISliderViewModel> {
        private Point _lastMousePosition;
        private bool _isSliding = false;
        private static Regex _ValueRegEx;

        public MpSliderParameterView() {
            InitializeComponent();
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (!IsEnabled) {
                return;
            }
            var tbl = SliderValueTextBox.TranslatePoint(new Point(), SliderBorder);
            var tbr = new Rect(tbl, new Size(SliderValueTextBox.ActualWidth, SliderValueTextBox.ActualHeight));

            var mp = e.GetPosition(SliderBorder);

            if (tbr.Contains(mp)) {
                return;
            }
            SliderValueTextBox.KillFocus();

            _isSliding = Mouse.Capture(SliderBorder);
            if (_isSliding) {
                _lastMousePosition = mp;
                //SliderValueTextBox.IsHitTestVisible = false;
                e.Handled = true;
                var sbr = new Rect(new Point(), SliderBorder.RenderSize);
                if (sbr.Contains(mp)) {
                    double newWidth = mp.X;
                    double widthPercent = newWidth / SliderBorder.ActualWidth;
                    double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
                    BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);
                }
            }
        }

        private void Border_MouseMove(object sender, MouseEventArgs e) {
            if (!_isSliding) {
                return;
            }

            if (!SliderBorder.IsMouseCaptured) {
                _isSliding = false;
                return;
            }
            double newWidth;

            var mp = e.GetPosition(SliderBorder);
            var sbr = new Rect(new Point(), SliderBorder.RenderSize);
            if (sbr.Contains(mp)) {
                newWidth = mp.X;
            } else {
                double deltaX = mp.X - _lastMousePosition.X;

                newWidth = SliderValueRectangle.ActualWidth + deltaX;
                newWidth = Math.Min(Math.Max(0, newWidth), SliderBorder.ActualWidth);
            }

            double widthPercent = newWidth / SliderBorder.ActualWidth;
            double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
            BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);

            _lastMousePosition = mp;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e) {
            if (_isSliding) {
                //SliderValueTextBox.IsHitTestVisible = true;
                _isSliding = false;
                _lastMousePosition = new Point();
                if (SliderBorder.IsMouseCaptured) {
                    SliderBorder.ReleaseMouseCapture();
                }
            }
        }


        private void SliderValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (_ValueRegEx == null) {
                _ValueRegEx = new Regex(@"[^0-9.-]", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }
            var keyStr = new KeyConverter().ConvertToString(e.Key);
            if (_ValueRegEx.IsMatch(keyStr) && e.Key != Key.Delete && e.Key != Key.Back) {
                e.Handled = true;
            }
        }

        private void SliderValueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                BindingContext.SliderValue = Convert.ToDouble(SliderValueTextBox.Text);
            }
            catch {
            }

            double percentFilled = BindingContext.SliderValue / (BindingContext.MaxValue - BindingContext.MinValue);
            SliderValueRectangle.Width = SliderBorder.ActualWidth * percentFilled;
        }
    }
}
