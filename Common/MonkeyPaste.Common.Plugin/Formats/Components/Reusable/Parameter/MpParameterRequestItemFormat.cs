namespace MonkeyPaste.Common.Plugin {
    public class MpParameterRequestItemFormat {
        public object paramId { get; set; }
        public string value { get; set; } = string.Empty;
        public MpParameterRequestItemFormat() { }
        public MpParameterRequestItemFormat(object id, string val) {
            paramId = id;
            value = val;
        }
    }
}
