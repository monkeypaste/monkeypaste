using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PropertyChanged;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAssignShortcutDialog : MpAvWindow {

        public bool DialogResult { get; set; } = false;
        public MpAvAssignShortcutDialog() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Closed += MpAvAssignShortcutWindow_Closed;
            this.Opened += MpAvAssignShortcutWindow_Opened;
            this.DataContextChanged += MpAvAssignShortcutWindow_DataContextChanged;
            var lb = this.FindControl<ListBox>("ShortcutListBox");

            //this.KeyDown += MpAvAssignShortcutWindow_KeyDown;
            //this.KeyUp += MpAvAssignShortcutWindow_KeyUp;
        }

        private void MpAvAssignShortcutWindow_DataContextChanged(object sender, System.EventArgs e) {
            if (DataContext == null) {
                return;
            }
            if (DataContext is MpAvAssignShortcutViewModel asmwvm) {
                //var dg = this.FindControl<DataGrid>("ShortcutDataGrid");
                //var cv = new DataGridCollectionView(asmwvm.KeyItems);
                //cv.GroupDescriptions.Add(new DataGridPathGroupDescription("SeqIdx"));
                //dg.Items = cv;
            }
        }

        private void MpAvAssignShortcutWindow_Opened(object sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = true;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Instance_OnGlobalKeyReleased;
        }

        private void MpAvAssignShortcutWindow_Closed(object sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = false;
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

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
