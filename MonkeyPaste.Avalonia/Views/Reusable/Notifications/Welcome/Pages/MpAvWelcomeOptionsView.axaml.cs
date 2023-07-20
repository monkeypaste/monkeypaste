using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeOptionsView : MpAvUserControl<MpAvWelcomeOptionGroupViewModel> {
        public MpAvWelcomeOptionsView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
