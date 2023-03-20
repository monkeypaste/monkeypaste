using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAssignShortcutDialog : MpAvWindow {
        public MpAvAssignShortcutDialog() {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
            this.Closed += MpAvAssignShortcutWindow_Closed;
            this.Opened += MpAvAssignShortcutWindow_Opened;
        }


        private void MpAvAssignShortcutWindow_Opened(object sender, System.EventArgs e) {
            //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Instance_OnGlobalKeyReleased;
        }

        private void MpAvAssignShortcutWindow_Closed(object sender, System.EventArgs e) {
            //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= Instance_OnGlobalKeyReleased;
        }


        private void Instance_OnGlobalKeyPressed(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!this.IsActive) {
                    return;
                }

                if (DataContext is MpAvAssignShortcutViewModel asmwvm) {
                    asmwvm.AddKeyDownCommand.Execute(keyStr);
                }
            });
        }
        private void Instance_OnGlobalKeyReleased(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!this.IsActive) {
                    return;
                }

                if (DataContext is MpAvAssignShortcutViewModel asmwvm) {
                    asmwvm.RemoveKeyDownCommand.Execute(keyStr);
                }
            });

        }
        private void Ok_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            (DataContext as MpAvAssignShortcutViewModel).OkCommand.Execute(null);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

    }
}
