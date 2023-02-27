using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowTitleMenuViewModel> {
        private bool _wasZoomDragging = false;
        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();

            var czfb = this.FindControl<Control>("CurZoomFactorButton");
            czfb.PointerMoved += Czfb_PointerMoved;
            czfb.DoubleTapped += Czfb_DoubleTapped;

            var czfb_cg = this.FindControl<Control>("ZoomSliderContainerGrid");
            czfb_cg.PointerReleased += Czfb_cg_PointerReleased;

            var windowDragButton = this.FindControl<Control>("WindowOrientationHandleButton");
            windowDragButton.AddHandler(Control.PointerPressedEvent, WindowDragButton_PointerPressed, RoutingStrategies.Tunnel);
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        #region Zoom Slider

        public void SetZoomFactor(double percent, MpPoint p = null) {
            var ctrvm = MpAvClipTrayViewModel.Instance;
            ctrvm.ZoomFactor = (ctrvm.MaxZoomFactor - ctrvm.MinZoomFactor) * percent;
            PositionZoomValueButton(p);
        }

        public void PositionZoomValueButton(MpPoint p = null) {
            var ctrvm = MpAvClipTrayViewModel.Instance;
            double percent =
                (ctrvm.ZoomFactor - ctrvm.MinZoomFactor) /
                (ctrvm.MaxZoomFactor - ctrvm.MinZoomFactor);
            var czfb_cg = this.FindControl<Control>("ZoomSliderContainerGrid");
            var czfb = this.FindControl<Control>("CurZoomFactorButton");

            double offsetX = -czfb.Bounds.Width * 0.5;
            double offsetY = -czfb.Bounds.Height * 0.5;
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                if (p == null) {
                    p = new MpPoint(
                        czfb_cg.Bounds.Width * percent,
                        (czfb_cg.Bounds.Height / 2) - (czfb.Bounds.Height / 2));
                } else {
                    p.Y = (czfb_cg.Bounds.Height / 2) - (czfb.Bounds.Height / 2);
                }
                p.X += offsetX;
            } else {
                if (p == null) {
                    p = new MpPoint(
                        (czfb_cg.Bounds.Width / 2) - (czfb.Bounds.Width / 2),
                        czfb_cg.Bounds.Height * percent);
                } else {
                    p.X = (czfb_cg.Bounds.Width / 2) - (czfb.Bounds.Width / 2);
                }
                p.Y += offsetY;
            }
            Canvas.SetLeft(czfb, p.X);
            Canvas.SetTop(czfb, p.Y);
        }
        private void Czfb_cg_PointerReleased(object sender, PointerReleasedEventArgs e) {
            var czfb_cg = this.FindControl<Control>("CurZoomValueCanvas");
            var cg_mp = e.GetClientMousePoint(czfb_cg);

            double percent =
                MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                    cg_mp.X / czfb_cg.Bounds.Width :
                    cg_mp.Y / czfb_cg.Bounds.Height;
            SetZoomFactor(percent, cg_mp);
        }

        private void Czfb_DoubleTapped(object sender, RoutedEventArgs e) {
            MpAvClipTrayViewModel.Instance.ZoomFactor =
                MpAvClipTrayViewModel.Instance.DefaultZoomFactor;
        }

        private void Czfb_PointerMoved(object sender, PointerEventArgs e) {
            var czfb_cg = this.FindControl<Control>("CurZoomValueCanvas");
            var cg_mp = e.GetClientMousePoint(czfb_cg);

            if (e.IsLeftDown(sender as Control)) {
                e.Pointer.Capture(sender as Control);
                _wasZoomDragging = true;
                PositionZoomValueButton(cg_mp);
            } else if (_wasZoomDragging) {
                _wasZoomDragging = false;

                double percent =
                    MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ?
                        cg_mp.X / czfb_cg.Bounds.Width :
                        cg_mp.Y / czfb_cg.Bounds.Height;
                SetZoomFactor(percent, cg_mp);
            }

        }

        #endregion

        #region Window Drag
        private MpMainWindowOrientationType _startOrientation;
        private MpMainWindowOrientationType _curOrientation;
        private void WindowDragButton_PointerPressed(object sender, PointerPressedEventArgs e) {
            var windowDragButton = sender as Control;
            if (windowDragButton == null) {
                return;
            }
            e.Handled = true;
            windowDragButton.DragCheckAndStart(
                e,
                WindowDragButton_Start, WindowDragButton_Move, WindowDragButton_End,
                null,
                MpAvShortcutCollectionViewModel.Instance);
        }

        private void WindowDragButton_Start(PointerPressedEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowOrientationDragging = true;
            _startOrientation = MpAvMainWindowViewModel.Instance.MainWindowOrientationType;
            e.Pointer.Capture(e.Source as Control);
        }
        private void WindowDragButton_Move(PointerEventArgs e) {
            MpPoint mw_mp = e.GetClientMousePoint(MpAvMainView.Instance);

            MpPoint screen_mp = MpAvMainView.Instance.PointToScreen(
                mw_mp.ToAvPoint())
                .ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);

            MpRect mw_screen_rect = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds;
            var screen_faces = mw_screen_rect.ToFaces();

            int cur_face_idx = -1;
            for (int i = 0; i < screen_faces.Length; i++) {
                var face = screen_faces[i];
                if (face.Contains(screen_mp)) {
                    cur_face_idx = i;
                    break;
                }
            }
            if (cur_face_idx < 0) {
                cur_face_idx = (int)_curOrientation;
            }

            _curOrientation = (MpMainWindowOrientationType)cur_face_idx;
            //MpConsole.WriteLine("");
            //MpConsole.WriteLine("Window Drag mp: " + mw_mp);
            //MpConsole.WriteLine("Screen Drag mp: " + screen_mp);
            //MpConsole.WriteLine("Cur Orientation: " + _curOrientation);
            //MpConsole.WriteLine("");
            if (MpAvMainWindowViewModel.Instance.MainWindowOrientationType != _curOrientation) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(_curOrientation);
            }
        }

        private void WindowDragButton_End(PointerReleasedEventArgs e) {
            e.Pointer.Capture(null);

            MpAvMainWindowViewModel.Instance.IsMainWindowOrientationDragging = false;

            MpMainWindowOrientationType final_or = _curOrientation;
            bool was_canceled = e == null;
            if (was_canceled) {
                final_or = _startOrientation;
            }
            if (MpAvMainWindowViewModel.Instance.MainWindowOrientationType != final_or) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(final_or);
            }
            MpAvMainView.Instance.ClampContentSizes();
        }

        #endregion
    }
}
