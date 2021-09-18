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
    /// Interaction logic for MpPasteTemplateToolbarView.xaml
    /// </summary>
    public partial class MpPasteTemplateToolbarView : UserControl {
        RichTextBox _activeRtb;

        public MpPasteTemplateToolbarView() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }
        public void SetActiveRtb(RichTextBox trtb) {
            _activeRtb = trtb;
            var rtbvm = _activeRtb.DataContext as MpRtbItemViewModel;
            foreach (var thlvm in rtbvm.TemplateHyperlinkCollectionViewModel.Templates) {
                thlvm.OnTemplateSelected += Thlvm_OnTemplateSelected;
            }
        }

        private void Thlvm_OnTemplateSelected(object sender, EventArgs e) {
            
        }
    }
}
