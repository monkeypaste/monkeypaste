using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Styling;
using System;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia.Controls.Generators;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvContextMenuView : ContextMenu, IStyleable, MpIContextMenuCloser {
        private static MpAvContextMenuView _instance;
        public static MpAvContextMenuView Instance => _instance ?? (_instance = new MpAvContextMenuView());
        Type IStyleable.StyleKey => typeof(ContextMenu);

        public bool IsShowingChildDialog { get; set; } = false;

        public MpAvContextMenuView() {
            InitializeComponent();
            this.Initialized += MpAvContextMenuView_Initialized;
        }

        private void MpAvContextMenuView_Initialized(object sender, EventArgs e) {
            (this.VisualRoot as PopupRoot).AttachDevTools();
        }


        private void MpAvContextMenuView_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
            return;
        }

        private void MpAvContextMenuView_ContextMenuClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            //e.Cancel = true;            
            if (IsShowingChildDialog) {
                e.Cancel = true;
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

        private void ColorButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if(sender is Control control && control.DataContext is MpMenuItemViewModel mivm) {
                if (!mivm.IsCustomColorButton) {
                    CloseMenu();
                }
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
    [DoNotNotify]
    public class MpAvMenuItem : MenuItem, IStyleable {
        //Type IStyleable.StyleKey => typeof(MenuItem);
    }
}
