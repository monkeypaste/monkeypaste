namespace MonkeyPaste.Common {
    public interface MpIKeyConverter<T> where T : struct {
        T ConvertStringToKey(string keyStr);
        string GetKeyLiteral(T key);

        int GetKeyPriority(T key);
    }
}