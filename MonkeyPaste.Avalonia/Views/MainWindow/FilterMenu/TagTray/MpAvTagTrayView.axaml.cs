using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.VisualTree;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTrayView : MpAvUserControl<MpAvTagTrayViewModel> {
        public MpAvTagTrayView() {
            InitializeComponent();
        }
        
        private void NavRightRepeatButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is ListBox TagTray) {
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

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
