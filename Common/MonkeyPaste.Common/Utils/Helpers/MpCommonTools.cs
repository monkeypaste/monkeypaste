using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpCommonTools {
        public static MpICommonTools Services { get; private set; }
        public static void Init(MpICommonTools services) {
            Services = services;
        }
    }
}
