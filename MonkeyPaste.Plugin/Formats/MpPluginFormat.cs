﻿
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public enum MpPluginComponentType {
        None = 0,
        Clipboard,
        Input,
        Restful,
        Gui,
        Composite
    }

    public class MpPluginFormat  {
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public string credits { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public string iconUrl { get; set; } = string.Empty;

        public DateTime manifestLastModifiedDateTime { get; set; }

        public MpPluginIoTypeFormat ioType { get; set; } = new MpPluginIoTypeFormat();

        public MpAnalyzerPluginFormat analyzer { get; set; } = null;

        public object Component { get; set; } = null;
    }

    public class MpPluginIoTypeFormat {
        public bool isDll { get; set; } = false;
        public bool isCommandLine { get; set; } = false;
        public bool isHttp { get; set; } = false;
    }

    
}