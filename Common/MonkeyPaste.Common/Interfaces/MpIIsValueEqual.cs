namespace MonkeyPaste.Common {
    public interface MpIIsValueEqual<T> where T : class {
        bool IsValueEqual(T other);
    }
}
