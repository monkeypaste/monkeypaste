using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
