﻿using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Media;
using ZoomAndPan;
namespace MpWpfApp {

    public class MpViewportCameraBehavior : MpBehavior<ZoomAndPanControl> {
        #region Private Variables

        private Point _lastContentPosition; //control
        private Point _viewportMouseDownPosition; //content

        private double _originalZoomFactor;
        private MpSize _originalSize;

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

        protected override async void OnLoad() {
            base.OnLoad();

            if (AssociatedObject == null || this.ViewportCameraViewModel == null) {
                return;
            }
            _originalZoomFactor = ViewportCameraViewModel.CameraZoomFactor;
            _originalSize = new MpSize(ViewportCameraViewModel.DesignerWidth, ViewportCameraViewModel.DesignerHeight);


            var designerView = AssociatedObject.GetVisualAncestor<UserControl>();

            while(designerView == null) {
                await Task.Delay(100);
                designerView = AssociatedObject.GetVisualAncestor<UserControl>();
            }
            designerView.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            designerView.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            designerView.PreviewMouseMove += AssociatedObject_MouseMove;
            designerView.PreviewMouseWheel += AssociatedObject_MouseWheel;
            AssociatedObject.MouseDoubleClick += DesignerView_MouseDoubleClick;


            MpMessenger.Register(
                MpActionCollectionViewModel.Instance,
                ReceivedActionCollectionViewModelMessage);

            ScaleToContent();
        }

        

        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                var designerView = AssociatedObject.GetVisualAncestor<UserControl>();

                designerView.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                designerView.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                designerView.PreviewMouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.PreviewMouseWheel -= AssociatedObject_MouseWheel;
            }


            MpMessenger.Unregister<MpMessageType>(
                MpActionCollectionViewModel.Instance,
                ReceivedActionCollectionViewModelMessage);
        }

        #region Public Methods

        #endregion

        #region Private Methods
        private void DesignerView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ScaleToContent();
        }

        public void ScaleToContent() {
            var astavml = MpActionCollectionViewModel.Instance.AllSelectedTriggerActions;
           if(astavml.Count == 0) {
                return;
            }
            var contentRect = new Rect();
            contentRect.Location = new Point(
                astavml.Min(x => x.Location.X), astavml.Min(x => x.Location.Y));
            foreach (var avm in astavml) {
                contentRect.Union(new Rect(avm.Location, new Size(avm.Width, avm.Height)));
            }

            Point offset = new Point();
            if (contentRect.Location.X < 0) {
                offset.X = Math.Abs(contentRect.Location.X) + 10;
            }
            if (contentRect.Location.Y < 0) {
                offset.Y = Math.Abs(contentRect.Location.Y) + 10;
            }

            astavml.ForEach(x => x.X += offset.X);
            astavml.ForEach(x => x.Y += offset.Y);
            contentRect.Width += 100;
            contentRect.Height += 100;

            AssociatedObject.ZoomTo(contentRect);

            //AssociatedObject.ScaleToFit();
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

            Vector offset1 = _lastContentPosition - contentMousePosition;
            _lastContentPosition = contentMousePosition;
            //var viewportMousePosition = e.GetPosition(AssociatedObject.GetVisualDescendent<ListBox>());

            //Vector offset = viewportMousePosition - _viewportMouseDownPosition;

            AssociatedObject.ContentOffsetX += offset1.X;
            AssociatedObject.ContentOffsetY += offset1.Y;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var lb = AssociatedObject.GetVisualDescendent<ListBox>();
            _lastContentPosition = e.GetPosition(AssociatedObject);
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