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
        #region Private Variables

        #endregion


        public async Task ReloadContentAsync(string cacheState) {
            await Dispatcher.UIThread.InvokeAsync(async () => {

                var ctcv = this.FindControl<MpAvClipTileContentView>("ClipTileContentView");
                var cc = ctcv.FindControl<ContentControl>("ClipTileContentControl");
                var wv = cc.GetVisualDescendant<MpAvCefNetWebView>();
                if(wv == null) {
                    return;
                }
                if(string.IsNullOrWhiteSpace(cacheState)) {
                    BindingContext.CachedState = await wv.EvaluateJavascriptAsync($"getState_ext()");
                } else {
                    BindingContext.CachedState = cacheState;
                }
                
                BindingContext.IsSelected = false;

                if (cc.DataTemplates.ElementAt(0) is MpAvClipTileContentDataTemplateSelector selector) {
                    cc.Content = selector.Build(BindingContext);
                    MpConsole.WriteLine($"Reload of {BindingContext.CopyItemTitle} successful.");
                    return;
                }
                MpConsole.WriteLine($"Reload of {BindingContext.CopyItemTitle} failed.");
                //cc.ApplyTemplate();
            });
        }

        public MpAvClipTileView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
