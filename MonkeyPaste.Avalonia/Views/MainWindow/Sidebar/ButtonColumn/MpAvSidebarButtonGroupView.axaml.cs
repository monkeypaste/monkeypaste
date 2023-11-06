using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSidebarButtonGroupView : MpAvUserControl<MpAvClipTrayViewModel> {

        public MpAvSidebarButtonGroupView() {
            InitializeComponent();
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }

        private void Fo_Opening(object sender, System.EventArgs e) {
            var fo = sender as Flyout;
            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                fo.HorizontalOffset = 100;
                fo.VerticalOffset = 5;
            } else {
                fo.HorizontalOffset = 0;
                fo.VerticalOffset = -100;
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
                        ctg.RowDefinitions = new RowDefinitions("*,0,*,*,0");
                        tbl.ForEach(x => Grid.SetColumn(x, 0));
                        tbl.ForEach(x => Grid.SetRow(x, tbl.IndexOf(x)));
                    } else {
                        // vertical shows sidebar across bottom
                        ctg.RowDefinitions.Clear();
                        ctg.ColumnDefinitions = new ColumnDefinitions("*,0,*,*,0");
                        tbl.ForEach(x => Grid.SetRow(x, 0));
                        tbl.ForEach(x => Grid.SetColumn(x, tbl.IndexOf(x)));
                    }
                    break;
            }
        }
    }
}
