using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvZoomFactorView : MpAvUserControl<MpIZoomFactorViewModel> {
        public MpAvZoomFactorView() {
            InitializeComponent();

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            ZoomSliderContainerGrid.PointerPressed += ZoomSliderContainerGrid_PointerPressed;
            ZoomSliderContainerGrid.PointerMoved += ZoomSliderContainerGrid_PointerMoved;

            this.EffectiveViewportChanged += MpAvZoomFactorView_EffectiveViewportChanged;
        }

        private void MpAvZoomFactorView_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {

            UpdateMarkerPositions();
        }

        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            UpdateMarkerPositions();
        }
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            UpdateMarkerPositions();
        }
        private void ZoomSliderContainerGrid_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext is not MpIZoomFactorViewModel zfvm) {
                return;
            }
            if (e.ClickCount == 2) {
                zfvm.ResetZoomCommand.Execute(null);
                return;
            }

            e.Pointer.Capture(ZoomSliderContainerGrid);
            SetZoomFactorByPercent(GetPercentByPointerEvent(e));
        }

        private void ZoomSliderContainerGrid_PointerMoved(object sender, PointerEventArgs e) {
            if (BindingContext is not MpIZoomFactorViewModel zfvm ||
                !e.IsLeftDown(sender as Control) ||
                e.Pointer.Captured != ZoomSliderContainerGrid) {
                return;
            }
            SetZoomFactorByPercent(GetPercentByPointerEvent(e));
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentZoomFactorChanged:
                    UpdateMarkerPositions();
                    break;
            }
        }

        private void SetZoomFactorByPercent(double percent) {
            if (BindingContext is not MpIZoomFactorViewModel zfvm) {
                return;
            }

            zfvm.SetZoomCommand.Execute(GetPercentAsZoomFactor(zfvm, percent));
        }


        public void UpdateMarkerPositions() {
            if (BindingContext is not MpIZoomFactorViewModel zfvm) {
                return;
            }
            double percent = GetZoomFactorAsPercent(zfvm);
            SetMarkerPosition(ZoomSliderContainerGrid, CurValLine, percent);

            // position default marker
            double default_percent = (zfvm.DefaultZoomFactor - zfvm.MinZoomFactor) / (zfvm.MaxZoomFactor - zfvm.MinZoomFactor);
            SetMarkerPosition(ZoomSliderContainerGrid, ZoomDefaultLine, default_percent);
        }

        private void SetMarkerPosition(Control container, Control marker, double percent) {
            if (marker.RenderTransform is not TranslateTransform tt) {
                return;
            }
            double hw = marker.Bounds.Width * 0.5;
            double hh = marker.Bounds.Height * 0.5;

            double x = MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                (container.Bounds.Width * percent) :
                (container.Bounds.Width / 2);
            //x += offsetX;

            double y = MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                (container.Bounds.Height / 2) :
                (container.Bounds.Height * percent);
            //y += offsetY;

            tt.X = x - hw;
            tt.Y = y - hh;
        }


        private double GetPercentByPointerEvent(PointerEventArgs e) {
            var cg_mp = e.GetClientMousePoint(ZoomSliderContainerGrid);

            double percent =
                MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                    cg_mp.X / ZoomSliderContainerGrid.Bounds.Width :
                    cg_mp.Y / ZoomSliderContainerGrid.Bounds.Height;
            return percent;
        }
        private double GetZoomFactorAsPercent(MpIZoomFactorViewModel zfvm) {
            return (zfvm.ZoomFactor - zfvm.MinZoomFactor) / (zfvm.MaxZoomFactor - zfvm.MinZoomFactor);
        }

        private double GetPercentAsZoomFactor(MpIZoomFactorViewModel zfvm, double percent) {
            return ((zfvm.MaxZoomFactor - zfvm.MinZoomFactor) * percent) + zfvm.MinZoomFactor;
        }
        private double GetSliderPercent() {
            return MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                    CurValLine.Bounds.Center.X / ZoomSliderContainerGrid.Bounds.Width :
                    CurValLine.Bounds.Center.Y / ZoomSliderContainerGrid.Bounds.Height;
        }
    }
}
