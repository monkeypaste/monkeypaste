namespace MonkeyPaste.Common {

    public interface MpIPlatformResource {
        object GetResource(string resourceKey);
        T GetResource<T>(string resourceKey);

        void SetResource(string resourceKey, object resourceValue);
    }
}
