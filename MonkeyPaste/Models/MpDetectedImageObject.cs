using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDetectedImageObject : MpDbModelBase {
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

        public MpDetectedImageObject() : this(0, 0, 0, 0, 0, 0, 0, string.Empty) { }

        public MpDetectedImageObject(int dioid, int cid, double c, double x, double y, double w, double h, string tcsv) {
            Id = dioid;
            CopyItemId = cid;
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
