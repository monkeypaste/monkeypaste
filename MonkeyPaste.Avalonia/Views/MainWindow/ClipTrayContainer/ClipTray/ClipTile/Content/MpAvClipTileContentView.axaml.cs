using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileContentView() {
            InitializeComponent();
            //InitDnd();
        }



    }
}
