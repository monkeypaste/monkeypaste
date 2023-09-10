
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {
    public class MpManifestFormat :
        MpJsonObject,
        MpIFilterMatch,
        MpIFilterMatchCount {
        #region Interfaces
        #region MpIFilterMatch Implementation
        private string[] _filterFields => new string[] {
            title,
            description,
            credits,
            tags,
            projectUrl
        };
        bool MpIFilterMatch.IsFilterMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            string lc_filter = filter.ToLower();
            return
                _filterFields
                .Where(x => x != null)
                .Any(x => x.ToLower().Contains(lc_filter));
        }
        int MpIFilterMatchCount.MatchCount(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return 1;
            }
            string lc_filter = filter.ToLower();
            return
                _filterFields
                .Where(x => x != null)
                .Sum(x => x.ToLower().IndexListOfAll(lc_filter).Count);
        }
        #endregion

        #endregion

        #region Properties
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string language { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
        public string credits { get; set; } = string.Empty;
        public string licenseUrl { get; set; } = string.Empty;
        public string donateUrl { get; set; } = string.Empty;
        public string readmeUrl { get; set; } = string.Empty;

        public string projectUrl { get; set; }
        public string packageUrl { get; set; } = string.Empty;
        public string reportAbuseUrl { get; set; } = string.Empty;

        public string tags { get; set; } = string.Empty;


        public string guid { get; set; } = string.Empty;
        public string iconUri { get; set; } = string.Empty;

        public MpPluginBundleType bundleType { get; set; }
        public List<MpPluginDependency> dependencies { get; set; }
        [JsonIgnore]
        public DateTime? datePublished { get; set; }
        #endregion
    }


}
