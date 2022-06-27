using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDataObjectItem : MpDbModelBase {
        #region Columns

        [Column("pk_MpDataObjectItemId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpDataObjectItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpDataObjectId")]
        public int DataObjectId { get; set; }
        public string ItemFormat { get; set; }

        public string ItemDataBase64 { get; set; }

        #endregion

        #region Properties 

        [Ignore]
        public Guid DataObjectItemGuid {
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

        #endregion


        public static async Task<MpDataObjectItem> CreateAsync(
            int dataObjectId = 0,
            string itemFormat = "",
            string itemData64 = null,
            bool suppressWrite = false) {

            var ndio = new MpDataObjectItem() {
                DataObjectItemGuid = System.Guid.NewGuid(),
                DataObjectId = dataObjectId,
                ItemFormat = itemFormat,
                ItemDataBase64 = itemData64
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        public MpDataObjectItem() { }
    }
}
