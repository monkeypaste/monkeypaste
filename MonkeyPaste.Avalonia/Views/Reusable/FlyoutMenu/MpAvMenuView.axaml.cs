using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMenuView : ContextMenu {
        static bool _IsDevToolsOpen = false;
        protected override Type StyleKeyOverride => typeof(ContextMenu);
        public MpAvIMenuItemViewModel BindingContext {
            get => DataContext as MpAvIMenuItemViewModel;
            set => DataContext = value;
        }

        public static MpAvMenuView ShowMenu(
            Control target,
            MpAvIMenuItemViewModel dc,
            PlacementMode placementMode,
            PopupAnchor popupAnchor,
            MpPoint offset = null) {
            target.ContextMenu = new MpAvMenuView() {
                DataContext = dc,
                Placement = placementMode,
                PlacementAnchor = popupAnchor,
                HorizontalOffset = offset == null ? 0 : offset.X,
                VerticalOffset = offset == null ? 0 : offset.Y,
            };
            target.ContextMenu.Open();
            return target.ContextMenu as MpAvMenuView;
        }
        public MpAvMenuView() : base() {
            InitializeComponent();

            this.GetObservable(IsOpenProperty).Subscribe(value => OnIsOpenChanged());
            this.Closed += MpAvMenuView_Closed;
            this.Closing += MpAvMenuView_Closing;

        }

        private void MpAvMenuView_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = _IsDevToolsOpen;
        }

        private void MpAvMenuView_Closed(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.PlacementTarget is Control target &&
                target.ContextMenu == this) {
                target.ContextMenu = null;
            }
        }

        private void OnIsOpenChanged() {
            if (BindingContext == null) {
                return;
            }
            BindingContext.IsSubMenuOpen = IsOpen;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (e.IsMiddleRelease(this)) {
                _IsDevToolsOpen = !_IsDevToolsOpen;
                if (_IsDevToolsOpen) {
                    bool success = this.Focus();
                    MpConsole.WriteLine($"Context menu focus success: {success}");
                    Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence("F12");
                }
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
#if DEBUG
            if (TopLevel.GetTopLevel(this) is TopLevel tl) {
                tl.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
            }
#endif
        }
    }
}
