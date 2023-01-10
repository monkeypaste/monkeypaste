
using System;
using System.Collections.Generic;
using System.Text;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Plugin {
    public enum MpPluginComponentType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPluginFormat : MpJsonObject {
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public string credits { get; set; } = string.Empty;
        public string licenseUrl { get; set; } = string.Empty;
        public string readmeUrl { get; set; } = string.Empty;
        public DateTime datePublished { get; set; }

        public string projectUrl { get; set; }
        public string reportAbuseUrl { get; set; }

        public string tags { get; set; }

        public List<MpPluginDependency> dependencies { get; set; }

        public string guid { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public string iconUri { get; set; } = string.Empty;

        public DateTime manifestLastModifiedDateTime { get; set; }

        public MpPluginIoTypeFormat ioType { get; set; } = new MpPluginIoTypeFormat();

        public MpActionPluginFormat action { get; set; } = null;
        public MpAnalyzerPluginFormat analyzer { get; set; } = null;

        public MpClipboardHandlerFormats clipboardHandler { get; set; }

        public MpContactFetcherFormat contactFetcher { get; set; }

        public string RootDirectory { get; set; }
        
        public object Component { get; set; } = null;

        public MpPluginFormat backupCheckPluginFormat { get; set; }
        
    }

    public class MpPluginIoTypeFormat : MpJsonObject {
        public bool isDll { get; set; } = false;
        public bool isCli { get; set; } = false;
        public bool isHttp { get; set; } = false;
    }

    public class MpPluginDependency {
        public string name { get; set; }
        public string minVersion { get; set; }
    }


}
