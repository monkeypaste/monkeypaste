using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvShortcutRecorderParameterView : MpAvUserControl<MpAvShortcutRecorderParameterViewModel> {
        public MpAvShortcutRecorderParameterView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
