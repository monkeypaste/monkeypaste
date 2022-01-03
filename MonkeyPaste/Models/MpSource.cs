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
        public bool IsUrlPrimarySource => PrimarySource.IsUrl;

        [Ignore]
        public MpISourceItem PrimarySource {
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
        public MpISourceItem SecondarySource {
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

        public static async Task<MpSource> Create(MpApp app, MpUrl url) {
            if(app == null) {
                throw new Exception("Source must have an app associated");
            }
            int urlId = url == null ? 0 : url.Id;
            MpSource dupCheck = await MpDataModelProvider.Instance.GetSourceByMembers(app.Id, urlId);
            if(dupCheck != null) {
                dupCheck = await MpDb.Instance.GetItemAsync<MpSource>(dupCheck.Id);
                return dupCheck;
            }
            var source = new MpSource() {
                SourceGuid = System.Guid.NewGuid(),
                App = app,
                AppId = app.Id,
                Url = url,
                UrlId = urlId
            };

            await source.WriteToDatabaseAsync();
            return source;
        }
        #endregion

        public MpSource() { }
    }
}
