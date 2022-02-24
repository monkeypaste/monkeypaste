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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSliderBorder.xaml
    /// </summary>
    public partial class MpSliderBorder : MpUserControl<MpISliderViewModel> {
        private Point _lastMousePosition;
        private bool _isSliding = false;
        private static Regex _ValueRegEx;
        public MpSliderBorder() {
            InitializeComponent();
        }


        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if(!IsEnabled) {
                return;
            }
            _isSliding = Mouse.Capture(SliderBorder);
            if(_isSliding) {
                _lastMousePosition = e.GetPosition(SliderBorder);
                SliderValueTextBox.IsHitTestVisible = false;
                e.Handled = true;
            }

        }

        private void Border_MouseMove(object sender, MouseEventArgs e) {
            if(!_isSliding) {
                return;
            }

            if (!SliderBorder.IsMouseCaptured) {
                _isSliding = false;
                return;
            }
            var mp = e.GetPosition(SliderBorder);
            double deltaX = mp.X - _lastMousePosition.X;

            double newWidth = SliderValueRectangle.Width + deltaX;
            newWidth = Math.Min(Math.Max(0, newWidth), SliderBorder.ActualWidth);

            double widthPercent = newWidth / SliderBorder.ActualWidth;
            double newValue = ((BindingContext.MaxValue - BindingContext.MinValue) * widthPercent) + BindingContext.MinValue;
            BindingContext.SliderValue = Math.Round(newValue, BindingContext.Precision);
            
            _lastMousePosition = mp;
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e) {
            if(_isSliding) {
                SliderValueTextBox.IsHitTestVisible = true;
                _isSliding = false;
                _lastMousePosition = new Point();
                if(SliderBorder.IsMouseCaptured) {
                    SliderBorder.ReleaseMouseCapture();
                }
            }
        }


        private void SliderValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (_ValueRegEx == null) {
                _ValueRegEx = new Regex(@"[^0-9.-]", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            }
            var keyStr = new KeyConverter().ConvertToString(e.Key);
            if(_ValueRegEx.IsMatch(keyStr)) {
                e.Handled = true;
            }
        }

        private void SliderValueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                BindingContext.SliderValue = Convert.ToDouble(SliderValueTextBox.Text);
            } catch { 
            }

            double percentFilled = BindingContext.SliderValue / (BindingContext.MaxValue - BindingContext.MinValue);
            SliderValueRectangle.Width = SliderBorder.ActualWidth * percentFilled;
        }
    }
}
