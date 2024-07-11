using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace iosKeyboardTest {
    public interface IKeyboardInputConnection
    {
        void OnText(string text);
        void OnDelete();
        void OnDone();
        void OnNavigate(int dx, int dy);
    }
    public interface IKeyboardInputConnection_ios : IKeyboardInputConnection, IHeadlessRender {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
    public interface IKeyboardInputConnection_desktop : IKeyboardInputConnection, IHeadLessRender_desktop {
        void SetKeyboardInputSource(TextBox textBox);
    }
    public interface IHeadlessRender {
        event EventHandler<Point?> OnPointerChanged;
    }
    public interface IHeadLessRender_desktop : IHeadlessRender {
        void SetRenderSource(Control sourceControl);
        void SetPointerInputSource(Control sourceControl);
        Bitmap RenderToBitmap(double screenScaling);
    }
}
