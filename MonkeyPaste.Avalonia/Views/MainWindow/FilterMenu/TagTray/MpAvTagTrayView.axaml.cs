using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.VisualTree;
using Avalonia.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTrayView : MpAvUserControl<MpAvTagTrayViewModel> {
        public MpAvTagTrayView() {
            InitializeComponent();
            var lb = this.FindControl<ListBox>("TagTray");
            lb.EffectiveViewportChanged += Lb_EffectiveViewportChanged;
            Dispatcher.UIThread.Post(async () => {
                var wp = lb.GetVisualDescendant<WrapPanel>();
                while(wp == null) {
                    wp = lb.GetVisualDescendant<WrapPanel>();
                    if(wp != null) {
                        break;
                    }
                    await Task.Delay(100);
                }
                wp.EffectiveViewportChanged += Wp_EffectiveViewportChanged;
            });
            Dispatcher.UIThread.Post(async () => {
                var sv = lb.GetVisualDescendant<ScrollViewer>();
                while (sv == null) {
                    sv = lb.GetVisualDescendant<ScrollViewer>();
                    if (sv != null) {
                        break;
                    }
                    await Task.Delay(100);
                }
                sv.EffectiveViewportChanged += Sv_EffectiveViewportChanged;
            });

        }

        private void Lb_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            //MpConsole.WriteLine("TagTray lb size: " + e.EffectiveViewport);
        }

        private void Sv_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            //MpConsole.WriteLine("TagTray Sv size: " + e.EffectiveViewport);
        }

        private void Wp_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
           // MpConsole.WriteLine("TagTray Wp size: " + e.EffectiveViewport);
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
