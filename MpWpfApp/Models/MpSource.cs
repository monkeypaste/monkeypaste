using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSource : MpDbModelBase {

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpSourceId")]
        public int SourceId { get; set; }

        [Column("MpSourceGuid")]
        public string Guid { get; set; }

        [Ignore]
        public Guid SourceGuid {
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

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }
        [Ignore]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }
        [Ignore]
        public MpApp App { get; set; }

        [Ignore]
        public MpICopyItemSource PrimarySource {
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
        public MpICopyItemSource SecondarySource {
            get {
                var ps = PrimarySource;
                if(ps != null) {
                    return ps.GetType() == typeof(MpApp) ? (MpICopyItemSource)Url : (MpICopyItemSource)App;
                }
                return null;
            }
        }

        public static async Task<List<MpSource>> GetAllSourcesAsync() {
            var allSources = await MpDb.Instance.GetItemsAsync<MpSource>();
            return allSources;
        }

        public static List<MpSource> GetAllSources() {
            return MpDb.Instance.GetItems<MpSource>();
        }

        public static async Task<MpSource> GetSourceById(int appId,int urlId) {
            var allSources = await GetAllSourcesAsync();
            return allSources.Where(x => x.AppId == appId && x.UrlId == urlId).FirstOrDefault();
        }

        public static MpSource Create(MpApp app, MpUrl url) {
            if(app == null) {
                throw new Exception("Source must have an app associated");
            }
            url = url == null ? new MpUrl() : url;
            MpSource dupCheck = GetAllSources().Where(x => x.AppId == app.Id && x.UrlId == url.Id).FirstOrDefault();
            if(dupCheck != null) {
                return dupCheck;
            }
            var source = new MpSource() {
                App = app,
                AppId = app.Id,
                Url = url,
                UrlId = (url == null ? 0 : url.Id)
            };
            return source;
        }

        public override void WriteToDatabase() {
            throw new NotImplementedException();
        }

        public MpSource() { }
    }
}
