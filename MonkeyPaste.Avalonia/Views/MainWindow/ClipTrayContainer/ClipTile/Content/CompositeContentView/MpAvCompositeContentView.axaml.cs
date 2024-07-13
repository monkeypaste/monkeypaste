using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ICSharpCode.SharpZipLib;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia;

public partial class MpAvCompositeContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvCompositeContentView() {
        InitializeComponent(); 
        EditableWebView.EffectiveViewportChanged += EditableTextContentControl_EffectiveViewportChanged;

    }
    private void EditableTextContentControl_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
        // seems intermittent or maybe platform specific (happens on windows)
        // but when webview is shrunk to 0 size the panel doesn't resize and gets stuck,
        // toggling visibility fixes it, might be a more direct way but it works...
        if (EditableWebView.Width != 0 || EditableWebView.Height != 0 || !EditableWebView.IsVisible) {
            return;
        }
        Dispatcher.UIThread.Post(async () => {
            MpConsole.WriteLine($"Fixing wv for zero size");
            await Task.Delay(100);
            EditableWebView.IsVisible = false;
            await Task.Delay(300);
            EditableWebView.IsVisible = true;
        });
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        InitDnd();
    }

    #region Dnd
    private void InitDnd() {
        ReadOnlyWebView.AddHandler(PointerPressedEvent, ReadOnlyWebView_PointerPressed, RoutingStrategies.Tunnel);
    }
    private void ReadOnlyWebView_PointerPressed(object sender, PointerPressedEventArgs e) {
#if MOBILE
            return;
#else
        if (sender is not HtmlPanel hp ||
            !e.IsLeftPress(hp) ||
            hp.IsSelectionEnabled ||
            BindingContext is not MpAvClipTileViewModel ctvm ||
            !ctvm.CanDrag ||
            ctvm.GetContentView() is not MpAvIContentWebViewDragSource cv) {
            return;
        }
        cv.LastPointerPressedEventArgs = e;

        this.DragCheckAndStart(e,
            start: async (start_e) => {
                await MpAvContentWebViewDragHelper.StartDragAsync(cv, DragDropEffects.Copy | DragDropEffects.Move);
            },
            move: (move_e) => {
            },
            end: (end_e) => {

                //BindingContext.IsTileDragging = false;
                //ended = true;
            },
            MIN_DISTANCE: 20);

#endif
    }

    #endregion

}