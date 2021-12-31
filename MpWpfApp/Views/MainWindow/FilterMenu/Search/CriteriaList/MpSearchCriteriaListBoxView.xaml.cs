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
    /// Interaction logic for MpSearchDetailView.xaml
    /// </summary>
    public partial class MpSearchCriteriaListBoxView : MpUserControl<MpSearchBoxViewModel> {
        public MpSearchCriteriaListBoxView() {
            InitializeComponent();
        }

        private void SearchCriteriaListBox_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            e.Handled = true;
        }

        private void SearchCriteriaListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
        }
    }
}
