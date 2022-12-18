namespace MonkeyPaste {

    public interface MpIPlatformResource {
        object GetResource(string resourceKey);
        void SetResource(string resourceKey, object resourceValue);
    }
}
