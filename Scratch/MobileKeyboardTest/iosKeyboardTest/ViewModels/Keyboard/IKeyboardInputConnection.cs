using Avalonia.Controls;

namespace iosKeyboardTest {
    public interface IKeyboardInputConnection
    {
        void OnText(string text);
        void OnDelete();
        void OnDone();
        void OnNavigate(int dx, int dy);
    }
    public interface IKeyboardInputConnection_ios : IKeyboardInputConnection {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
    public interface IKeyboardInputConnection_desktop : IKeyboardInputConnection {
        void SetInputSource(TextBox textBox);
    }
}
