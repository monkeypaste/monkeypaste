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
    /// Interaction logic for MpAnalyticItemParameterView.xaml
    /// </summary>
    public partial class MpAnalyticItemParameterView : MpUserControl<MpAnalyticItemParameterViewModel> {
        public MpAnalyticItemParameterView() {
            InitializeComponent();
        }

        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;            

            foreach(var aipvvm in (BindingContext as MpListBoxParameterViewModel).Items) {
                aipvvm.IsSelected = lb.SelectedItems.Contains(aipvvm);
            }
        }

        private void lstBox_Loaded(object sender, RoutedEventArgs e) {
            //var lb = sender as ListBox;
            //foreach(var svvm in (BindingContext as MpMultiSelectComboBoxParameterViewModel).SelectedViewModels) {
            //    lb.SelectedItems.Add(svvm);
            //}
        }

        private void MultiSelectComboBox_Loaded(object sender, RoutedEventArgs e) {
            var mscb = sender as ComboBox;
            foreach (var aipvvm in (BindingContext as MpListBoxParameterViewModel).SelectedItems) {
                ((ListBox)mscb.Template.FindName("lstBox", mscb)).SelectedItems.Add(aipvvm);
            }
        }
    }
}
