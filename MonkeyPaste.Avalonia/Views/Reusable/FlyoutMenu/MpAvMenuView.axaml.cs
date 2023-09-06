using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMenuView : ContextMenu {
        protected override Type StyleKeyOverride => typeof(ContextMenu);
        public MpAvIMenuItemCollectionViewModel BindingContext {
            get => DataContext as MpAvIMenuItemCollectionViewModel;
            set => DataContext = value;
        }


        public static MpAvMenuView ShowAt(Control target, MpAvIMenuItemCollectionViewModel dc) {
            target.ContextMenu = new MpAvMenuView() {
                DataContext = dc
            };
            target.ContextMenu.Open();
            return target.ContextMenu as MpAvMenuView;
        }
        public MpAvMenuView() : base() {
            AvaloniaXamlLoader.Load(this);
            this.GetObservable(IsOpenProperty).Subscribe(value => OnIsOpenChanged());
            this.Closed += MpAvMenuView_Closed;
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);

#if DEBUG
            if (this.VisualRoot is PopupRoot pr) {
                pr.AttachDevTools(MpAvWindow.DefaultDevToolOptions);
            }
#endif
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
            BindingContext.IsMenuOpen = IsOpen;
        }
    }
}
