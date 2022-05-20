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
    /// Interaction logic for MpClipboardHandlerItemSelectorView.xaml
    /// </summary>
    public partial class MpClipboardHandlerItemSelectorView : MpUserControl<MpClipboardHandlerCollectionViewModel> {
        public MpClipboardHandlerItemSelectorView() {
            InitializeComponent();
        }

        private void AnalyticItemChooserComboBox_DropDownOpened(object sender, EventArgs e) {
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
        }

        private void AnalyticItemChooserComboBox_DropDownClosed(object sender, EventArgs e) {
            MpMainWindowViewModel.Instance.IsShowingDialog = false;
        }
    }
}
