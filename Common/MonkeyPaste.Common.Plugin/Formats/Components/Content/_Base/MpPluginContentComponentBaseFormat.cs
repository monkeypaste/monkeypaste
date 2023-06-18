namespace MonkeyPaste.Common.Plugin {
    public class MpPluginInputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public string pattern { get; set; }
    }

    public class MpPluginOutputFormat : MpJsonObject {
        public bool html { get; set; }
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public bool imageAnnotation { get; set; } = false;
        public bool textAnnotation { get; set; } = false;

        public bool IsOutputNewContent() {
            return html || text || image || file;
        }
        public bool IsOutputAnnotation() {
            return imageAnnotation || textAnnotation;
        }
    }

    public abstract class MpPluginContentComponentBaseFormat : MpParameterHostBaseFormat {
        public MpHttpTransactionFormatBase http { get; set; }
        public MpPluginInputFormat inputType { get; set; } = null;
        public MpPluginOutputFormat outputType { get; set; } = null;
    }

}
