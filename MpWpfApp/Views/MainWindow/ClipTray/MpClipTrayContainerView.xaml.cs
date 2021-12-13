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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTrayContainerView.xaml
    /// </summary>
    public partial class MpClipTrayContainerView : MpUserControl<MpClipTrayViewModel> {
        public MpClipTrayContainerView() {
            InitializeComponent();
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeWE;
        }

        private void GridSplitter_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void ClipTraySplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(ClipTraySplitter.IsEnabled) {
                //pin tray has items
                GridLength pinColWidth = ClipTrayContainerGrid.ColumnDefinitions[0].Width;
                if(pinColWidth.IsAuto) {
                    //is default, collapsed so pop it out to show one item
                    pinColWidth = new GridLength(MpClipTileViewModel.DefaultBorderWidth, GridUnitType.Pixel);
                } else {
                    pinColWidth = new GridLength(ClipTraySplitter.ActualWidth, GridUnitType.Pixel);
                }
                ClipTrayContainerGrid.ColumnDefinitions[0].Width = pinColWidth;
            }
        }
    }
}
