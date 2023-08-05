using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutDataGridView : MpAvUserControl<object> {
        public MpAvShortcutDataGridView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
