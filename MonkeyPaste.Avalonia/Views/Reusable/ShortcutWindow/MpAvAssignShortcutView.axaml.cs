using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAssignShortcutView : MpAvUserControl<MpAvAssignShortcutViewModel> {
        public MpAvAssignShortcutView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
