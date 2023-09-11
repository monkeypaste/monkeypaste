using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Avalonia.Plugin {
    public class MpAvPluginFormat : MpPluginFormat {

        public MpClipboardHandlerFormats oleHandler { get; set; }
        public new MpAvPluginFormat backupCheckPluginFormat { get; set; }

        public override IEnumerable<MpParameterHostBaseFormat> componentFormats {
            get {

                foreach (var cf in base.componentFormats) {
                    yield return cf;
                }
                if (oleHandler != null) {
                    if (oleHandler.readers != null) {
                        foreach (var cr in oleHandler.readers) {
                            yield return cr;
                        }
                    }
                    if (oleHandler.writers != null) {
                        foreach (var cw in oleHandler.writers) {
                            yield return cw;
                        }
                    }
                }
            }
        }
    }
}
