namespace MonkeyPaste.Common {
    public class MpHttpQueryArgument {
        public string key { get; set; }
        public string value { get; set; }
        public bool isEnumId { get; set; }
        public bool omitIfNullOrEmpty { get; set; }
    }
}
