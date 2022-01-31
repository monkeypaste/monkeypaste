using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MpMatcherCollectionView.xaml
    /// </summary>
    public partial class MpTriggerActionTreeView : MpUserControl<MpActionCollectionViewModel> {
        private static bool test = false;
        public MpTriggerActionTreeView() {
            InitializeComponent();
        }

        private void CompareDataTextBox_Loaded(object sender, RoutedEventArgs e) {
            if(test) {
                return;
            }
            test = true;
            new ResizeAdorner(sender as TextBox, 3000, 3000);
        }
    }
}
