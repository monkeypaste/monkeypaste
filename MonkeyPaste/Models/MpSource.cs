using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSource : MpDbModelBase {
        private static List<MpSource> _AllSources = null;

        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; } = -1;

        [ForeignKey(typeof(MpUrl))]
        public int UrlId { get; set; }
        [Ignore]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpApp))]
        public int AppId { get; set; }
        [Ignore]
        public MpApp App { get; set; }

        [Ignore]
        public MpIClipSource PrimarySource {
            get {
                if (UrlId <= 0) {
                    if (AppId <= 0) {
                        return null;
                    } else if (App != null) {
                        return App;
                    }
                } else if (Url != null) {
                    return Url;
                }
                return null;
            }
        }

        [Ignore]
        public MpIClipSource SecondarySource {
            get {
                var ps = PrimarySource;
                if(ps != null) {
                    return ps is MpApp ? Url : App;
                }
                return null;
            }
        }

        public static async Task<List<MpSource>> GetAllSources() {
            if(_AllSources == null) {
                _AllSources = await MpDb.Instance.GetItems<MpSource>();
            }
            return _AllSources;
        }
        public static async Task<MpSource> GetSourceById(int appId,int urlId) {
            var allSources = await GetAllSources();
            return allSources.Where(x => x.AppId == appId && x.UrlId == urlId).FirstOrDefault();
        }

        //public static async Task<MpSource> GetOrCreateSource(string appPath,string urlPath) {
        //    var app = await MpApp.GetAppByPath(appPath);
        //    var url = await MpUrl.GetUrlByPath(urlPath);
        //    if(url == null) {
        //        if(app == null) {

        //        }
        //    }
        //    var allSources = await GetAllSources();

        //}
        public MpSource() : base(typeof(MpSource)) { }
    }
}
