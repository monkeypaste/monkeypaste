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

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowViewModel> {
        private bool wasPressZoomJump = false;

        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();
            var zoomFactorSlider = this.FindControl<Slider>("ZoomFactorSlider");
            zoomFactorSlider.DoubleTapped += ZoomFactorSlider_DoubleTapped;
            zoomFactorSlider.AddHandler(Slider.PointerPressedEvent, ZoomFactorSlider_PointerPressed, RoutingStrategies.Tunnel);
            //zoomFactorSlider.PointerPressed += ZoomFactorSlider_PointerPressed;
            zoomFactorSlider.PointerReleased += ZoomFactorSlider_PointerReleased;
            
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
            //var ltb = this.FindControl<Button>("MainWindowOrientationButton");
            //ltb.AddHandler(Button.PointerPressedEvent, Ltb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void ZoomFactorSlider_PointerPressed(object sender, PointerPressedEventArgs e) {
            MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeBegin);

            //bool isThumbPress = (e.Source as Control).GetVisualAncestor<Thumb>() != null;
            //if(!isThumbPress) {
                
            //    var zoomFactorSlider = this.FindControl<Slider>("ZoomFactorSlider");
            //    var track = zoomFactorSlider.GetVisualDescendant<Track>();
            //    if(track == null) {
            //        Debugger.Break();
            //    }
            //    // is track press
            //    var track_mp = e.GetPosition(track);
            //    double newValue = track.ValueFromPoint(track_mp);
            //    MpAvClipTrayViewModel.Instance.ZoomFactor = newValue;
            //    MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeEnd);
            //    wasPressZoomJump = true;
            //    e.Handled = true;
            //    return;
            //}
            //MpAvClipTrayViewModel.Instance.IsZoomDragging = true;
            e.Handled = false;
        }
        private void ZoomFactorSlider_PointerReleased(object sender, PointerReleasedEventArgs e) {
            //if(!wasPressZoomJump) {
            //    MpAvClipTrayViewModel.Instance.IsZoomDragging = false;
            //    MpMessenger.SendGlobal(MpMessageType.TrayZoomFactorChangeEnd);
            //} else {
            //    wasPressZoomJump = false;
            //}
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
