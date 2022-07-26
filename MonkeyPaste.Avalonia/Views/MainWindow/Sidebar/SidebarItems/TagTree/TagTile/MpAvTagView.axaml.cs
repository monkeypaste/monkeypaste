using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using PropertyChanged;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTagView : MpAvUserControl<MpAvTagTileViewModel> {
        public bool IsTreeTag { get; set; } = false;

        public MpAvTagView() {
            InitializeComponent();
            var tagViewContainerDockPanel = this.FindControl<DockPanel>("TagViewContainerDockPanel");
            tagViewContainerDockPanel.AddHandler(DockPanel.PointerPressedEvent, TagViewContainerDockPanel_PointerPressed, RoutingStrategies.Tunnel);

            var tagNameBorder = this.FindControl<MpAvClipBorder>("TagNameBorder");
            tagNameBorder.PointerPressed += TagNameBorder_PointerPressed;

            var tagNameTextBox = this.FindControl<TextBox>("TagNameTextBox");
            tagNameTextBox.AddHandler(TextBox.KeyDownEvent, TagNameTextBox_KeyDown, RoutingStrategies.Tunnel);
            tagNameTextBox.GetObservable(TextBox.IsVisibleProperty).Subscribe(value => {
                if(!value) {
                    return;
                }
                Dispatcher.UIThread.Post(async () => {
                    await Task.Delay(500);
                    tagNameTextBox.SelectAll();
                    //MpAvIsFocusedExtension.SetIsFocused(tagNameTextBox, true);
                    tagNameTextBox.Focus();
                });
            });
        }

        private void TagNameTextBox_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                BindingContext.FinishRenameTagCommand.Execute(null);

            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                BindingContext.CancelRenameTagCommand.Execute(null);
            }
        }


        private void TagNameBorder_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                return;
            }
            if(e.ClickCount > 1) {
                BindingContext.RenameTagCommand.Execute(IsTreeTag);
            } else if (BindingContext.IsSelected) {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged();
            }
            //MpDragDropManager.StartDragCheck(BindingContext);
        }

        private void TagViewContainerDockPanel_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
