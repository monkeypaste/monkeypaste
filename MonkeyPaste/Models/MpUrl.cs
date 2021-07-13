using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrl : MpDbModelBase, MpIClipSource {
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUrlId")]
        public override int Id { get; set; }

        [Column("MpUrlGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid UrlGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [ForeignKey(typeof(MpUrlDomain))]
        [Column("fk_MpUrlDomainId")]
        public int UrlDomainId { get; set; }
        [ManyToOne]
        public MpUrlDomain UrlDomain { get; set; }

        [Indexed]
        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        public MpUrl() {
        }
        public MpUrl(string urlPath, string urlTitle) : this() {
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }

        public static async Task<MpUrl> GetUrlByPath(string urlPath) {
            var allUrls = await MpDb.Instance.GetItems<MpUrl>();
            return allUrls.Where(x => x.UrlPath.ToLower() == urlPath.ToLower()).FirstOrDefault();
        }

        public static async Task<MpUrl> GetUrlById(int urlId) {
            var allUrls = await MpDb.Instance.GetItems<MpUrl>();
            var udbpl = allUrls.Where(x => x.Id == urlId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }


        #region MpIClipSource Implementation
        public MpIcon SourceIcon {
            get {
                if (UrlDomain == null) {
                    return null;
                }
                return UrlDomain.FavIcon;
            }
        }

        public string SourcePath => UrlPath;

        public string SourceName => UrlTitle;

        public int RootId {
            get {
                if (UrlDomain == null) {
                    return 0;
                }
                return UrlDomain.Id;
            }
        }

        public bool IsSubSource => true;

        #endregion
    }
}
