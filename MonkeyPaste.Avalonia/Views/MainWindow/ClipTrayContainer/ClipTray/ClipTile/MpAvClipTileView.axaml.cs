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


        public void ReloadContent() {
            Dispatcher.UIThread.Post(async () => {

                var ctcv = this.FindControl<MpAvClipTileContentView>("ClipTileContentView");
                var cc = ctcv.FindControl<ContentControl>("ClipTileContentControl");
                var wv = cc.GetVisualDescendant<MpAvCefNetWebView>();
                if(wv == null) {
                    return;
                }
                BindingContext.CachedState = await wv.EvaluateJavascriptAsync($"getState_ext()");
                BindingContext.IsSelected = false;

                if (cc.DataTemplates.ElementAt(0) is MpAvClipTileContentDataTemplateSelector selector) {
                    cc.Content = selector.Build(BindingContext);
                }
                
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
