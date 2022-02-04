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
using ZoomAndPan;
namespace MpWpfApp {

    public class MpViewportCameraBehavior : MpBehavior<ZoomAndPanControl> {
        #region Private Variables

        private Point _lastMousePosition;

        private Point _designerMouseDownPosition; //control
        private Point _viewportMouseDownPosition; //content

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

        public double ZoomFactor { get; set; } =  1.0;

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
            //double oldZoom = ViewportCameraViewModel.CameraZoomFactor;
            //double newZoom = oldZoom + -e.Delta * _mouseWheelDampening;

            ////ViewportCameraViewModel.CameraZoomFactor = Math.Min(
            ////                                                Math.Max(
            ////                                                    newZoom,
            ////                                                    ViewportCameraViewModel.MinCameraZoomFactor), 
            ////                                                ViewportCameraViewModel.MaxCameraZoomFactor);
            //var position = e.GetPosition(AssociatedObject);
            //var transform = (MatrixTransform)AssociatedObject.RenderTransform;
            //var matrix = transform.Matrix;
            //var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            //matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            //transform.Matrix = matrix;

            double deltaZoom = 0;
            if (e.Delta > 0) {
                deltaZoom = ZoomFactor;
            } else if (e.Delta < 0) {
                deltaZoom = -ZoomFactor;
            }
            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            AssociatedObject.AnimatedZoomAboutPoint(AssociatedObject.ContentScale + deltaZoom, e.GetPosition(lb));
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!ViewportCameraViewModel.IsPanning) {
                return;
            }

            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            var vmp = e.GetPosition(lb);

            Vector delta = vmp - _viewportMouseDownPosition;
            _lastMousePosition = vmp;

            AssociatedObject.ContentOffsetX -= delta.X;
            AssociatedObject.ContentOffsetY -= delta.Y;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            _designerMouseDownPosition = e.GetPosition(AssociatedObject);
            _viewportMouseDownPosition = e.GetPosition(lb);

            if (ViewportCameraViewModel.CanPan) {
                ViewportCameraViewModel.IsPanning = AssociatedObject.CaptureMouse();

                if (ViewportCameraViewModel.IsPanning) {
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
