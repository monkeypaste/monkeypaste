
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public class MpManifestFormat : MpOmitNullJsonObject {
        #region Statics

        #endregion

        #region Interfaces

        #endregion

        #region Properties
        public string title { get; set; }
        public string description { get; set; }
        public string languageCode { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        public string licenseUrl { get; set; }
        public bool requireLicenseAcceptance { get; set; } = false;
        public string donateUrl { get; set; }
        public string readmeUrl { get; set; }

        public string projectUrl { get; set; }
        public string packageUrl { get; set; }
        public string reportAbuseUrl { get; set; }

        public string tags { get; set; }


        public string guid { get; set; }

        public string iconUri { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MpPluginPackageType packageType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual MpPluginType pluginType { get; set; } = MpPluginType.Analyzer;
        public List<MpPluginDependency> dependencies { get; set; }

        public DateTime? datePublished { get; set; }
        #endregion

        public override string ToString() {
            return $"{title} ({version})";
        }
    }


}
