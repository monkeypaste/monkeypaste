
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {
    public class MpManifestFormat :
        MpJsonObject {
        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string languageCode { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public string author { get; set; } = string.Empty;
        public string licenseUrl { get; set; } = string.Empty;
        public bool requireLicenseAcceptance { get; set; } = false;
        public string donateUrl { get; set; } = string.Empty;
        public string readmeUrl { get; set; } = string.Empty;

        public string projectUrl { get; set; }
        public string packageUrl { get; set; } = string.Empty;
        public string reportAbuseUrl { get; set; } = string.Empty;

        public string tags { get; set; } = string.Empty;


        public string guid { get; set; } = string.Empty;

        public string iconUri { get; set; } = string.Empty;

        public MpPluginPackageType packageType { get; set; }
        public List<MpPluginDependency> dependencies { get; set; }
        [JsonIgnore]
        public DateTime? datePublished { get; set; }
        #endregion

        public override string ToString() {
            return $"{title} ({version})";
        }
    }


}
