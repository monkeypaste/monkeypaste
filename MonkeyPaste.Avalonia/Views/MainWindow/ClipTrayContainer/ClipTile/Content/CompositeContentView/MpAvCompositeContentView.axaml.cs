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
        CompositeWebViewContainerGrid.Classes.CollectionChanged += Classes_CollectionChanged1;
        EditableTextContentControl.EffectiveViewportChanged += EditableTextContentControl_EffectiveViewportChanged;
        EditableTextContentControl.Classes.CollectionChanged += Classes_CollectionChanged;
    }

    private void Classes_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        
        if(e.NewItems != null && e.NewItems.OfType<string>().Contains("show-read-only")) {
            MpConsole.WriteLine($"Tile '{DataContext}' show-read-only enabled");
            HideWebView();
            ShowReadOnlyView();
        }
    }

    private void Classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        if(e.OldItems != null && e.OldItems.OfType<string>().Any(x=>x == "active")) {

        }
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
        //Dispatcher.UIThread.Post(async () => {
        //    MpConsole.WriteLine($"Fixing wv for zero size");
        //    await Task.Delay(100);
        //    EditableTextContentControl.IsVisible = false;
        //    await Task.Delay(300);
        //    EditableTextContentControl.IsVisible = true;
        //});
    }

    public void HideWebView() {
        EditableTextContentControl.Width = 0;
        EditableTextContentControl.Height = 0;
        EditableTextContentControl.InvalidateAll();
        IsVisible = false;
        MpConsole.WriteLine($"WebView for '{DataContext}' hidden");
        Dispatcher.UIThread.Post(async () => {
            await Task.Delay(3_000);
            IsVisible = true;
        });
    }
    
    private void ShowWebView() {
        EditableTextContentControl.IsVisible = true;
        EditableTextContentControl.Width = double.NaN;
        EditableTextContentControl.Height = double.NaN;
        EditableTextContentControl.InvalidateAll();
        MpConsole.WriteLine($"WebView for '{DataContext}' shown");
    }

    private void ShowReadOnlyView() {
        if(BindingContext.IsPlaceholder) {
            return;
        }
        var pcvl = CompositeWebViewContainerGrid.Children.Where(x => x.Classes.Contains("plain-content-view"));
        if(pcvl.FirstOrDefault(x=>x.Classes.Contains("active")) is not { } pcv) {
            switch(BindingContext.CopyItemType) {
                case MpCopyItemType.FileList:
                    pcv = this.FileListViewer;
                    break;
                case MpCopyItemType.Image:
                    pcv = this.ImageViewer;
                    break;
                case MpCopyItemType.Text:
                    pcv = this.ReadOnlyWebView;
                    break;
                default:
                    pcv = null;
                    break;
            }
            if(pcv == null) {
                MpConsole.WriteLine($"No CopyItemType set. IsAnyPlaceholder: {BindingContext.IsAnyPlaceholder}");
            } else {
                pcv.Classes.Add("active");
            }
        }
        if(pcv == null) {
            MpConsole.WriteLine("No active readonly view set!");
        } else {
            MpConsole.WriteLine($"Active ReadOnly View: '{pcv.Name}'");
            pcv.IsVisible = true;
            pcv.InvalidateAll();
        }
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