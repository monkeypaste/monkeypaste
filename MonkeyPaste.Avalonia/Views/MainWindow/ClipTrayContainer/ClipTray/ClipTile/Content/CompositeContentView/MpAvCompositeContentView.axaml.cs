using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia;

public partial class MpAvCompositeContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvCompositeContentView() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        InitDnd();
    }
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

}