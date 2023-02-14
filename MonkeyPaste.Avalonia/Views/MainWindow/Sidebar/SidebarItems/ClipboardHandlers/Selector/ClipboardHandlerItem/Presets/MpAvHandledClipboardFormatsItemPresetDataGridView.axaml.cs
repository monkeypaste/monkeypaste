using Avalonia.Markup.Xaml;
namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvHandledClipboardFormatsItemPresetDataGridView : MpAvUserControl<MpAvClipboardHandlerItemViewModel> {
        public MpAvHandledClipboardFormatsItemPresetDataGridView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var cmb = sender as ComboBox;
        //    ClipboardFormatPresetDatagridContainer.DataContext = cmb.SelectedItem;
        //    ClipboardFormatPresetDatagrid.Items.Refresh();

        //    var cbhisv = this.GetVisualAncestor<MpAvClipboardHandlerItemSelectorView>();
        //    if(cbhisv == null) {
        //        return;
        //    }
        //    cbhisv.ClipboardFormatPresetParameterListBoxView.DataContext = ClipboardFormatPresetDatagrid.SelectedItem;
        //    cbhisv.ClipboardFormatPresetParameterListBoxView.ConfigurePresetListBox.Items.Refresh();
        //}

        //private void ClipboardFormatPresetDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    var dg = sender as DataGrid;
        //    var cbhisv = this.GetVisualAncestor<MpAvClipboardHandlerItemSelectorView>();
        //    if (cbhisv == null) {
        //        return;
        //    }
        //    cbhisv.ClipboardFormatPresetParameterListBoxView.DataContext = dg.SelectedItem;
        //    cbhisv.ClipboardFormatPresetParameterListBoxView.ConfigurePresetListBox.Items.Refresh();
        //}

        //private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        //    if (sender is Panel p) {
        //        var tbb = p.GetVisualDescendent<TextBoxBase>();
        //        if (tbb.IsVisible) {
        //            return;
        //        }
        //        e.Handled = true;
        //        var cbfpvm = tbb.DataContext as MpAvClipboardFormatPresetViewModel;
        //        if (cbfpvm == null) {
        //            return;
        //        }
        //        cbfpvm.IsLabelReadOnly = false;
        //    }
        //}

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
