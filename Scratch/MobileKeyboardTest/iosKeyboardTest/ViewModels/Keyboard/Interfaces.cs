using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace iosKeyboardTest {
    public interface IMainThread {
        void Post(Action action);
    }
    public interface IAssetLoader {
        Stream LoadStream(string path); 
    }
    public interface ISharedPrefService {
        T GetPrefValue<T>(MyPrefKeys prefKey) where T : struct;
    }

    public interface ITextMeasurer {
        Rect MeasureText(string text, double scaledFontSize, TextAlignment alignment, out double ascent, out double descent);
    }
    public interface IKeyboardInputConnection
    {
        event EventHandler<TextRangeInfo> OnCursorChanged;
        event EventHandler OnFlagsChanged;
        event EventHandler OnDismissed;
        TextRangeInfo OnTextRangeInfoRequest();
        void OnText(string text);
        void OnBackspace(int count);
        void OnDone();
        void OnNavigate(int dx, int dy);
        void OnFeedback(KeyboardFeedbackFlags flags);
        void OnShowPreferences(object args);
        KeyboardFlags Flags { get; }
        ITextMeasurer TextMeasurer { get; }
        ISharedPrefService SharedPrefService { get; }
        IAssetLoader AssetLoader { get; }
        IMainThread MainThread { get; }
    }
    public interface IKeyboardViewRenderer {
        void Layout(bool invalidate);
        void Measure(bool invalidate);
        void Paint(bool invalidate);
        void Render(bool invalidate);
    }
    public interface IKeyboardRenderSource {
        void SetRenderer(IKeyboardViewRenderer renderer);
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
