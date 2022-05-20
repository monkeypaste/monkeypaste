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
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpHandledClipboardFormatsItemPresetDataGridView : MpUserControl<MpClipboardHandlerItemViewModel> {
        public MpHandledClipboardFormatsItemPresetDataGridView() {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var cmb = sender as ComboBox;
            ClipboardFormatPresetDatagridContainer.DataContext = cmb.SelectedItem;
            ClipboardFormatPresetDatagrid.Items.Refresh();

            var cbhisv = this.GetVisualAncestor<MpClipboardHandlerItemSelectorView>();
            if(cbhisv == null) {
                return;
            }
            cbhisv.ClipboardFormatPresetParameterListBoxView.DataContext = ClipboardFormatPresetDatagrid.SelectedItem;
            cbhisv.ClipboardFormatPresetParameterListBoxView.ConfigurePresetListBox.Items.Refresh();
        }

        private void ClipboardFormatPresetDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var dg = sender as DataGrid;
            var cbhisv = this.GetVisualAncestor<MpClipboardHandlerItemSelectorView>();
            if (cbhisv == null) {
                return;
            }
            cbhisv.ClipboardFormatPresetParameterListBoxView.DataContext = dg.SelectedItem;
            cbhisv.ClipboardFormatPresetParameterListBoxView.ConfigurePresetListBox.Items.Refresh();
        }

        //private void Button_MouseEnter(object sender, MouseEventArgs e) {
        //    var pvm = (sender as FrameworkElement).DataContext as MpAnalyticItemPresetViewModel;
        //    if (pvm != null && pvm.IsDefault) {
        //        MpCursorStack.CurrentCursor = MpCursorType.Invalid;
        //    } else {
        //        MpCursorStack.CurrentCursor = MpCursorType.Default;
        //    }
        //}

        //private void Button_MouseLeave(object sender, MouseEventArgs e) {
        //    MpCursorStack.CurrentCursor = MpCursorType.Default;
        //}
    }
}
