using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTrayView : MpAvUserControl<MpAvTagTrayViewModel> {
        public MpAvTagTrayView() {
            InitializeComponent();

            var nav_left = this.FindControl<RepeatButton>("TagTrayNavLeftButton");
            var nav_right = this.FindControl<RepeatButton>("TagTrayNavRightButton");
            nav_left.Click += NavLeftRepeatButton_Click;
            nav_right.Click += NavRightRepeatButton_Click;
        }


        private void NavRightRepeatButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.FindControl<ListBox>("TagTray") is ListBox ttrlb &&
                ttrlb.GetVisualDescendant<ScrollViewer>() is ScrollViewer sv) {
                sv.ScrollToHorizontalOffset(sv.Offset.X + 20);
            }
        }

        private void NavLeftRepeatButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.FindControl<ListBox>("TagTray") is ListBox ttrlb &&
                ttrlb.GetVisualDescendant<ScrollViewer>() is ScrollViewer sv) {
                sv.ScrollToHorizontalOffset(sv.Offset.X - 20);
            }
        }
    }
}
