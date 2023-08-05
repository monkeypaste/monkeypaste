using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvDefaultMenuItemView : MpAvUserControl<MpAvMenuItemViewModel> {
        #region Overrides
        protected override Type StyleKeyOverride => typeof(MenuItem);
        #endregion
        public MpAvDefaultMenuItemView() {
            AvaloniaXamlLoader.Load(this);
            this.PointerPressed += MpAvDefaultMenuItemView_PointerPressed;
        }

        private void MpAvDefaultMenuItemView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (DataContext is MpAvMenuItemViewModel mivm && mivm.SubItems != null && mivm.SubItems.Count > 0) {
                //don't close tree item parents
                return;
            }
            MpAvMenuExtension.CloseMenu();
        }
    }
}
