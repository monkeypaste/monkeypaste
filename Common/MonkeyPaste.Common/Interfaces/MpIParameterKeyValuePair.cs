namespace MonkeyPaste.Common {
    public interface MpIParameterKeyValuePair : MpIJsonObject {
        string paramName { get; }
        string value { get; }
    }
}
