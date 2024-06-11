using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Linq;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia;

public partial class MpAvCompositeContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvCompositeContentView() {
        InitializeComponent();
        EditableTextContentControl.EffectiveViewportChanged += EditableTextContentControl_EffectiveViewportChanged;
    }


    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        InitDnd();
    }


    private void EditableTextContentControl_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
        // seems intermittent or maybe platform specific (happens on windows)
        // but when webview is shrunk to 0 size the panel doesn't resize and gets stuck,
        // toggling visibility fixes it, might be a more direct way but it works...
        if(EditableTextContentControl.Width != 0 || EditableTextContentControl.Height != 0 || !EditableTextContentControl.IsVisible) {
            return;
        }
        Dispatcher.UIThread.Post(async () => {
            await Task.Delay(100);
            EditableTextContentControl.IsVisible = false;
            await Task.Delay(300);
            EditableTextContentControl.IsVisible = true;
        });
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