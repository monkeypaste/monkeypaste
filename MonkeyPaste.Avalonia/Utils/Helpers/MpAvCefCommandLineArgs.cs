#if CEFNET_WV

using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefCommandLineArgs {
        public static Dictionary<string, string> Args { get; } = new() {
            {"no-proxy-server",null },
            {"disable-component-update",null },
            //{"process-per-site",null },
            {"enable-devtools-experiments",null },
#if CEFNET_WV
            {"in-process-gpu",null },
            {"disable-gpu",null },
            {"disable-gpu-compositing",null },

            {"ignore-certificate-errors",null },
            {"enable-begin-frame-scheduling",null },
            {"enable-media-stream",null },
            {"enable-blink-features", "CSSPseudoHas"},
#if LINUX
            {"no-zygote",null },
            {"no-sandbox",null },
#endif
#endif
#if MAC
            {"use-mock-keychain",null }, 
#endif
        };
    }


}
#endif