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
    /// Interaction logic for MpTreeItemCollectionView.xaml
    /// </summary>
    public partial class MpTreeItemCollectionView : MpUserControl<MpITreeItemViewModel> {
        public FrameworkElement TreeItemView { get; set; }

        public MpTreeItemCollectionView() {
            InitializeComponent();
        }

        //#region TreeItemView Property
        //public static readonly DependencyProperty TreeItemViewProperty =
        //    DependencyProperty.Register(
        //        "TreeItemView",
        //        typeof(FrameworkElement),
        //        typeof(MpTreeItemCollectionView),
        //        new FrameworkPropertyMetadata(
        //            null,
        //            OnTreeItemViewPropertyChanged));

        //private static void OnTreeItemViewPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
        //    ((FrameworkElement)source).TreeItemView = (MpManageAnalyticItemsContainerView)e.NewValue;
        //    ((FrameworkElement)source).Resources["ClipTileBack"] = (MpManageAnalyticItemsContainerView)e.NewValue;
        //}

        //private void Viewport3D_Unloaded(object sender, RoutedEventArgs e) {
        //    frontToBack.Completed -= FrontToBack_Completed;
        //    backToFront.Completed -= BackToFront_Completed;
        //}

        //public FrameworkElement TreeItemView {
        //    set { SetValue(TreeItemViewProperty, value); }
        //    get { return (MpManageAnalyticItemsContainerView)GetValue(TreeItemViewProperty); }
        //}
        //#endregion
    }
}
