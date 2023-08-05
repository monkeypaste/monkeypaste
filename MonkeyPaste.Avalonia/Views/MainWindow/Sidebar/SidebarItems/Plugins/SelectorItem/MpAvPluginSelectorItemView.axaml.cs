using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPluginSelectorItemView : MpAvUserControl<MpIAsyncComboBoxItemViewModel> {
        public MpAvPluginSelectorItemView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
