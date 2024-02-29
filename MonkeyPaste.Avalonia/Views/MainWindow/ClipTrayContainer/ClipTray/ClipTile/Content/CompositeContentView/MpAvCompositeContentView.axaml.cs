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
        ReadOnlyWebView.AddHandler(PointerPressedEvent, MpAvTagView_PointerPressed, RoutingStrategies.Tunnel);
    }
    private void MpAvTagView_PointerPressed(object sender, PointerPressedEventArgs e) {
#if MOBILE
            return;
#else
        if (sender is not HtmlPanel hp) {
            return;
        }

        if (!e.IsLeftPress(hp) ||
            hp.IsSelectionEnabled) {
            return;
        }

        this.DragCheckAndStart(e,
            start: async (start_e) => {
                int ciid = BindingContext.CopyItemId;
                BindingContext.IsTileDragging = true;
                var avdo = BindingContext.CopyItem.ToAvDataObject(includeSelfRef: true, includeTitle: true);
                await avdo.MapAllPseudoFormatsAsync();
                var result = await MpAvDoDragDropWrapper.DoDragDropAsync(hp, e, avdo, DragDropEffects.Copy);
                if (MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == ciid) is not { } ctvm) {
                    return;
                }
                ctvm.IsTileDragging = false;

            },
            move: (move_e) => {

            },
            end: (end_e) => {

                BindingContext.IsTileDragging = false;
                //ended = true;
            },
            MIN_DISTANCE: 20);

#endif
    }

}