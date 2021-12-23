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
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAnalyticItemPresetParameterListBoxView : MpUserControl<MpAnalyticItemViewModel> {
        public MpAnalyticItemPresetParameterListBoxView() {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            this.GetVisualAncestor<MpManageAnalyticItemsContainerView>().Close(false);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.GetVisualAncestor<MpManageAnalyticItemsContainerView>().Close(true);
        }
    }
}
