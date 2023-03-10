using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSettingsWindow : Window {
        public static MpAvSettingsWindow Instance { get; private set; }

        public MpAvSettingsWindow() {
            if (Instance == null) {
                Instance = this;
            }
            AvaloniaXamlLoader.Load(this);
            DataContext = MpAvSettingsWindowViewModel.Instance;
            this.AttachedToVisualTree += MpAvSettingsWindow_AttachedToVisualTree;
            this.Closed += MpAvSettingsWindow_Closed;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MpAvSettingsWindow_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {

        }

        private void MpAvSettingsWindow_Closed(object sender, System.EventArgs e) {
            MpAvSettingsWindowViewModel.Instance.IsVisible = false;
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen &&
                App.MainView is Window w) {
                w.Activate();
            }
        }

    }
}
