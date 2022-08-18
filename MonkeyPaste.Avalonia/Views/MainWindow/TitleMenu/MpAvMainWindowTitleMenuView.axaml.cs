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
            //var ltb = this.FindControl<Button>("MainWindowOrientationButton");
            //ltb.AddHandler(Button.PointerPressedEvent, Ltb_PointerPressed, RoutingStrategies.Tunnel);
        }

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
