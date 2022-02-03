using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
using MonkeyPaste;
using System.Windows.Media;

namespace MpWpfApp {

    public class MpViewportCameraBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables

        private Point _lastMousePosition;
        private Point _mouseDownPosition;

        private double _originalZoomFactor;
        private MpSize _originalSize;

        private double _mouseWheelDampening = 0.008;

        #endregion

        #region Properties

        #region ViewportCameraViewModel DependencyProperty

        public MpIViewportCameraViewModel ViewportCameraViewModel {
            get { return (MpIViewportCameraViewModel)GetValue(ViewportCameraViewModelProperty); }
            set { SetValue(ViewportCameraViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewportCameraViewModelProperty =
            DependencyProperty.Register(
                "ViewportCameraViewModel", typeof(MpIViewportCameraViewModel),
                typeof(MpViewportCameraBehavior),
                new FrameworkPropertyMetadata(null));

        #endregion


        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            if (AssociatedObject == null || this.ViewportCameraViewModel == null) {
                return;
            }

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.PreviewMouseWheel += AssociatedObject_MouseWheel;

            _originalZoomFactor = ViewportCameraViewModel.CameraZoomFactor;
            _originalSize = new MpSize(ViewportCameraViewModel.DesignerWidth, ViewportCameraViewModel.DesignerHeight);
        }

        protected override void OnUnload() {
            base.OnUnload();

            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_MouseWheel;
        }

        #region Public Methods

        #endregion

        #region Private Methods

        #region Event Handlers

        private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e) {
            double oldZoom = ViewportCameraViewModel.CameraZoomFactor;
            double newZoom = oldZoom + -e.Delta * _mouseWheelDampening;

            ViewportCameraViewModel.CameraZoomFactor = Math.Min(
                                                            Math.Max(
                                                                newZoom,
                                                                ViewportCameraViewModel.MinCameraZoomFactor), 
                                                            ViewportCameraViewModel.MaxCameraZoomFactor);

            var st = AssociatedObject.RenderTransform as ScaleTransform;
            var tavm = AssociatedObject.DataContext as MpTriggerActionViewModelBase;
            double deltaX = ViewportCameraViewModel.ViewportWidth /  ViewportCameraViewModel.DesignerWidth;
            double deltaY = ViewportCameraViewModel.ViewportHeight / ViewportCameraViewModel.DesignerHeight;
            tavm.Parent.AllSelectedActions.ForEach(x => x.X *= deltaX);
            tavm.Parent.AllSelectedActions.ForEach(x => x.Y *= deltaY);

            var mp = e.GetPosition(AssociatedObject);
            var cp = new Point(AssociatedObject.RenderSize.Width / 2, AssociatedObject.RenderSize.Height / 2);
            var delta = cp - mp;
            //delta *= ViewportCameraViewModel.CameraZoomFactor;
            //ViewportCameraViewModel.CameraX += delta.X;
            //ViewportCameraViewModel.CameraY += delta.Y;
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!ViewportCameraViewModel.IsPanning) {
                return;
            }

            var mp = e.GetPosition(AssociatedObject);

            Vector delta = mp - _lastMousePosition;
            _lastMousePosition = mp;

            ViewportCameraViewModel.CameraX += delta.X;
            ViewportCameraViewModel.CameraY += delta.Y;

            //MpConsole.WriteLine($"Camera X:{ViewportCameraViewModel.CameraX} Y:{ViewportCameraViewModel.CameraY}");
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (ViewportCameraViewModel.CanPan) {
                ViewportCameraViewModel.IsPanning = AssociatedObject.CaptureMouse();

                if (ViewportCameraViewModel.IsPanning) {
                    Viewbox vb = AssociatedObject.GetVisualAncestor<Viewbox>();
                    Grid g = AssociatedObject.GetVisualAncestor<Grid>();

                    var mp = e.GetPosition(AssociatedObject);

                    _mouseDownPosition = _lastMousePosition = mp;
                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();

            if (ViewportCameraViewModel.IsPanning) {
                ViewportCameraViewModel.IsPanning = false;
            }
        }

        #endregion

        #endregion
    }
}
