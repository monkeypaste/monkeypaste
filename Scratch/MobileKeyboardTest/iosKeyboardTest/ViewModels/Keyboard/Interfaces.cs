using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace iosKeyboardTest {
    public interface IKeyboardInputConnection
    {
        event EventHandler OnCursorChanged;
        event EventHandler OnFlagsChanged;
        event EventHandler OnDismissed;
        string GetLeadingText(int offset, int len);
        void OnText(string text);
        void OnBackspace(int count);
        void OnDone();
        void OnNavigate(int dx, int dy);
        void OnFeedback(KeyboardFeedbackFlags flags);
        KeyboardFlags Flags { get; }
    }
    public interface IKeyboardRenderer {
        void Render();
    }
    public interface IKeyboardInputConnection_ios : IKeyboardInputConnection {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
    public interface IKeyboardInputConnection_desktop : IKeyboardInputConnection, IHeadLessRender_desktop {
        void SetKeyboardInputSource(TextBox textBox);
    }
    public interface ITriggerTouchEvents {
        event EventHandler<TouchEventArgs> OnPointerChanged;
    }
    public interface IHeadLessRender_desktop : ITriggerTouchEvents {
        void SetRenderSource(Control sourceControl);
        void SetPointerInputSource(Control sourceControl);
    }
}
