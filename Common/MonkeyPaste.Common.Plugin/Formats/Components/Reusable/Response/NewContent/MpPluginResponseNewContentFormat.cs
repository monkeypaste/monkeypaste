using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpPluginResponseNewContentFormat : MpPluginResponseItemBaseFormat {
        public string format { get; set; }
        public MpJsonPathProperty content { get; set; } = new MpJsonPathProperty(string.Empty);

        public bool isRequestChild { get; set; } = true;

        public List<MpPluginResponseAnnotationFormat> annotations { get; set; } = new List<MpPluginResponseAnnotationFormat>();
    }

}
