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

        [ForeignKey(typeof(MpCopyItemTransaction))]
        [Column("fk_MpCopyItemTransactionId")]
        public int CopyItemTransactionId { get; set; }

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }


        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        #endregion

        #region Fk Objects

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpApp App { get; set; }


        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpUrl Url { get; set; }


        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpCopyItemTransaction CopyItemTransaction { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public bool IsUrlPrimarySource => PrimarySource.IsUrl;

        [Ignore]
        public MpISourceItem PrimarySource {
            get {
                if (CopyItemTransaction != null) {
                    if (CopyItemTransaction.HttpTransaction != null) {
                        return CopyItemTransaction.HttpTransaction;
                    }
                    if (CopyItemTransaction.CliTransaction != null) {
                        return CopyItemTransaction.CliTransaction;
                    }
                    if (CopyItemTransaction.DllTransaction != null) {
                        return CopyItemTransaction.DllTransaction;
                    }
                }
                if(Url != null) {
                    return Url;
                }
                return App;;
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

        #endregion

        #region Statics

        [Ignore]
        public static int ThisAppSourceId { get; set; }

        public static async Task<MpSource> Create(MpApp app, MpUrl url) {
            if(app == null) {
                throw new Exception("Source must have an app associated");
            }
            int urlId = url == null ? 0 : url.Id;
            MpSource dupCheck = await MpDataModelProvider.GetSourceByMembers(app.Id, urlId);
            if(dupCheck != null) {
                dupCheck = await MpDb.GetItemAsync<MpSource>(dupCheck.Id);
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

            if(source.UrlId == 0 && source.AppId == 0) {
                // where is this happening?
                Debugger.Break();
            }
            return source;
        }

        public static async Task<MpSource> Create(
            int copyItemTransactionId = 0, 
            bool suppressWrite = false) {
            if (copyItemTransactionId <= 0 && !suppressWrite) {
                throw new Exception("Source transaction must be populated");
            }

            var source = new MpSource() {
                SourceGuid = System.Guid.NewGuid(),
                AppId = MpPreferences.ThisAppSource.AppId,
                CopyItemTransactionId = copyItemTransactionId
            };

            if(!suppressWrite) {
                await source.WriteToDatabaseAsync();
            }
            return source;
        }
        #endregion

        public MpSource() { }
    }
}
