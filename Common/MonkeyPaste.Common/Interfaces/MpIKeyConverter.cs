namespace MonkeyPaste.Common {
    public interface MpIKeyConverter {
        object ConvertStringToKey(string keyStr);
        string GetKeyLiteral(object key);
    }
    public interface MpIKeyConverter<T> where T : struct {
        T ConvertStringToKey(string keyStr);
        string GetKeyLiteral(T key);
    }
}