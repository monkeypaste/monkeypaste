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
using Avalonia.Animation;
using System.Threading;
using Avalonia.VisualTree;
using Avalonia.Styling;
using System.Globalization;
using Avalonia.Animation.Easings;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTitleView : MpAvUserControl<MpAvClipTileViewModel> {

        public MpAvClipTileTitleView() {
            InitializeComponent();
            var sb = this.FindControl<Button>("ClipTileAppIconImageButton");
            sb.AddHandler(Button.PointerPressedEvent, Sb_PointerPressed);
        }

        private void Sb_PointerPressed(object sender, PointerPressedEventArgs e) {
            if(!e.IsRightPress(e.Source as Control)) {
                return;
            }
            if (this.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv &&
                ctv.GetVisualDescendant<MpAvCefNetWebView>() is MpAvCefNetWebView wv) {
                wv.ShowDevTools();
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void SourceIconGrid_PointerPressed(object sender, PointerPressedEventArgs e) {
            //if(BindingContext.GetContentView() is MpAvCefNetWebView wv) {
            //    wv.ShowDevTools();
            //}
            
        }
    }
}
