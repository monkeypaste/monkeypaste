using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvContextMenuView : ContextMenu {
        private static MpAvContextMenuView _instance;
        public static MpAvContextMenuView Instance => _instance ?? (_instance = new MpAvContextMenuView());

        public bool IsShowingChildDialog { get; set; } = false;

        public MpAvContextMenuView() {
            InitializeComponent();
        }

        private void MpAvContextMenuView_DataContextChanged(object sender, System.EventArgs e) {
            throw new System.NotImplementedException();
        }

        private void MpAvContextMenuView_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
        }

        private void MpAvContextMenuView_ContextMenuClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if(IsShowingChildDialog) {
                return;
            }
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
        }


        private void ContextMenu_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var mivm = (sender as StyledElement).DataContext as MpMenuItemViewModel;
            mivm.Command.Execute(mivm.CommandParameter);

            if (mivm.Command != MpPlatformWrapper.Services.CustomColorChooserMenu.SelectCustomColorCommand) {
                CloseMenu();
            }
        }

        public void CloseMenu() {
            if(IsInitialized && !IsShowingChildDialog) {
                IsOpen = false;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
