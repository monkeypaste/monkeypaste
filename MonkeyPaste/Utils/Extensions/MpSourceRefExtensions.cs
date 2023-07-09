namespace MonkeyPaste {
    public static class MpSourceRefExtensions {
        public static string ToSourceUri(this MpISourceRef sr_model) {
            return Mp.Services.SourceRefTools.ConvertToInternalUrl(sr_model);
        }
        public static bool IsSourceEqual(this MpISourceRef a, MpISourceRef b) {
            if (a == b) {
                return true;
            }
            if (a == null || b == null) {
                return false;
            }
            return a.SourceType == b.SourceType && a.SourceObjId == b.SourceObjId;
        }
    }
}
