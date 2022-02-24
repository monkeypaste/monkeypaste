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
    /// Interaction logic for MpActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpActionPropertyListBoxItemView : MpUserControl<MpActionViewModelBase> {
        public MpActionPropertyListBoxItemView() {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            (sender as ComboBox).IsDropDownOpen = false;
            this.UpdateLayout();
            //this.GetVisualAncestor<ListBox>().Items.Refresh();
        }
    }
}
