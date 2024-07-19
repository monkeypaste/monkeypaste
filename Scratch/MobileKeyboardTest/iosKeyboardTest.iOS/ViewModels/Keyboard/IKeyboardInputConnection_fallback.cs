using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;

namespace iosKeyboardTest.iOS {
    [Flags]
    public enum KeyboardFeedbackFlags_fallback : long {
        None = 0,
        Vibrate = 1L << 1,
        Click = 1L << 2,
        Return = 1L << 3,
    }
    public interface IKeyboardInputConnection_fallback
    {
        event EventHandler OnCursorChanged;
        event EventHandler OnFlagsChanged;
        event EventHandler OnDismissed;
        string GetLeadingText(int offset, int len);
        void OnText(string text);
        void OnBackspace(int count);
        void OnDone();
        void OnNavigate(int dx, int dy);
        void OnFeedback(KeyboardFeedbackFlags_fallback flags);
        KeyboardFlags_fallback Flags { get; }
    }
    public interface IKeyboardInputConnection_ios_fallback : IKeyboardInputConnection_fallback {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
    public interface ITriggerTouchEvents_fallback {
        event EventHandler<TouchEventArgs_fallback> OnPointerChanged;
    }
}
