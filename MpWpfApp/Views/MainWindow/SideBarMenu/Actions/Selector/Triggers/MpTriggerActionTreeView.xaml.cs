using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MpMatcherCollectionView.xaml
    /// </summary>
    public partial class MpTriggerActionTreeView : MpUserControl<MpActionCollectionViewModel> {
        public MpTriggerActionTreeView() {
            InitializeComponent();
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
            var fe = sender as FrameworkElement;
            var g = fe.GetVisualAncestor<TreeViewItem>();
            g.Height += e.VerticalChange;
        }
    }
}
