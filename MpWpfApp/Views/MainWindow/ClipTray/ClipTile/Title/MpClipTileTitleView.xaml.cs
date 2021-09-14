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
    /// Interaction logic for MpClipTileTitleView.xaml
    /// </summary>
    public partial class MpClipTileTitleView : UserControl {
        public MpClipTileTitleView() {
            InitializeComponent();
        }

        private void ClipTileAppIconBorderImage_Loaded(object sender, RoutedEventArgs e) {
            ClipTileTitleTextGrid.Width = MpMeasurements.Instance.ClipTileTitleTextGridWidth;
            RenderOptions.SetBitmapScalingMode(ClipTileAppIconBorderImage, BitmapScalingMode.LowQuality);
        }

        private void ClipTileTitleTextGrid_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            Application.Current.MainWindow.Cursor = Cursors.IBeam;
            ctvm.IsHoveringOnTitleTextGrid = true;
        }

        private void ClipTileTitleTextGrid_MouseLeave(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            Application.Current.MainWindow.Cursor = Cursors.Arrow;
            ctvm.IsHoveringOnTitleTextGrid = false;
        }

        private void ClipTileTitleTextGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsEditingTitle = true;
            e.Handled = true;
        }

        private void ClipTileTitleTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (ClipTileTitleTextBox.Visibility == Visibility.Collapsed) {
                return;
            }
            if(ctvm != null) {
                ClipTileTitleTextBox.Focus();
                ClipTileTitleTextBox.SelectAll();
            }
        }

        private void ClipTileTitleTextBox_LostFocus(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            ctvm.IsEditingTitle = false;
        }

        private void ClipTileTitleTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (e.Key == Key.Enter || e.Key == Key.Escape) {
                ctvm.IsEditingTitle = false;
            }
        }

        private void ClipTileAppIconImageButton_MouseEnter(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (MpClipTrayViewModel.Instance.IsScrolling) {
                return;
            }
            if (ctvm.IsEditingTemplate || ctvm.IsPastingTemplate) {
                return;
            }
            double t = 100;
            double angle = 15;
            var a = new DoubleAnimation(0, angle, new Duration(TimeSpan.FromMilliseconds(t)));
            a.Completed += (s1, e1) => {
                var b = new DoubleAnimation(angle, -angle, new Duration(TimeSpan.FromMilliseconds(t * 2)));
                b.Completed += (s2, e2) => {
                    var c = new DoubleAnimation(-angle, 0, new Duration(TimeSpan.FromMilliseconds(t)));
                    ClipTileAppIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, c);
                };
                ClipTileAppIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, b);
            };

            ClipTileAppIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, a);

            ClipTileAppIconBorderImage.Visibility = Visibility.Visible;
            double fromScale = 1;
            double toScale = 1.1;
            double st = 300;
            var sa = new DoubleAnimation(fromScale, toScale, new Duration(TimeSpan.FromMilliseconds(st)));
            var easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseOut;
            sa.EasingFunction = easing;
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
        }

        private void ClipTileAppIconImageButton_MouseLeave(object sender, MouseEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            if (MpClipTrayViewModel.Instance.IsScrolling || ctvm.IsContextMenuOpened) {
                return;
            }
            if (ctvm.IsEditingTemplate || ctvm.IsPastingTemplate) {
                return;
            }
            double fromScale = 1.15;
            double toScale = 1;
            double st = 300;
            var sa = new DoubleAnimation(fromScale, toScale, new Duration(TimeSpan.FromMilliseconds(st)));
            sa.Completed += (s1, e31) => {
                ClipTileAppIconBorderImage.Visibility = Visibility.Hidden;
            };
            var easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            sa.EasingFunction = easing;
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
        }

        private void ClipTileAppIconImageButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;
            //MpHelpers.Instance.OpenUrl(CopyItem.Source.App.AppPath);
            MpClipTrayViewModel.Instance.ClearClipSelection();
            ctvm.IsSelected = true;
            foreach (var vctvm in MpClipTrayViewModel.Instance.VisibileClipTiles) {
                if (vctvm.CopyItemAppId != ctvm.CopyItemAppId) {
                    bool hasSubItemWithApp = false;
                    if (vctvm.ContentContainerViewModel.ItemViewModels.Count > 1) {
                        foreach (var vrtbvm in vctvm.ContentContainerViewModel.ItemViewModels) {
                            if (vrtbvm.CopyItem.Source.App.Id != ctvm.CopyItemAppId) {
                                vrtbvm.ItemVisibility = Visibility.Collapsed;
                            } else {
                                hasSubItemWithApp = true;
                            }
                        }
                    }
                    if (!hasSubItemWithApp) {
                        vctvm.TileVisibility = Visibility.Collapsed;
                    }
                }
            }
            //this triggers clip tray to swap out the app icons for the filtered app
            //MpClipTrayViewModel.Instance.FilterByAppIcon = ctvm.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64.ToBitmapSource();
            MpClipTrayViewModel.Instance.IsFilteringByApp = true;
        }
    }
}
