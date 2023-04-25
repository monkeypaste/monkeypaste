using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvPluginBrowserView : MpAvUserControl<MpAvPluginBrowserViewModel> {

        public MpAvPluginBrowserView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
