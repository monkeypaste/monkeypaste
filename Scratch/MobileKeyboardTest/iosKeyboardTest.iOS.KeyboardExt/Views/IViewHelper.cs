namespace iosKeyboardTest.iOS.KeyboardExt {
    public interface IViewHelper {
        void Layout(bool invalidate);
        void Measure(bool invalidate);
        void Paint(bool invalidate);
        void Render(bool invalidate);
    }
}