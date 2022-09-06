using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;
using PropertyChanged;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAssignShortcutWindow : Window {

        public bool DialogResult { get; set; } = false;
        public MpAvAssignShortcutWindow() {
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
            if(DataContext == null) {
                return;
            }
            if(DataContext is MpAvAssignShortcutModalWindowViewModel asmwvm) {
                //var dg = this.FindControl<DataGrid>("ShortcutDataGrid");
                //var cv = new DataGridCollectionView(asmwvm.KeyItems);
                //cv.GroupDescriptions.Add(new DataGridPathGroupDescription("SeqIdx"));
                //dg.Items = cv;
            }
        }

        private void MpAvAssignShortcutWindow_Opened(object sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
           
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Instance_OnGlobalKeyReleased;
        }

        private void MpAvAssignShortcutWindow_Closed(object sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= Instance_OnGlobalKeyPressed;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= Instance_OnGlobalKeyReleased;
        }
        

        private void Instance_OnGlobalKeyPressed(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!this.IsActive) {
                    return;
                }

                if (DataContext is MpAvAssignShortcutModalWindowViewModel asmwvm) {
                    asmwvm.AddKeyDownCommand.Execute(keyStr);
                }
            });
        }
        private void Instance_OnGlobalKeyReleased(object sender, string keyStr) {
            Dispatcher.UIThread.Post(() => {
                if (!this.IsActive) {
                    return;
                }

                if (DataContext is MpAvAssignShortcutModalWindowViewModel asmwvm) {
                    asmwvm.RemoveKeyDownCommand.Execute(keyStr);
                }
            });
            
        }
        private void Ok_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            (DataContext as MpAvAssignShortcutModalWindowViewModel).OkCommand.Execute(null);
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
