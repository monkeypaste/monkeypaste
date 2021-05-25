using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrl : MpDbObject, MpICopyItemSource {
        private static List<MpUrl> _AllUrlList = null;
        public static int TotalUrlCount = 0;

        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpUrlDomain))]
        public int UrlDomainId { get; set; }
        [ManyToOne]
        public MpUrlDomain UrlDomain { get; set; }

        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        public MpUrl() : base(typeof(MpUrl)) { }
        public MpUrl(string urlPath, string urlTitle) : this() {
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }
        public static async Task<List<MpUrl>> GetAllUrls() {
            if(_AllUrlList == null) {
                _AllUrlList = await MpDb.Instance.QueryAsync<MpUrl>("select * from MpUrl", null);
            }
            
            return _AllUrlList;
        }

        public static MpUrl GetUrlById(int urlId) {
            if (_AllUrlList == null) {
                GetAllUrls();
            }
            var udbpl = _AllUrlList.Where(x => x.Id == urlId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }


        #region MpICopyItemSource Implementation
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
