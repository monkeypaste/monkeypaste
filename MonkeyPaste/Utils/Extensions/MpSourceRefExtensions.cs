namespace MonkeyPaste {
    public static class MpSourceRefExtensions {
        public static string ToSourceUri(this MpISourceRef sr_model) {
            return Mp.Services.SourceRefTools.ConvertToInternalUrl(sr_model);
        }
    }
}
