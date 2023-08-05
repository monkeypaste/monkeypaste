using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvCheckBoxParameterView : MpAvUserControl<MpAvCheckBoxParameterViewModel> {
        public MpAvCheckBoxParameterView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
