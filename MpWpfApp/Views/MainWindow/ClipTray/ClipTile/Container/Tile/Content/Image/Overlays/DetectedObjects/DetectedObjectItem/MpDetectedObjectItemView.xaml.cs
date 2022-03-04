using MonkeyPaste;
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
    /// Interaction logic for MpDetectedObjectItemView.xaml
    /// </summary>
    public partial class MpDetectedObjectItemView : MpUserControl<MpImageAnnotationViewModel> {
        private static DispatcherTimer timer;

        public MpDetectedObjectItemView() {
            InitializeComponent();
        }


        private void ToolTipOpeningHandler(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.DisplayScore = 0;
            BindingContext.OnPropertyChanged(nameof(BindingContext.ValueCircle));
            timer.Tag = BindingContext;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var bc = (sender as DispatcherTimer).Tag as MpImageAnnotationViewModel;
            if (bc.DisplayScore < bc.Score) {
                bc.DisplayScore += 0.1;
            } else {
                bc.DisplayScore = bc.Score;
                timer.Stop();
            }
            bc.OnPropertyChanged(nameof(bc.ValueCircle));
        }

        private void DetectedImageObjectButton_Loaded(object sender, RoutedEventArgs e) {
            if(timer == null) {
                timer = new DispatcherTimer(DispatcherPriority.Normal);
                timer.Interval = TimeSpan.FromMilliseconds(25);

                timer.Tick += Timer_Tick;
            }
            ToolTipService.AddToolTipOpeningHandler(DetectedImageObjectButton, ToolTipOpeningHandler);
        }

        private void DetectedImageObjectButton_Unloaded(object sender, RoutedEventArgs e) {
            ToolTipService.RemoveToolTipOpeningHandler(DetectedImageObjectButton, ToolTipOpeningHandler);
        }
    }
}
