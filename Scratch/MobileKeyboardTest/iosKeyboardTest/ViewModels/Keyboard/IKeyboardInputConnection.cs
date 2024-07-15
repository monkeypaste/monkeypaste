using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace iosKeyboardTest {
    public interface IKeyboardInputConnection
    {
        event EventHandler OnCursorChanged;
        string GetLeadingText(int n);
        void OnText(string text);
        void OnDelete();
        void OnDone();
        void OnNavigate(int dx, int dy);
        void OnVibrateRequest();
        KeyboardFlags Flags { get; }
    }
    public interface IKeyboardInputConnection_ios : IKeyboardInputConnection, IHeadlessRender {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
    public interface IKeyboardInputConnection_desktop : IKeyboardInputConnection, IHeadLessRender_desktop {
        void SetKeyboardInputSource(TextBox textBox);
    }
    public interface IHeadlessRender {
        event EventHandler<TouchEventArgs> OnPointerChanged;
    }
    public interface IHeadLessRender_desktop : IHeadlessRender {
        void SetRenderSource(Control sourceControl);
        void SetPointerInputSource(Control sourceControl);
    }
}
