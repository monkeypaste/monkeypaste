using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvCheckableMenuItemView : MenuItem {
        #region Overrides
        protected override Type StyleKeyOverride => typeof(MenuItem);
        #endregion
        public MpAvCheckableMenuItemView() {
            InitializeComponent();
            this.PointerPressed += MpAvCheckableMenuItemView_PointerPressed;
            //var cb = this.FindControl<CheckBox>("MenuItemCheckBox");
            //cb.PointerPressed += Cb_PointerPressed;
            //new CheckBox().Checked += MpAvCheckableMenuItemView_Checked;
        }

        private void MpAvCheckableMenuItemView_Checked(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (!this.IsInitialized) {
                return;
            }
        }

        private void Cb_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            return;
        }

        private void MpAvCheckableMenuItemView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (sender is CheckBox ||
                (sender is Control control && control.GetVisualAncestor<CheckBox>() != null)) {
                //don't close tree item parents
                MpAvMenuExtension.CloseMenu();
            }

        }
    }
}
