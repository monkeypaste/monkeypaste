namespace MonkeyPaste.Common.Plugin {

    public class MpPluginInputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpPluginOutputFormat : MpJsonObject {
        public bool html { get; set; }
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public bool imageToken { get; set; } = false;
        public bool textToken { get; set; } = false;
    }
    public abstract class MpPluginContentComponentBaseFormat : MpPluginComponentBaseFormat {
        public MpHttpTransactionFormatBase http { get; set; }
        public MpPluginInputFormat inputType { get; set; } = null;
        public MpPluginOutputFormat outputType { get; set; } = null;
    }

}
