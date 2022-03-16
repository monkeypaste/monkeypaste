using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public enum MpBoxType {
        None = 0,
        DesignerItem
    }

    public class MpBox : MpDbModelBase {
        #region Columns

        [Column("pk_MpBoxId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;


        [Column("MpBoxGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_BoxObjectId")]
        public int BoxObjId { get; set; }


        [Column("e_MpBoxTypeId")]
        public int BoxTypeId { get; set; } = 0;

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        #endregion

        #region Properties 

        [Ignore]
        public Guid BoxGuid {
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

        [Ignore]
        public MpBoxType BoxType {
            get => (MpBoxType)BoxTypeId;
            set => BoxTypeId = (int)value;
        }

        #endregion


        public static async Task<MpBox> Create(
            MpBoxType boxType = MpBoxType.None,
            int boxObjId = 0,
            double x = 0, double y = 0, double w = 0, double h = 0,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetBoxByTypeAndObjId(boxType, boxObjId);
            if(dupCheck != null) {
                MpConsole.WriteTraceLine($"Duplicate box attempt detected for Type:'{boxType}' Id:'{boxObjId}', ignoring creating new");
                return dupCheck;
            }

            var ndio = new MpBox() {
                BoxGuid = System.Guid.NewGuid(),
                BoxType = boxType,
                BoxObjId = boxObjId,
                X = x,
                Y = y,
                Width = w,
                Height = h
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        public MpBox() { }
    }
}
