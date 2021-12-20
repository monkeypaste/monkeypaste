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
using System.Windows.Shapes;

namespace MpWpfApp {

    public partial class MpManageAnalyticItemModalWindow : MpWindow<MpAnalyticItemCollectionViewModel> {
        public MpManageAnalyticItemModalWindow() {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e) {            
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) {
            var aivm = BindingContext.SelectedItem;
            var pvm = aivm.SelectedPresetViewModel;
            if(pvm.IsDefault) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Invalid;
            } else {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void AnalyticItemChooserComboBox_Loaded(object sender, RoutedEventArgs e) {
            AnalyticItemChooserComboBox.SelectedItem = BindingContext.SelectedItem;//.Items.IndexOf(BindingContext.SelectedItem);
            return;
        }

        private void AnalyticItemChooserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            BindingContext.Items.ForEach(x => x.IsSelected = BindingContext.Items.IndexOf(x) == AnalyticItemChooserComboBox.SelectedIndex);
        }
    }
}
