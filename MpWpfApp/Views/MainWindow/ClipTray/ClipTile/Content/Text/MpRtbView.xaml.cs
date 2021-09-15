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
    /// Interaction logic for Mpxaml
    /// </summary>
    public partial class MpRtbView : UserControl {
        public MpRtbView() {
            InitializeComponent();            
        }
        private void Rtb_Loaded(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (rtbvm.HostClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.CaretPosition = Rtb.Document.ContentStart;
                Rtb.Document.TextAlignment = TextAlignment.Left;
                UpdateLayout();
            }
        }

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (rtbvm.IsEditingContent) {
                //rtbvm.HostClipTileViewModel.EditRichTextBoxToolbarViewModel.Rtb_SelectionChanged(rtbvm.Rtb, e3);
            }
        }
    }
}
