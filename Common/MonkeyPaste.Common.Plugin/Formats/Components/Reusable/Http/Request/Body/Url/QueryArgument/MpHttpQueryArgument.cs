namespace MonkeyPaste.Common.Plugin {
    public class MpHttpQueryArgument : MpJsonObject {
        public string key { get; set; }
        public string value { get; set; }
        public bool isEnumId { get; set; }
        public bool omitIfNullOrEmpty { get; set; }
    }
}
