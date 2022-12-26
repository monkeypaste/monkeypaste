using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using Avalonia.Threading;
using System.Threading.Tasks;
using System.Linq;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowTitleMenuViewModel> {
        private bool wasPressZoomJump = false;

        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();

            var titleGrid = this.FindControl<Grid>("TitlePanel");
            titleGrid.Children.CollectionChanged += Children_CollectionChanged;

            var zoomFactorSlider = this.FindControl<Slider>("ZoomFactorSlider");
            zoomFactorSlider.DoubleTapped += ZoomFactorSlider_DoubleTapped;
            zoomFactorSlider.AddHandler(Slider.PointerPressedEvent, ZoomFactorSlider_PointerPressed, RoutingStrategies.Tunnel);
            //zoomFactorSlider.PointerPressed += ZoomFactorSlider_PointerPressed;
            zoomFactorSlider.PointerReleased += ZoomFactorSlider_PointerReleased;
            zoomFactorSlider.PointerMoved += ZoomFactorSlider_PointerMoved;
            Dispatcher.UIThread.Post(async () => {
                var zoom_thumb = zoomFactorSlider.GetVisualDescendant<Thumb>();
                while(zoom_thumb == null) {
                    zoom_thumb = zoomFactorSlider.GetVisualDescendant<Thumb>();
                    if(zoom_thumb == null) {
                        await Task.Delay(100);
                    } else {
                        zoom_thumb.Cursor = new Cursor(StandardCursorType.Hand);
                    }
                }
            });

            var windowDragBorder = this.FindControl<Control>("WindowOrientationHandleBorder");
            windowDragBorder.AddHandler(Border.PointerPressedEvent, WindowDragBorder_PointerPressed, RoutingStrategies.Tunnel);
            //var ltb = this.FindControl<Button>("MainWindowOrientationButton");
            //ltb.AddHandler(Button.PointerPressedEvent, Ltb_PointerPressed, RoutingStrategies.Tunnel);

        }

        #region Window Drag
        private MpMainWindowOrientationType _startOrientation;
        private MpMainWindowOrientationType _curOrientation;
        private void WindowDragBorder_PointerPressed(object sender, PointerPressedEventArgs e) {
            var windowDragBorder = sender as Control;
            if(windowDragBorder == null) {
                return;
            }
            windowDragBorder.DragCheckAndStart(
                e, 
                WindowDragBorder_Start, WindowDragBorder_Move, WindowDragBorder_End, 
                null,
                MpAvShortcutCollectionViewModel.Instance);
        }

        private void WindowDragBorder_Start(PointerPressedEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowOrientationDragging = true;
            _startOrientation = MpAvMainWindowViewModel.Instance.MainWindowOrientationType;
            e.Pointer.Capture(e.Source as Control);
        }
        private void WindowDragBorder_Move(PointerEventArgs e) {
            MpPoint mw_mp = e.GetClientMousePoint(MpAvMainWindow.Instance);

            MpPoint screen_mp = MpAvMainWindow.Instance.PointToScreen(
                mw_mp.ToAvPoint())
                .ToPortablePoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelDensity);

            MpRect mw_screen_rect = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds;
            var screen_faces = mw_screen_rect.ToFaces();

            int cur_face_idx = -1;
            for (int i = 0; i < screen_faces.Length; i++) {
                var face = screen_faces[i];
                if(face.Contains(screen_mp)) {
                    cur_face_idx = i;
                    break;
                }
            }
            if(cur_face_idx < 0) {
                cur_face_idx = (int)_curOrientation;
            }

            _curOrientation = (MpMainWindowOrientationType)cur_face_idx;
            //MpConsole.WriteLine("");
            //MpConsole.WriteLine("Window Drag mp: " + mw_mp);
            //MpConsole.WriteLine("Screen Drag mp: " + screen_mp);
            //MpConsole.WriteLine("Cur Orientation: " + _curOrientation);
            //MpConsole.WriteLine("");
            if(MpAvMainWindowViewModel.Instance.MainWindowOrientationType != _curOrientation) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(_curOrientation);
            }
        }

        private void WindowDragBorder_End(PointerReleasedEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowOrientationDragging = false;

            MpMainWindowOrientationType final_or = _curOrientation;
            bool was_canceled = e == null;
            if(was_canceled) {
                final_or = _startOrientation;
            }
            if(MpAvMainWindowViewModel.Instance.MainWindowOrientationType != final_or) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(final_or);
            }
            MpAvMainWindow.Instance.ClampContentSizes();
        }

        #endregion

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            
            var titleGrid = this.FindControl<Grid>("TitlePanel");
            double totalItemWidth = titleGrid.Children.Sum(x => x.Bounds.Width);
            MpConsole.WriteLine("total items width: " + totalItemWidth);
        }

        private void ZoomFactorSlider_PointerPressed(object sender, PointerPressedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeBegin);

            bool isThumbPress = (e.Source as Control).GetVisualAncestor<Thumb>() != null;
            if (!isThumbPress) {

                var zoomFactorSlider = this.FindControl<Slider>("ZoomFactorSlider");
                var track = zoomFactorSlider.GetVisualDescendant<Track>();
                if (track == null) {
                    Debugger.Break();
                }
                // is track press
                var track_mp = e.GetPosition(track);
                double newValue = track.ValueFromPoint(track_mp);
                MpAvClipTrayViewModel.Instance.ZoomFactor = newValue;
                //MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeEnd);
                //e.Pointer.Capture(slider);
                wasPressZoomJump = true;
                e.Handled = true;
                return;
            }
            e.Handled = false;
        }


        private void ZoomFactorSlider_PointerMoved(object sender, PointerEventArgs e) {
            if(!wasPressZoomJump) {
                return;
            }
            var slider = sender as Slider;
            var track = slider.GetVisualDescendant<Track>();
            var track_mp = e.GetPosition(track);
            double newValue = track.ValueFromPoint(track_mp);
            MpAvClipTrayViewModel.Instance.ZoomFactor = newValue;
        }

        private void ZoomFactorSlider_PointerReleased(object sender, PointerReleasedEventArgs e) {
            wasPressZoomJump = false;
            MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeEnd);
        }


        //private void Ltb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
        //    if(e.IsL.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
        //        MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(false);
        //    } else if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
        //        MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(true);
        //    }
        //}

        private void ZoomFactorSlider_DoubleTapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            MpAvClipTrayViewModel.Instance.ResetZoomFactorCommand.Execute(null);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
