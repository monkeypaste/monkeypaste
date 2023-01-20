using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public static class MpSourceRefExtensions {
        public static string ToSourceUri(this MpISourceRef sr_model, string base64Args = null) {
            return MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(sr_model, base64Args);
        }
    }
}
