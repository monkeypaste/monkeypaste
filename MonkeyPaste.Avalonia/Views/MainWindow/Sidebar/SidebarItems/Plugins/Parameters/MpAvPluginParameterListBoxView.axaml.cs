using Avalonia;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPluginParameterListBoxView : MpAvUserControl<MpAvIParameterCollectionViewModel> {
        public MpAvPluginParameterListBoxView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
