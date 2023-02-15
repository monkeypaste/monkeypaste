namespace MonkeyPaste.Common {
    public interface MpIIsFuzzyValueEqual<T> where T : class {
        bool IsValueEqual(T other, double thresh = 0);
    }
}
