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

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvContextMenuView : ContextMenu, IStyleable, MpIContextMenuCloser {
        private static MpAvContextMenuView _instance;
        public static MpAvContextMenuView Instance => _instance ?? (_instance = new MpAvContextMenuView());
        Type IStyleable.StyleKey => typeof(ContextMenu);

        public bool IsShowingChildDialog { get; set; } = false;

        public MpAvContextMenuView() {
            InitializeComponent();
        }


        private void MpAvContextMenuView_DataContextChanged(object sender, System.EventArgs e) {
            //if(DataContext is MpMenuItemViewModel mivm) {
            //    var mil = new List<TemplatedControl>();
            //    foreach(var cmivm in mivm.SubItems) {
            //        if (cmivm.IsSeparator) {
            //            mil.Add(new Separator());
            //        } else if(cmivm.IsColorPallete) {

            //        } else {
            //            var mi = new MenuItem() {
            //                MinWidth = 100,
            //                MinHeight = 30,
            //                Header = cmivm.Header,
            //                Icon = new MpAvIconSourceObjToBitmapConverter().Convert(cmivm.IconSourceObj, null, null, null),
            //                Command = cmivm.Command,
            //                CommandParameter = cmivm.CommandParameter
            //                //InputGesture = new KeyGesture()
            //            };
            //            mil.Add(mi);
            //        }                    
            //    }
            //    this.Items = mil;
            //}
        }

        private void MpAvContextMenuView_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
        }

        private void MpAvContextMenuView_ContextMenuClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if(IsShowingChildDialog) {
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
}
