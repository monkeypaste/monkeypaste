using Avalonia.Markup.Xaml;
//using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpFileSystemTriggerPropertyListBoxItemView.xaml
    /// </summary>
    public partial class MpAvAssignableShortcutView : MpAvUserControl<MpAvIShortcutCommandViewModel> {
        public MpAvAssignableShortcutView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
