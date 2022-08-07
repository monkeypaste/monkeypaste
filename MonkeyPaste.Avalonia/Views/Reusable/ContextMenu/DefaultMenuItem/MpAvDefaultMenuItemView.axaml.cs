using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;
using MonkeyPaste;
using Avalonia.Styling;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvDefaultMenuItemView : MenuItem, IStyleable {
        Type IStyleable.StyleKey => typeof(MenuItem);
        public MpAvDefaultMenuItemView() {
            InitializeComponent();
            this.PointerPressed += MpAvDefaultMenuItemView_PointerPressed;
        }

        private void MpAvDefaultMenuItemView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(DataContext is MpMenuItemViewModel mivm && mivm.SubItems != null && mivm.SubItems.Count > 0) {
                //don't close tree item parents
                return;
            }
            MpAvMenuExtension.CloseMenu();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
