using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSource : MpDbModelBase {
        #region Columns

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpSourceId")]
        public override int Id { get; set; }

        [Column("MpSourceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

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

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public MpApp App { get; set; }

        #endregion

        [Ignore]
        public bool IsUrlSource => PrimarySource == Url;

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
                    return ps is MpApp ? Url : App;
                }
                return null;
            }
        }

        #region Statics

        [Ignore]
        public static int ThisAppSourceId { get; set; }

        public static MpSource GetThisDeviceAppSource() {
            var s = MpDb.Instance.GetItem<MpSource>(ThisAppSourceId);
            if(s == null) {
                var process = Process.GetCurrentProcess();
                string appPath = process.MainModule.FileName;
                string appName = MpPreferences.Instance.ApplicationName;
                var iconStr = new MpImageConverter().Convert(MpHelpers.Instance.LoadBitmapResource(@"MonkeyPaste.Resources.Icons.monkey.png"), typeof(string)) as string;
                var icon = MpIcon.Create(iconStr);
                var app = MpApp.Create(appPath, appName, icon);
                s = MpSource.Create(app, null);
                ThisAppSourceId = s.Id;
            }
            return s;
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

        public static async Task<MpSource> GetSourceByGuid(string guid) {
            var allSources = await GetAllSourcesAsync();
            return allSources.Where(x => x.SourceGuid.ToString() == guid).FirstOrDefault();
        }

        public static MpSource Create(MpApp app, MpUrl url) {
            if(app == null) {
                throw new Exception("Source must have an app associated");
            }
            int urlId = url == null ? 0 : url.Id;
            MpSource dupCheck = GetAllSources().Where(x => x.AppId == app.Id && x.UrlId == urlId).FirstOrDefault();
            if(dupCheck != null) {
                return dupCheck;
            }
            var source = new MpSource() {
                SourceGuid = System.Guid.NewGuid(),
                App = app,
                AppId = app.Id,
                Url = url,
                UrlId = urlId
            };

            MpDb.Instance.AddItem<MpSource>(source);
            return source;
        }
        #endregion

        public MpSource() { }
    }
}
