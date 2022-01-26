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
    /// Interaction logic for MpActionTreeItemView.xaml
    /// </summary>
    public partial class MpActionTreeItemView : MpUserControl<MpActionViewModelBase> {
        public MpActionTreeItemView() {
            InitializeComponent();
        }

        private void AddChildActionButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
            var fe = sender as FrameworkElement;
            var cm = new MpContextMenuView();
            cm.DataContext = BindingContext.MenuItemViewModel;
            fe.ContextMenu = cm;
            fe.ContextMenu.PlacementTarget = this;
            fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            fe.ContextMenu.IsOpen = true;
        }

        private void AnalyzerPresetComboBox_Loaded(object sender, RoutedEventArgs e) {
            var items = MpAnalyticItemCollectionViewModel.Instance.AllPresets;

            ListCollectionView lcv = new ListCollectionView(items);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("ParentAnalyticItemId",new MpAnalyticItemIdToTitleConverter()));
            
            var cb = sender as ComboBox;
            cb.ItemsSource = lcv;
        }
    }
}
