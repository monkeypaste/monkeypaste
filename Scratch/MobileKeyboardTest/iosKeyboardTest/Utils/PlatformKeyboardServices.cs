namespace iosKeyboardTest {
    public interface IKeyboardPermissionHelper {
        void ShowKeyboardActivator();
    }
    public static class PlatformKeyboardServices {
        public static IKeyboardPermissionHelper KeyboardPermissionHelper { get; set; }
    }
}
