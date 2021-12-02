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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : MpUserControl<MpClipTileViewModel> {
        public MpClipTileContainerView() {
            InitializeComponent();
        }

        private void Grid_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e) {
            //this.ClearBindings();
            //BindingContext.Dispose();
            ExpandBehavior.Detach();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            ExpandBehavior.Attach(this);
        }
    }
}
