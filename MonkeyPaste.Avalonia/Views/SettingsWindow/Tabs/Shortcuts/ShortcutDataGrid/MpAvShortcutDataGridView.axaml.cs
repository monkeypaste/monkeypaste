using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutDataGridView : UserControl {
        public MpAvShortcutDataGridView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
