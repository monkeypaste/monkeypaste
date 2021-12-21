using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using System;

namespace MonkeyPaste {
    public class MpHttpHeaderItem : MpDbModelBase {
        #region Columns
        [Column("pk_MpHttpHeaderItemId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpHttpHeaderItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpHttpTransactionId")]
        [ForeignKey(typeof(MpHttpRequest))]
        public int HttpRequestId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid HttpHeaderItemGuid {
            get {
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        #endregion

        public static async Task<MpHttpHeaderItem> Create(
            string key, string value) {

            var httpHeaderItem = new MpHttpHeaderItem() {
                HttpHeaderItemGuid = System.Guid.NewGuid(),
                HttpRequestId = 0,
                Key = key,
                Value = value
            };

            await httpHeaderItem.WriteToDatabaseAsync();

            return httpHeaderItem;
        }
    }
}
