#if CEFNET_WV || OUTSYS_WV || SUGAR_WV

using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefCommandLineArgs {
        public static Dictionary<string, string> Args { get; } = new() {
            {"no-proxy-server",null },
            {"disable-component-update",null },
            {"process-per-site",null },
#if OUTSYS_WV
            {"enable-devtools-experiments",null },
#endif
#if CEFNET_WV || SUGAR_WV
            {"ignore-certificate-errors",null },
//#if WINDOWS
		{"in-process-gpu",null },
            {"disable-gpu",null },
            {"disable-gpu-compositing",null },  
//#endif

            //{"enable-begin-frame-scheduling",null },
            //{"enable-media-stream",null },
            //{"enable-blink-features", "CSSPseudoHas"},
#if LINUX
            {"no-zygote",null },
            {"no-sandbox",null },
#endif
#endif
#if MAC
            {"use-mock-keychain",null }, 
#endif
        };
        public static string ToArgString() {
            string result =
                string.Join(
                    " ",
                    Args
                    .OrderBy(x => x.Value == null)
                    .Select(x => $"--{x.Key}{(x.Value == null ? string.Empty : $"={x.Value}")}"));
            return result;

        }
    }


}
#endif