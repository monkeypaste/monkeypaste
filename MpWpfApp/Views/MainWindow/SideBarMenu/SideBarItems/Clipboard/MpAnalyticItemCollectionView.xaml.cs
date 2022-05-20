using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MpAnalyticItemCollectionView.xaml
    /// </summary>
    public partial class MpAnalyticItemCollectionView : MpUserControl<MpAnalyticItemCollectionViewModel> {
        public MpAnalyticItemCollectionView() {
            InitializeComponent();
        }

        private void AnalyticRootItemBorder_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = true;
        }

        private void AnalyticRootItemBorder_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = false;
        }
    }
}
