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

        private Point _contentMouseDownPosition; //control
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

            MpMessenger.Register(
                MpActionCollectionViewModel.Instance, 
                ReceivedActionCollectionViewModelMessage);
        }


        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.PreviewMouseWheel -= AssociatedObject_MouseWheel;
            }


            MpMessenger.Unregister<MpMessageType>(
                MpActionCollectionViewModel.Instance,
                ReceivedActionCollectionViewModelMessage);
        }

        #region Public Methods

        #endregion

        #region Private Methods

        private void ScaleToContent() {
            var astavml = MpActionCollectionViewModel.Instance.AllSelectedTriggerActions;
            var contentRect = new Rect();
            foreach (var avm in astavml) {
                contentRect.Union(new Rect(avm.Location, new Size(avm.Width, avm.Height)));
            }
            contentRect.Width += 100;
            contentRect.Height += 100;
            AssociatedObject.ZoomTo(contentRect);
        }

        private void ReceivedActionCollectionViewModelMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.ActionViewportChanged:
                    ScaleToContent();
                    break;
            }
        }

        #region Event Handlers

        private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e) {
            double deltaZoom = 0;
            if (e.Delta > 0) {
                deltaZoom = ZoomFactor;
            } else if (e.Delta < 0) {
                deltaZoom = -ZoomFactor;
            }
            var contentMousePosition = e.GetPosition(AssociatedObject);
            AssociatedObject.ZoomAboutPoint(AssociatedObject.ContentScale + deltaZoom, contentMousePosition);
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if(Mouse.LeftButton == MouseButtonState.Released) {
                ViewportCameraViewModel.IsPanning = false;
            }
            if (!ViewportCameraViewModel.IsPanning) {
                return;
            }

            var contentMousePosition = e.GetPosition(AssociatedObject);

            Vector offset = _contentMouseDownPosition - contentMousePosition;

            AssociatedObject.ContentOffsetX = offset.X;
            AssociatedObject.ContentOffsetY = offset.Y;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            _contentMouseDownPosition = e.GetPosition(AssociatedObject);
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
