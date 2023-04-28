using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpManifestLedger : MpJsonObject {
        public List<MpManifestFormat> manifests { get; set; } = new List<MpManifestFormat>();
    }


}
