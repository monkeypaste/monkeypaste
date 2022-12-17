

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvActionCollectionViewModel> {
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        //private void Button_PreviewMouseLeftButtonDown(object sender, PointerDownEventArgs e) {
        //    e.Handled = true;
        //    var fe = sender as Control;
        //    var cm = MpAvContextMenuView.Instance;
        //    cm.DataContext = BindingContext.ContextMenuItemViewModel;
        //    fe.ContextMenu = cm;
        //    fe.ContextMenu.PlacementTarget = this;
        //    fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
        //    fe.ContextMenu.IsOpen = true;
        //}

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    (sender as ComboBox).IsDropDownOpen = false;
        //}

        //private void DetailGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
        //    BindingContext.DesignerWidth = e.NewSize.Width;
        //    BindingContext.DesignerHeight = e.NewSize.Height;
        //}
    }
}
