using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    // MpCefBuildRoot myDeserializedClass = JsonConvert.DeserializeObject<MpCefBuildRoot>(myJsonResponse);

    public class MpCefBuildRoot {
        public MpCefBuildVersion linux32 { get; set; }
        public MpCefBuildVersion linux64 { get; set; }
        public MpCefBuildVersion linuxarm { get; set; }
        public MpCefBuildVersion linuxarm64 { get; set; }
        public MpCefBuildVersion macosarm64 { get; set; }
        public MpCefBuildVersion macosx64 { get; set; }
        public MpCefBuildVersion windows32 { get; set; }
        public MpCefBuildVersion windows64 { get; set; }
        public MpCefBuildVersion windowsarm64 { get; set; }
    }
    public class MpCefBuildFile {
        public DateTime last_modified { get; set; }
        public string name { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public string type { get; set; }
    }

    public class MpCefBuildVersion {
        public List<MpCefVersion> versions { get; set; }
    }



    public class MpCefVersion {
        public string cef_version { get; set; }
        public string channel { get; set; }
        public string chromium_version { get; set; }
        public List<MpCefBuildFile> files { get; set; }
    }

}
