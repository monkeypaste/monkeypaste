using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using CefNet.Avalonia;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvCefNetContentWebView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvCefNetContentWebView() {
            InitializeComponent();         
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }



    }
}
