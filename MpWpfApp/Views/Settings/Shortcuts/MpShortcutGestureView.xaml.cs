using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpShortcutItemView.xaml
    /// </summary>
    public partial class MpShortcutGestureView : MpUserControl<MpIShortcutCommand> {
        private DispatcherTimer _timer;

        public MpShortcutGestureView() {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            AssignButton.Visibility = Visibility.Visible;
            StartAnimation();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            if (!BindingContext.ShortcutViewModel.IsEmpty) {
                AssignButton.Visibility = Visibility.Hidden;
            }
            StopAnimation();
        }

        private void StartAnimation() {
            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(0.5);
                _timer.Tick += _timer_Tick;
            }
            _timer.Start();
        }

        private void StopAnimation() {
            _timer.Stop();
            RecordEllipse.Visibility = Visibility.Visible;
        }

        private void _timer_Tick(object sender, EventArgs e) {
            if(RecordEllipse.Visibility == Visibility.Visible) {
                RecordEllipse.Visibility = Visibility.Hidden;
            } else {
                RecordEllipse.Visibility = Visibility.Visible;
            }
        }

    }
}
