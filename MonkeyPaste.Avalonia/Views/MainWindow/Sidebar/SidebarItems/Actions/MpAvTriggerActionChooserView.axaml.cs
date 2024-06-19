using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAnalyticItemSelectorView.xaml
    /// </summary>
    public partial class MpAvTriggerActionChooserView : MpAvUserControl<MpAvTriggerCollectionViewModel>, MpAvISidebarContentView, MpAvIFocusHeaderMenuView {
        private IEnumerable<(ScrollViewer,Vector)> _offsets;
        public MpAvTriggerActionChooserView() {
            InitializeComponent();
            this.AddHandler(PointerPressedEvent, MpAvTriggerActionChooserView_PointerPressed, RoutingStrategies.Tunnel);
            this.AddHandler(PointerReleasedEvent, MpAvTriggerActionChooserView_PointerReleased, RoutingStrategies.Tunnel);
        }

        private void MpAvTriggerActionChooserView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(MpAvMainView.Instance.SelectedSidebarContainerBorder.GetVisualDescendants<ScrollViewer>() is not { } svl ||
                e.Source is not Control c ||
                c.GetVisualAncestor<ScrollBar>() != null) {
                return;
            }
            //svl.ForEach(x => x.IsEnabled = false);
            //_offsets = svl.Select(x => (x, new Vector(x.Offset.X,x.Offset.Y)));
            //bool test = _offsets.Any(x => (x.Item1.Parent as Control).Name == "SelectedSidebarContentControl");
        }

        private void MpAvTriggerActionChooserView_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (MpAvMainView.Instance.SelectedSidebarContainerBorder.GetVisualDescendants<ScrollViewer>() is not { } svl ||
                _offsets == null) {
                return;
            }
            //svl.ForEach(x => x.IsEnabled = true);

            //Dispatcher.UIThread.Post(async () => {
            //    await Task.Delay(300);

            //    _offsets.ForEach(x => x.Item1.Offset = x.Item2);
            //    _offsets = null;
            //});

        }

        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if(!MpAvThemeViewModel.Instance.IsMobileOrWindowed ||
                sender is not ScrollViewer sv ||
                this.GetVisualDescendant<MpAvZoomBorder>() is not { } zb ||
                !zb.IsKeyboardFocusWithin) {
                return;
            }

            sv.Offset -= e.OffsetDelta;
        }
    }
}
