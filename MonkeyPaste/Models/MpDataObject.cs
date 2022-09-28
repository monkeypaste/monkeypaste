using MonkeyPaste.Common;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDataObject : MpDbModelBase {
        #region Columns

        [Column("pk_MpDataObjectId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpDataObjectGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        #endregion

        #region Properties 

        [Ignore]
        public Guid DataObjectGuid {
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

        public static async Task<MpDataObject> CreateAsync(
            string guid = "",
            int dataObjectId = 0,
            MpPortableDataObject pdo = null,
            bool suppressWrite = false) {
                        
            var ndio = new MpDataObject() {
                DataObjectGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid(): System.Guid.Parse(guid),
                Id = dataObjectId
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }

            if(pdo != null) {
                foreach(var kvp in pdo.DataFormatLookup) {
                    string itemData64Str = null;
                    if(kvp.Value is byte[] bytes) {
                        itemData64Str = bytes.ToBase64String();
                    }  else if(kvp.Value is not string) {
                        // what type is it?
                        Debugger.Break();
                        itemData64Str = kvp.Value.ToString();
                    } else {
                        itemData64Str = kvp.Value.ToString();
                    }
                    var pdoi = await MpDataObjectItem.CreateAsync(
                        dataObjectId: ndio.Id,
                        itemFormat: kvp.Key.Name,
                        itemData64: itemData64Str);
                }
            }
            return ndio;
        }

        public MpDataObject() { }
    }
}
