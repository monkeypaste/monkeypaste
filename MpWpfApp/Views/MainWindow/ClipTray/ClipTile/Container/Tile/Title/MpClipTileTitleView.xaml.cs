using MonkeyPaste;
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
    public partial class MpClipTileTitleView : MpUserControl<MpClipTileViewModel> {
        private bool _isAnimating = false;

        public MpClipTileTitleView() : base() {
            InitializeComponent();
        }
        private void ClipTileTitleContainerGrid_Loaded(object sender, RoutedEventArgs e) {
            //AttachAllBehaviors();
            RenderOptions.SetBitmapScalingMode(ClipTileAppIconBorderImage, BitmapScalingMode.LowQuality);
        }

        private void AttachAllBehaviors() {
            //ClipTileTitleHighlightBehavior.Attach(this);
            //SourceHighlightBehavior.Attach(this);
        }

        private void DetachAllBehaviors() {
            //ClipTileTitleHighlightBehavior.Detach();
            //SourceHighlightBehavior.Detach();
        }


        private void ClipTileTitleContainerGrid_Unloaded(object sender, RoutedEventArgs e) {
            //DetachAllBehaviors();
        }

        private void ClipTileTitleTextGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //if (BindingContext == null || BindingContext.IsEditingTitle) {
            //    return;
            //}
            //if (!BindingContext.IsEditingTitle) {
            //    BindingContext.IsTitleReadOnly = false;
            //    e.Handled = true;
            //}            
        }

        private void ClipTileTitleTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if (ClipTileTitleTextBox.Visibility == Visibility.Hidden) {
                //MpMarqueeTextBoxExtension.Init(ClipTileTitleMarqueeCanvas);
                return;
            }


            //MpMarqueeTextBoxExtension.Reset(ClipTileTitleMarqueeCanvas);

            ClipTileTitleTextBox.Focus();
            ClipTileTitleTextBox.CaretIndex = 0;
            ClipTileTitleTextBox.SelectAll();
        }

        private void ClipTileTitleTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.IsTitleReadOnly = true;
        }

        private void ClipTileTitleTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            if (e.Key == Key.Enter || e.Key == Key.Escape) {
                //ctvm.CopyItemTitle = ClipTileTitleTextBox.Text;
                BindingContext.IsTitleReadOnly = true;
                e.Handled = true;
            }
        }

        private void ClipTileAppIconImageButton_MouseEnter(object sender, MouseEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                while(_isAnimating) {
                    await Task.Delay(10);
                }
                AnimateEnter();
            });
        }

        private void ClipTileAppIconImageButton_MouseLeave(object sender, MouseEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                while (_isAnimating) {
                    await Task.Delay(10);
                }
                AnimateLeave();
            });
        }

        private void AnimateEnter() {
            if (BindingContext == null || 
                MpClipTrayViewModel.Instance.HasScrollVelocity) {
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
                    _isAnimating = false;
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
            _isAnimating = true;
        }

        private void AnimateLeave() {
            if (BindingContext == null ||
                MpClipTrayViewModel.Instance.HasScrollVelocity || 
                BindingContext.IsContextMenuOpened) {
                return;
            }

            double fromScale = 1.15;
            double toScale = 1;
            double st = 300;
            var sa = new DoubleAnimation(fromScale, toScale, new Duration(TimeSpan.FromMilliseconds(st)));
            sa.Completed += (s1, e31) => {
                ClipTileAppIconBorderImage.Visibility = Visibility.Hidden;
                _isAnimating = false;
            };
            var easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            sa.EasingFunction = easing;
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
            ClipTileAppIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
            _isAnimating = true;
        }

        private void ClipTileAppIconImageButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (BindingContext == null) {
                return;
            }
            //ctvm.Parent.IsFlipped = true;

            //MpHelpers.OpenUrl(CopyItem.Source.App.AppPath);
            MpClipTrayViewModel.Instance.ClearClipSelection();
            BindingContext.IsSelected = true;
            //this triggers clip tray to swap out the app icons for the filtered app
            //MpClipTrayViewModel.Instance.FilterByAppIcon = ctvm.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64.ToBitmapSource();
            MpClipTrayViewModel.Instance.IsFilteringByApp = true;
            e.Handled = true;
        }

        

        private void FlipButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MpClipTrayViewModel.Instance.FlipTileCommand.Execute(BindingContext.Parent);
            e.Handled = true;
        }

    }
}
