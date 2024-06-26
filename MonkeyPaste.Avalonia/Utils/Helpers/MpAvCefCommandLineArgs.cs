﻿#if CEFNET_WV || OUTSYS_WV || SUGAR_WV

using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefCommandLineArgs { 
        public static Dictionary<string, string> Args { get; } = new() {
            {"no-proxy-server",null },
            {"disable-component-update",null },
#if !DEBUG
		{"process-per-site",null },  
#endif
#if CEFNET_WV || SUGAR_WV || OUTSYS_WV
            {"ignore-certificate-errors",null },
            {"enable-devtools-experiments",null },
            {"use-mock-keychain",null }, 
		    {"in-process-gpu",null },
            {"disable-gpu",null },
            {"disable-gpu-compositing",null },              
#if LINUX
            {"no-zygote",null },
            {"no-sandbox",null },
#if CEFNET_WV
		    {"enable-begin-frame-scheduling",null },  
            {"enable-media-stream",null },
            {"enable-blink-features", "CSSPseudoHas"},
            
            //{"remote-debugging-port", "9222"},
#endif
#endif
#endif
#if MAC
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