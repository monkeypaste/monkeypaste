namespace MonkeyPaste.Common.Plugin.Formats.Components.Content.Base {

    public class MpPluginInputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpPluginOutputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public bool imageToken { get; set; } = false;
        public bool textToken { get; set; } = false;
    }
    public abstract class MpPluginContentComponentBaseFormat : MpPluginComponentBaseFormat {
        public MpHttpTransactionFormat http { get; set; }
        public MpPluginInputFormat inputType { get; set; } = null;
        public MpPluginOutputFormat outputType { get; set; } = null;
    }

}
