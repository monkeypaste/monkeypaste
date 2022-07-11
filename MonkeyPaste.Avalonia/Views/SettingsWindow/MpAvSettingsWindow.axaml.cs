using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSettingsWindow : Window {
        //private static MpAvSettingsWindow _instance;
        //public static MpAvSettingsWindow Instance => _instance ?? (_instance = new MpAvSettingsWindow());
        public MpAvSettingsWindow() { 
            InitializeComponent();
            DataContext = MpAvSettingsWindowViewModel.Instance;
            this.Closed += MpAvSettingsWindow_Closed;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MpAvSettingsWindow_Closed(object sender, System.EventArgs e) {
            MpAvSettingsWindowViewModel.Instance.IsVisible = false;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
