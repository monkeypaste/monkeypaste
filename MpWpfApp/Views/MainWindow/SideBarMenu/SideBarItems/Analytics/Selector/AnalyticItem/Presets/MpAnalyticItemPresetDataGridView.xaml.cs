using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAnalyticItemPresetDataGridView : MpUserControl<MpAnalyticItemViewModel> {
        public MpAnalyticItemPresetDataGridView() {
            InitializeComponent();
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(sender is Panel p) {
                var tbb = p.GetVisualDescendent<TextBoxBase>();
                if(tbb.IsVisible) {
                    return;
                }
                e.Handled = true;
                var aipvm = tbb.DataContext as MpAnalyticItemPresetViewModel;
                if(aipvm == null) {
                    return;                
                }
                aipvm.IsLabelReadOnly = false;
            }
        }
    }
}
