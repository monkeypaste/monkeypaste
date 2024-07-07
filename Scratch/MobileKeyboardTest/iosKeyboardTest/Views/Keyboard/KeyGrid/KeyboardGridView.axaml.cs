using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;

using System;
using System.Linq;

namespace iosKeyboardTest
{

    public partial class KeyboardGridView : UserControl {
        KeyboardViewModel BindingContext =>
            DataContext as KeyboardViewModel;

        public KeyboardGridView() 
        {
            InitializeComponent();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            BindingContext.SetPointerLocation(e.GetPosition(this));
        }
        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);

            if (OperatingSystem.IsWindows() &&
                !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                // ignore mouse movement on desktop
                BindingContext.SetPointerLocation(null);
                return;
            }
            BindingContext.SetPointerLocation(e.GetPosition(this));
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            BindingContext.SetPointerLocation(null);
        }
    }
}