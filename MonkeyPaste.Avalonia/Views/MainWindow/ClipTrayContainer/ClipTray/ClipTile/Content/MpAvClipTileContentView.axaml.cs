using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileContentView() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
