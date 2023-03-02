using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTrayView : MpAvUserControl<MpAvTagTrayViewModel> {
        public MpAvTagTrayView() {
            AvaloniaXamlLoader.Load(this);

            var tt_lb = this.FindControl<ListBox>("TagTray");
            //tt_lb.EnableItemsControlAutoScroll();
        }


        private void NavRightRepeatButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is ListBox TagTray) {
                var sv = TagTray.GetVisualDescendant<ScrollViewer>();
                sv.ScrollToHorizontalOffset(sv.Offset.X - 20);
            }
        }

        private void NavLeftRepeatButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is ListBox TagTray) {
                var sv = TagTray.GetVisualDescendant<ScrollViewer>();
                sv.ScrollToHorizontalOffset(sv.Offset.X + 20);
            }
        }
    }
}
