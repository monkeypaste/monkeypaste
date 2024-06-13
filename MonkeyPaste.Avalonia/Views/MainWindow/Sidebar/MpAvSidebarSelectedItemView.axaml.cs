using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia;
[DoNotNotify]
public partial class MpAvSidebarSelectedItemView : MpAvUserControl<MpAvSidebarItemCollectionViewModel>
{
    public MpAvSidebarSelectedItemView()
    {
        InitializeComponent();
        SelectedSidebarContentControl.Loaded += SelectedSidebarContentControl_Loaded;
    }

    private void SelectedSidebarContentControl_Loaded(object sender, RoutedEventArgs e) {
        AttachScrollHandlers();
    }


    private void AttachScrollHandlers() {

        if (this.GetVisualDescendant<ScrollViewer>() is not { } sv) {
            Dispatcher.UIThread.Post(async () => {
                sv = await this.GetVisualDescendantAsync<ScrollViewer>(timeOutMs: -1);
                sv.AddHandler(ScrollViewer.ScrollChangedEvent, Sv_ScrollChanged, RoutingStrategies.Tunnel);
            });
            return;
        }

        sv.ScrollChanged += Sv_ScrollChanged;
    }

    private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e) {

    }
}