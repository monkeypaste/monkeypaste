namespace MonkeyPaste {
    public static class MpSourceRefExtensions {
        public static string ToSourceUri(this MpISourceRef sr_model, string base64Args = null) {
            return Mp.Services.SourceRefTools.ConvertToRefUrl(sr_model, base64Args);
        }
    }
}
