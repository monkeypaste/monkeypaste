using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace iosKeyboardTest.iOS.KeyboardExt {
    [Flags]
    public enum KeyboardFeedbackFlags : long {
        None = 0,
        Vibrate = 1L << 1,
        Click = 1L << 2,
        Return = 1L << 3,
    }
    public interface IKeyboardInputConnection
    {
        event EventHandler OnCursorChanged;
        event EventHandler OnFlagsChanged;
        event EventHandler OnDismissed;
        string GetLeadingText(int offset, int len);
        void OnText(string text);
        void OnDelete();
        void OnDone();
        void OnNavigate(int dx, int dy);
        void OnFeedback(KeyboardFeedbackFlags flags);
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
