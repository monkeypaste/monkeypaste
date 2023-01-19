namespace MonkeyPaste.Common {
    public class MpParameterRequestItemFormat : MpJsonObject {
        public object paramId { get; set; }
        public string value { get; set; } = string.Empty;
        public MpParameterRequestItemFormat() { }
        public MpParameterRequestItemFormat(object id, string val) {
            paramId = id;
            value = val;
        }
    }
}
