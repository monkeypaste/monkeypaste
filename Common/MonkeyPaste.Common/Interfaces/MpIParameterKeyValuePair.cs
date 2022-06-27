namespace MonkeyPaste.Common {
    public interface MpIParameterKeyValuePair : MpIJsonObject {
        int paramId { get; }
        string value { get; }
    }
}
