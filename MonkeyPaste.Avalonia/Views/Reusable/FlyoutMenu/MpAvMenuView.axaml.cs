using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMenuView : ContextMenu {
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
            AvaloniaXamlLoader.Load(this);
            this.GetObservable(IsOpenProperty).Subscribe(value => OnIsOpenChanged());
            this.Closed += MpAvMenuView_Closed;

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
    }
}
