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
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpLoaderNotificationVIew.xaml
    /// </summary>
    public partial class MpLoaderNotificationView : MpUserControl<MpLoaderNotificationViewModel> {
        private DispatcherTimer _updateTimer;

        public MpLoaderNotificationView() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if(_updateTimer == null) {
                _updateTimer = new DispatcherTimer();
                _updateTimer.Interval = TimeSpan.FromMilliseconds(100); _updateTimer.IsEnabled = true;
                _updateTimer.Tick += _updateTimer_Tick;
                _updateTimer.Start();
            }
        }

        private void _updateTimer_Tick(object sender, EventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.ProgressBarCurrentWidth));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Title));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Body));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Detail));
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e) {
            if(_updateTimer != null) {
                _updateTimer.Stop();
            }
        }
    }
}
