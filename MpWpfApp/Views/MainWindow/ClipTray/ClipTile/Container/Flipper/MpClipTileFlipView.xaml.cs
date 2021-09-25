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
        Storyboard frontToBack, backToFront;

        public MpClipTileFlipView() {
            InitializeComponent();
        }

        private void Viewport3D_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (MpMainWindowViewModel.IsMainWindowLoading || frontToBack == null || backToFront == null) {
                return;
            }
            var ctvm = DataContext as MpClipTileViewModel;
            if(ctvm.IsFlipped) {
                backToFront.Begin();
            } else {
                frontToBack.Begin();
            }
            
        }

        private void ClipTileFlipView_Loaded(object sender, RoutedEventArgs e) {
            frontToBack = this.FindResource("FrontToBack") as Storyboard;
            backToFront = this.FindResource("BackToFront") as Storyboard;

            frontToBack.Completed += FrontToBack_Completed;
            backToFront.Completed += BackToFront_Completed;
        }

        private void BackToFront_Completed(object sender, EventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsFlipping = false;
            ctvm.IsFlipped = false;
        }

        private void FrontToBack_Completed(object sender, EventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsFlipping = false;
            ctvm.IsFlipped = true;
        }


        #region OwnerFront Property
        public static readonly DependencyProperty OwnerFrontProperty =
            DependencyProperty.Register(
                "OwnerFront",
                typeof(MpClipTileView),
                typeof(MpClipTileFlipView),
                new FrameworkPropertyMetadata(
                    null,
                    OnOwnerFrontPropertyChanged));

        private static void OnOwnerFrontPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            ((MpClipTileFlipView)source).OwnerFront = (MpClipTileView)e.NewValue;
            ((MpClipTileFlipView)source).Resources["ClipTileFront"] = (MpClipTileView)e.NewValue;
        }

        public MpClipTileView OwnerFront {
            set { SetValue(OwnerFrontProperty, value); }
            get { return (MpClipTileView)GetValue(OwnerFrontProperty); }
        }
        #endregion

        #region OwnerBack Property
        public static readonly DependencyProperty OwnerBackProperty =
            DependencyProperty.Register(
                "OwnerBack",
                typeof(MpContentItemAnalyticsView),
                typeof(MpClipTileFlipView),
                new FrameworkPropertyMetadata(
                    null,
                    OnOwnerBackPropertyChanged));

        private static void OnOwnerBackPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            ((MpClipTileFlipView)source).OwnerBack = (MpContentItemAnalyticsView)e.NewValue;
            ((MpClipTileFlipView)source).Resources["ClipTileBack"] = (MpContentItemAnalyticsView)e.NewValue;
        }

        public MpContentItemAnalyticsView OwnerBack {
            set { SetValue(OwnerBackProperty, value); }
            get { return (MpContentItemAnalyticsView)GetValue(OwnerBackProperty); }
        }
        #endregion
    }
}
