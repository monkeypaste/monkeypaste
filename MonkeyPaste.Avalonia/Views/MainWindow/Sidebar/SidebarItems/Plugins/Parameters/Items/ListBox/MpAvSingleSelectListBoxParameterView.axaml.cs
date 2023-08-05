using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvSingleSelectListBoxParameterView : MpAvUserControl<MpAvEnumerableParameterViewModelBase> {
        public MpAvSingleSelectListBoxParameterView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
