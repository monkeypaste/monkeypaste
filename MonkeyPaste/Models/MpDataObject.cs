using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
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


        public static async Task<MpDataObject> Create(
            string guid = "",
            int dataObjectId = 0,
            bool suppressWrite = false) {

            // TODO dup check here?

            var ndio = new MpDataObject() {
                DataObjectGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid(): System.Guid.Parse(guid),
                Id = dataObjectId
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        public MpDataObject() { }
    }
}
