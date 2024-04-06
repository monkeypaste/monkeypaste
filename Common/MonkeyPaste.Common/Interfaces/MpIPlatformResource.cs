namespace MonkeyPaste.Common {

    public interface MpIPlatformResource {
        object GetResource(string resourceKey);
        T GetResource<T>(string resourceKey);
        T GetResource<T>(MpThemeResourceKey resourceKey);

        void SetResource(string resourceKey, object resourceValue);
    }
}
