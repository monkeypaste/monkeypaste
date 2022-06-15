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
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayContainerView.xaml
    /// </summary>
    public partial class MpClipTrayContainerView : MpUserControl<MpClipTrayViewModel> {
        private bool _isUserResizingPinTray = false;
        public MpClipTrayContainerView() {
            InitializeComponent();
        }

        private void ClipTraySplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(ClipTraySplitter.IsEnabled) {
                //pin tray has items
                GridLength pinColWidth = ClipTrayContainerGrid.ColumnDefinitions[0].Width;
                if(pinColWidth.IsAuto) {
                    //is default, collapsed so pop it out to show one item
                    //pinColWidth = new GridLength(MpClipTileViewModel.DefaultBorderWidth, GridUnitType.Pixel);
                    pinColWidth = new GridLength(MpMeasurements.Instance.ClipTileDefaultMinSize, GridUnitType.Pixel);
                } else {
                    pinColWidth = new GridLength(ClipTraySplitter.ActualWidth, GridUnitType.Pixel);
                }
                ClipTrayContainerGrid.ColumnDefinitions[0].Width = pinColWidth;
            }
        }

        private void ClipTraySplitter_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(!ClipTraySplitter.IsEnabled) {
                ClipTrayContainerGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
            }
        }

        private void ClipTraySplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
            BindingContext.HasUserAlteredPinTrayWidth = true;
            BindingContext.PinTrayTotalWidth =  PinTrayView.PinTrayListBox.GetVisualDescendent<ScrollViewer>().ExtentWidth;
            BindingContext.OnPropertyChanged(nameof(BindingContext.ClipTrayScreenWidth));
        }

    }
}
