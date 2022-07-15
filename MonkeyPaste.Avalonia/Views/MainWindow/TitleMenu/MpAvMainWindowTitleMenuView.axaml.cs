using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMainWindowTitleMenuView : MpAvUserControl<MpAvMainWindowViewModel> {
        public MpAvMainWindowTitleMenuView() {
            InitializeComponent();
        }

        private void ZoomFactorSlider_DoubleTapped(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            MpAvClipTrayViewModel.Instance.ResetZoomFactorCommand.Execute(null);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
