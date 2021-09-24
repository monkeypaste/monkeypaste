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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : UserControl {
        bool isFlipped = false;
        Storyboard frontToBack, backToFront;

        public MpClipTileContainerView() {
            InitializeComponent();
        }

        private void Viewport3D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(MpMainWindowViewModel.IsMainWindowLoading || frontToBack == null || backToFront == null) {
                return;
            }
            frontToBack.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            frontToBack = this.FindResource("FrontToBack") as Storyboard;
            backToFront = this.FindResource("BackToFront") as Storyboard;
        }
    }
}
