using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowViewModel> {
        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();
            //var ltb = this.FindControl<Button>("MainWindowOrientationButton");
            //ltb.AddHandler(Button.PointerPressedEvent, Ltb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void Ltb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(false);
            } else if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(true);
            }
        }

        private void ZoomFactorSlider_DoubleTapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            MpAvClipTrayViewModel.Instance.ResetZoomFactorCommand.Execute(null);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
