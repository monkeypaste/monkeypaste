using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrl : MpDbObject {
        private static List<MpUrl> _AllUrlList = null;
        public static int TotalUrlCount = 0;

        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpUrlDomain))]
        public int UrlDomainId { get; set; }

        public string UrlPath { get; set; }
        public string UrlTitle { get; set; }

        [ManyToOne]
        public MpUrlDomain UrlDomain { get; set; }

        public MpUrl() { }
        public MpUrl(string urlPath, string urlTitle) {
            UrlPath = urlPath;
            UrlTitle = urlTitle;
            UrlDomain = MpUrlDomain.GetUrlDomainByPath(MpHelpers.Instance.GetUrlDomain(urlPath));            
        }
        public static async Task<List<MpUrl>> GetAllUrls() {
            if(_AllUrlList == null) {
                _AllUrlList = await MpDb.Instance.ExecuteAsync<MpUrl>("select * from MpUrl", null);
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
    }
}
