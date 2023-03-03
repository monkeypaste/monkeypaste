using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSidebarButtonGroupView : MpAvUserControl<MpAvClipTrayViewModel> {

        public MpAvSidebarButtonGroupView() {
            AvaloniaXamlLoader.Load(this);
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            var amb = this.FindControl<Control>("AppModeToggleButton");
            //amb.PointerReleased += Amb_PointerReleased;

            if (FlyoutBase.GetAttachedFlyout(amb) is Flyout fo) {

                fo.Closing += Fb_Closing;
            }
        }
        private void Fb_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (MpAvSidebarItemCollectionViewModel.Instance.SelectedItemIdx == 4) {
                // mode changes trigger ntf windows that deactivate mw and close flyout
                // force flyout to be closed by toggling sidebar button
                e.Cancel = true;
            }
        }
        private void Amb_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (sender is Control c) {
                FlyoutBase.ShowAttachedFlyout(c);
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.SidebarItemSizeChanged:
                    if (MpAvSidebarItemCollectionViewModel.Instance.SelectedItemIdx != 4) {
                        var amb = this.FindControl<Control>("AppModeToggleButton");
                        if (FlyoutBase.GetAttachedFlyout(amb) is Flyout fo) {
                            fo.Hide();
                        }
                    }
                    break;
                case MpMessageType.MainWindowOrientationChangeEnd:
                    var ctg = this.FindControl<Grid>("SidebarButtonGroupContainerGrid");
                    var tbl = ctg.GetVisualDescendants<Button>().ToList();

                    if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        // horizontal shows sidebar down left side
                        ctg.ColumnDefinitions.Clear();
                        ctg.RowDefinitions = new RowDefinitions("*,*,*,*,*");
                        tbl.ForEach(x => Grid.SetColumn(x, 0));
                        tbl.ForEach(x => Grid.SetRow(x, tbl.IndexOf(x)));
                    } else {
                        // vertical shows sidebar across bottom
                        ctg.RowDefinitions.Clear();
                        ctg.ColumnDefinitions = new ColumnDefinitions("*,*,*,*,*");
                        tbl.ForEach(x => Grid.SetRow(x, 0));
                        tbl.ForEach(x => Grid.SetColumn(x, tbl.IndexOf(x)));
                    }
                    break;
            }
        }
    }
}
