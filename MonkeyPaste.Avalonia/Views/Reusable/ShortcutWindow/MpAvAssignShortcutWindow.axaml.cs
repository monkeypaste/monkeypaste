using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvAssignShortcutWindow : Window {
        private MpAvKeyGestureHelper<Key> _gestureHelper;

        public bool DialogResult { get; set; } = false;
        public MpAvAssignShortcutWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Closed += MpAvAssignShortcutWindow_Closed;
            this.Opened += MpAvAssignShortcutWindow_Opened;
            this.DataContextChanged += MpAvAssignShortcutWindow_DataContextChanged;
            this.KeyDown += MpAvAssignShortcutWindow_KeyDown;
            this.KeyUp += MpAvAssignShortcutWindow_KeyUp;
            _gestureHelper = new MpAvKeyGestureHelper<Key>(GetPriority);
        }


        private int GetPriority(Key key) {
            switch (key) {
                case Key.LeftCtrl:
                    return 0;
                case Key.RightCtrl:
                    return 1;
                //case Key.System:
                case Key.LeftAlt:
                    return 2;
                case Key.RightAlt:
                    return 3;
                case Key.LeftShift:
                    return 4;
                case Key.RightShift:
                    return 5;
                default:
                    return 6;
            }
        }

        private void MpAvAssignShortcutWindow_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            _gestureHelper.AddKeyUp(e.Key);
            if (DataContext is MpAvAssignShortcutModalWindowViewModel asmwvm) {
                asmwvm.SetKeyList(MpAvKeyboardInputHelpers.ConvertStringToKeySequence(_gestureHelper.CurrentGesture));
            }
        }

        private void MpAvAssignShortcutWindow_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            _gestureHelper.AddKeyDown(e.Key);
            if (DataContext is MpAvAssignShortcutModalWindowViewModel asmwvm) {
                asmwvm.SetKeyList(MpAvKeyboardInputHelpers.ConvertStringToKeySequence(_gestureHelper.CurrentGesture));
            }
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
            _gestureHelper.Reset();            
        }

        private void MpAvAssignShortcutWindow_Closed(object sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
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
