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
    /// Interaction logic for MpAnalyticToolbarTreeView.xaml
    /// </summary>
    public partial class MpSearchCriteriaItemView : MpUserControl<MpSearchCriteriaItemViewModel> {
        public MpSearchCriteriaItemView() {
            InitializeComponent();
        }

        private void AddCriteriaItemButton_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsOverAddCriteriaButton = true;
        }

        private void AddCriteriaItemButton_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsOverAddCriteriaButton = false;
        }

        private void RemoveCriteriaItemButton_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsOverRemoveCriteriaButton = true;
        }

        private void RemoveCriteriaItemButton_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsOverRemoveCriteriaButton = false;
        }

        private void CriteriaItemBorder_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = true;
        }

        private void CriteriaItemBorder_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsHovering = false;
        }

        private void ComboBox_GotFocus(object sender, RoutedEventArgs e) {

        }

        private void SearchCriteriaTextBox_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
        }

        private void SearchCriteriaTextBox_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void SearchCriteriaInputBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //if(SearchCriteriaTextBox.IsFocused) {
            //    e.Handled = false;
            //    return;
            //}
            //Keyboard.Focus(SearchCriteriaTextBox);
            SearchCriteriaTextBox.Focus();
            e.Handled = true;
        }
    }
}
