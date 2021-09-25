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
    /// Interaction logic for MpClipTileFlipView.xaml
    /// </summary>
    public partial class MpClipTileFlipView : UserControl {
        bool isFlipped = false;
        Storyboard frontToBack, backToFront;

        public MpClipTileFlipView() {
            InitializeComponent();
        }

        private void Viewport3D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (MpMainWindowViewModel.IsMainWindowLoading || frontToBack == null || backToFront == null) {
                return;
            }
            frontToBack.Begin();
        }

        private void ClipTileFlipView_Loaded(object sender, RoutedEventArgs e) {
            frontToBack = this.FindResource("FrontToBack") as Storyboard;
            backToFront = this.FindResource("BackToFront") as Storyboard;
        }


        #region Owner Property
        public static readonly DependencyProperty OwnerProperty =
            DependencyProperty.Register(
                "Owner",
                typeof(MpClipTileView),
                typeof(MpClipTileFlipView),
                new FrameworkPropertyMetadata(
                    null,
                    OnOwnerPropertyChanged));

        private static void OnOwnerPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            ((MpClipTileFlipView)source).Owner = (MpClipTileView)e.NewValue;
            ((MpClipTileFlipView)source).Resources["ClipTileView"] = (MpClipTileView)e.NewValue;
        }

        public MpClipTileView Owner {
            set { SetValue(OwnerProperty, value); }
            get { return (MpClipTileView)GetValue(OwnerProperty); }
        }
        #endregion
    }
}
