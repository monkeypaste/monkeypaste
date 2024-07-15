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
        public static Canvas DebugCanvas { get; private set; }

        public KeyboardViewModel BindingContext =>
            DataContext as KeyboardViewModel;

        public KeyboardGridView() 
        {
            InitializeComponent();
            DebugCanvas = this.DebugCanvasOverlay;
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            BindingContext.SetPointerLocation(new TouchEventArgs(e.GetPosition(this),TouchEventType.Press));
        }
        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);

            if (OperatingSystem.IsWindows() &&
                !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                // ignore mouse movement on desktop
                //BindingContext.SetPointerLocation(null,false);
                return;
            }
            BindingContext.SetPointerLocation(new TouchEventArgs(e.GetPosition(this), TouchEventType.Move));
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            BindingContext.SetPointerLocation(new TouchEventArgs(e.GetPosition(this), TouchEventType.Release));
        }
    }
}