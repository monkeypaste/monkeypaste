using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvComboBoxParameterView : MpAvUserControl<MpAvSingleEnumerableParameterViewModel> {
        public MpAvComboBoxParameterView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
