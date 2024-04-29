using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public enum MpLinuxSelectionType {
        None = 0,
        Primary,
        Secondary,
        Clipboard
    }
    public static class MpX11ClipboardHelper {

        public static async Task<string> ReadFormatAsync(string format) {
            string xclip_cmd = $"xclip -o -target {format}";// -selection clipboard";
            if(MpDataFormatRegistrar.IsFormatStrBase64(format)) {
                xclip_cmd = $"{xclip_cmd} | base64 --wrap=0";
            }
            string result = await xclip_cmd.ShellExecAsync();
            if(format == MpPortableDataFormats.MimeMozUrl &&
                result != null) {
                // I think this is utf16 or something but every char is padded w/ \0 so get em gone
                result = result.Replace("\0", string.Empty);
            }
            return result;
        }

        public static async Task<string[]> GetFormatsAsync(MpLinuxSelectionType selType) {
            string xclip_cmd = $"xclip -o -target TARGETS";// -selection {selType.ToString().ToLowerInvariant()}";
            string result = await xclip_cmd.ShellExecAsync();
            return result.SplitByLineBreak();
        }

        

    }
}