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
    /// Interaction logic for MpPasteToAppPathDataGridView.xaml
    /// </summary>
    public partial class MpPasteToAppPathDataGridView : MpUserControl<MpPasteToAppPathViewModelCollection> {
        public MpPasteToAppPathDataGridView() {
            InitializeComponent();
        }

        private void PasteToAppPathDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (var ptapvm in BindingContext.Items) {
                ptapvm.IsSelected = ptapvm == BindingContext.SelectedPasteToAppPathViewModel ? true : false;
            }
        }
    }
}
