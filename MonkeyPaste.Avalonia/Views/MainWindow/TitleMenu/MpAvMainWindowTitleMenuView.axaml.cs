using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowTitleMenuViewModel> {
        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();

            var windowDragButton = this.FindControl<Control>("WindowOrientationHandleButton");
            windowDragButton.AddHandler(Control.PointerPressedEvent, WindowDragButton_PointerPressed, RoutingStrategies.Tunnel);
        }

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
            MpConsole.WriteLine("");
            MpConsole.WriteLine("Window Drag mp: " + mw_mp);
            MpConsole.WriteLine("Screen Drag mp: " + screen_mp);
            MpConsole.WriteLine("Cur Orientation: " + _curOrientation);
            MpConsole.WriteLine($"Scaling: {MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling}");
            MpConsole.WriteLine("");
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
        }

        #endregion
    }
}
