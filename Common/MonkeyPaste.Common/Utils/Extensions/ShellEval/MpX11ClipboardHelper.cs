using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpX11ClipboardHelper {

        public static async Task<string> ReadFormatAsync(string format) {
            string output_suffix = string.Empty;
            if(MpPortableDataFormats.IsFormatStrBase64(format)) {
                output_suffix = " | base64";
            }
            string result = await $"echo -n `xclip -o -target {format} -selection clipboard`{output_suffix}".ShellExecAsync();
            return result;
        }
    }
}