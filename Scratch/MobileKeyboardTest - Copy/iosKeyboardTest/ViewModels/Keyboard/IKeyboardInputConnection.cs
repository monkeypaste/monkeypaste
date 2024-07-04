namespace iosKeyboardTest
{
    public interface IKeyboardInputConnection
    {
        void OnText(string text);
        void OnDelete();
        void OnDone();
    }
    public interface iosIKeyboardInputConnection : IKeyboardInputConnection {
        bool NeedsInputModeSwitchKey { get; }
        void OnInputModeSwitched();
    }
}
