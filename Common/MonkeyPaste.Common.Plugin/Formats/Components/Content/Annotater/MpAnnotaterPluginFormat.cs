﻿
using MonkeyPaste.Common.Plugin.Formats.Components.Content.Base;

namespace MonkeyPaste.Common.Plugin {
    public class MpAnnotatorPluginFormat : MpPluginContentComponentBaseFormat {

    }

    public class MpAnnotatorRequestFormat : MpPluginRequestFormatBase { 
        public MpPortableDataObject data { get; set; }
    }
    public class MpAnnotatorResponseFormat : MpPluginResponseFormatBase { }
}