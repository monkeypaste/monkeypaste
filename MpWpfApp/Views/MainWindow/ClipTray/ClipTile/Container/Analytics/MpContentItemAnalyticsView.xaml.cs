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
    /// Interaction logic for MpContentItemAnalyticsView.xaml
    /// </summary>
    public partial class MpContentItemAnalyticsView : UserControl {
        public MpContentItemAnalyticsView() {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            MpClipTrayViewModel.Instance.FlipTileCommand.Execute(civm.Parent);
        }
    }
}
