using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileSortView : MpAvUserControl<MpAvFilterMenuViewModel> {
        public MpAvClipTileSortView() {
            AvaloniaXamlLoader.Load(this);
            var sdb = this.FindControl<Button>("SortDirectionButton");
            sdb.AddHandler(PointerPressedEvent, Sdb_PointerPressed, RoutingStrategies.Tunnel);
            sdb.PointerPressed += Sdb_PointerPressed;
            //sdb.Tapped += Sdb_Tapped;
            //sdb.DoubleTapped += Sdb_DoubleTapped;
        }


        private void Sdb_DoubleTapped(object sender, global::Avalonia.Input.TappedEventArgs e) {
            var clsdvm = MpAvClipTileSortDirectionViewModel.Instance;
            if (clsdvm.IsExpanded) {
                clsdvm.ToggleSortDirectionCommand.Execute(null);
                return;
            }
        }

        private void Sdb_Tapped(object sender, global::Avalonia.Input.TappedEventArgs e) {
            var clsdvm = MpAvClipTileSortDirectionViewModel.Instance;
            if (clsdvm.IsExpanded) {
                clsdvm.ToggleSortDirectionCommand.Execute(null);
                return;
            }
        }

        bool was_double_click = false;

        private void Sdb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var clsdvm = MpAvClipTileSortDirectionViewModel.Instance;
            if (clsdvm.IsExpanded) {
                clsdvm.ToggleSortDirectionCommand.Execute(null);
                return;
            }

            if (e.ClickCount == 2) {
                was_double_click = true;
                clsdvm.ToggleSortDirectionCommand.Execute(null);
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                while (true) {
                    if (was_double_click) {
                        was_double_click = false;
                        return;
                    }
                    if (sw.ElapsedMilliseconds > 500) {
                        break;
                    }
                    await Task.Delay(100);
                }
                clsdvm.IsExpanded = true;
            });
        }
    }
}
