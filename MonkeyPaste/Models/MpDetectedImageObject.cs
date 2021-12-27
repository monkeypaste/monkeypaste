using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDetectedImageObject : MpDbModelBase {
        #region Columns

        [Column("pk_MpDetectedImageObjectId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;


        [Column("MpDetectedImageObjectGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        public double Confidence { get; set; } = 0;

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public string ObjectTypeName { get; set; } = String.Empty;

        #endregion

        #region Properties 

        [Ignore]
        public Guid DetectedImageObjectGuid {
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


        public static async Task<MpDetectedImageObject> Create(int cid, double c, double x, double y, double w, double h, string tcsv) {
            var ndio = new MpDetectedImageObject() {
                CopyItemId = cid,
                Confidence = c,
                X = x,
                Y = y,
                Width = w,
                Height = h,
                ObjectTypeName = tcsv
            };

            await ndio.WriteToDatabaseAsync();
            return ndio;
        }

        public MpDetectedImageObject() { }

        public MpDetectedImageObject(double c, double x, double y, double w, double h, string tcsv) {
            Confidence = c;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            ObjectTypeName = tcsv;
        }
        public override string ToString() {
            return string.Format(
                "Type: {0} Bounding Box: ({1},{2},{3},{4}) Confidence: {5} CopyItemId: {6}",
                ObjectTypeName, X, Y, Width, Height, Confidence, CopyItemId);
        }
    }
}
