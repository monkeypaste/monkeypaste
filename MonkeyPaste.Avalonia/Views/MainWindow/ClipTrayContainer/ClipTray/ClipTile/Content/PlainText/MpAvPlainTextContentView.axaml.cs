using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using CefNet.Avalonia;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPlainTextContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvPlainTextContentView() {
            InitializeComponent();            
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
