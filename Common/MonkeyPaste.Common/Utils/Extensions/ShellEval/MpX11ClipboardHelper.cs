using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpX11ClipboardHelper {

        public static async Task<string> ReadFormatAsync(string format) {
            string xclip_cmd = $"xclip -o -target {format} -selection clipboard";
            if(MpPortableDataFormats.IsFormatStrBase64(format)) {
                xclip_cmd = $"echo -n `{xclip_cmd}` | base64";
            }
            string result = await xclip_cmd.ShellExecAsync();
            return result;
        }
    }
}