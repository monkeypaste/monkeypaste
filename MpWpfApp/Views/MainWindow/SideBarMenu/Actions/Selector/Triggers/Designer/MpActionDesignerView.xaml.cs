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
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpActionDesignerView.xaml
    /// </summary>
    public partial class MpActionDesignerView : MpUserControl<MpTriggerActionViewModelBase> {
        public MpActionDesignerView() {
            InitializeComponent();
        }

        private void ActionDesignerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;
            lb.ScrollIntoView(MpActionCollectionViewModel.Instance.PrimaryAction);
        }

        private void ZoomAndPanControl_Loaded(object sender, RoutedEventArgs e) {
            ZoomAndPanControl.ScaleToFit();
        }
    }
}
