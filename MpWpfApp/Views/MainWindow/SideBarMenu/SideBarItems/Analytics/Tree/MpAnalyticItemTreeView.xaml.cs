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
    /// Interaction logic for MpAnalyticItemView.xaml
    /// </summary>
    public partial class MpAnalyticItemTreeView : MpUserControl<MpAnalyticItemViewModel> {
        public MpAnalyticItemTreeView() {
            InitializeComponent();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = false;
        }
    }
}
