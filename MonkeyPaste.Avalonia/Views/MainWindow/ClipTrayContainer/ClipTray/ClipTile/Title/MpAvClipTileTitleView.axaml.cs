using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls.Shapes;
using MonkeyPaste.Common;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTitleView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileTitleHighlightBehavior ClipTileTitleHighlightBehavior { get; private set; }
        public MpAvSourceHighlightBehavior SourceHighlightBehavior { get; private set; }

        public MpAvClipTileTitleView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvClipTileTitleView_AttachedToVisualTree;
        }

        private void MpAvClipTileTitleView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            ClipTileTitleHighlightBehavior = new MpAvClipTileTitleHighlightBehavior();
            ClipTileTitleHighlightBehavior.Attach(this);

            SourceHighlightBehavior = new MpAvSourceHighlightBehavior();
            SourceHighlightBehavior.Attach(this);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void SourceIconGrid_PointerPressed(object sender, PointerPressedEventArgs e) {
            var ctv = this.GetVisualAncestor<MpAvClipTileView>();
            var wv = ctv.GetVisualDescendant<WebViewControl.WebView>();
            if(wv != null) {
                wv.ShowDeveloperTools();
                return;
            }
            var cwv = ctv.GetVisualDescendant<MpAvCefNetWebView>();
            if(cwv != null) {
                cwv.ShowDevTools();
            }
        }
    }
}
