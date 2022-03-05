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
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTooltipInfoView.xaml
    /// </summary>
    public partial class MpContentReportView : MpUserControl<MpAnalysisReportCollectionViewModel> {
        public MpContentReportView() {
            InitializeComponent();
        }

        private void tooltip_MouseLeave(object sender, MouseEventArgs e) {
            tooltip.IsOpen = false;
        }

        private void TooltipInfoBorder_MouseEnter(object sender, MouseEventArgs e) {
            tooltip.IsOpen = true;
        }

        private void TooltipInfoBorder_MouseLeave(object sender, MouseEventArgs e) {
            if(!tooltip.IsMouseOver) {
                tooltip.IsOpen = false;
            }
        }
    }
}
