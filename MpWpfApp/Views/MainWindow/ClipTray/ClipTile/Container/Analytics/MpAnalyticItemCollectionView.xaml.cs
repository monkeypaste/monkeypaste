using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MpAnalyticItemCollectionView.xaml
    /// </summary>
    public partial class MpAnalyticItemCollectionView : MpUserControl<MpAnalyticItemCollectionViewModel> {
        public MpAnalyticItemCollectionView() {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            MpClipTrayViewModel.Instance.FlipTileCommand.Execute(civm.Parent);
        }

        private async void AnalyticTreeView_Expanded(object sender, RoutedEventArgs e) {
            if(BindingContext.SelectedItem == null) {
                return;
            }
            await BindingContext.SelectedItem.LoadChildren();
        }

        private void AnalyticTreeView_Collapsed(object sender, RoutedEventArgs e) {

        }

        private void AnalyticTreeView_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}
