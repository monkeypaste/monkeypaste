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
    /// Interaction logic for MpClipTileDetailView.xaml
    /// </summary>
    public partial class MpClipTileDetailView : UserControl {
        public MpClipTileDetailView() {
            InitializeComponent();
        }

        private void ClipTileDetailTextBlock_MouseEnter(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.CycleDetailCommand.Execute(null);
        }

        private void ClipTileDetailTextBlock_MouseLeave(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
        }
    }
}
