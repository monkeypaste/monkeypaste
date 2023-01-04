namespace MonkeyPaste.Common.Plugin {
    public class MpHttpHeaderItemFormat : MpJsonObject {
        public string key { get; set; }
        public string value { get; set; }
        public MpJsonPathProperty valuePath { get; set; }
        public string type { get; set; }
    }
}
