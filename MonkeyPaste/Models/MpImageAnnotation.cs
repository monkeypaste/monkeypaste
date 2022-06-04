using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpImageAnnotation : MpDbModelBase, MpIImageRange, MpIAnnotation {
        #region Columns

        [Column("pk_MpImageAnnotationId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpImageAnnotationGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        public double Score { get; set; } = 0;

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HexColor { get; set; } = MpSystemColors.LightGray;


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

        public static async Task<MpImageAnnotation> Create(
            int cid = 0, 
            double c = 0, 
            double x = 0, 
            double y = 0, 
            double w = 0, 
            double h = 0, 
            string label = "",
            string description = "",
            string hexColor = "",
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetImageAnnotationByData(cid, x, y, w, h, c, label, description,hexColor);
            if(dupCheck != null) {
                MpConsole.WriteTraceLine($"Duplicate detected image object detected ignoring create");
                return dupCheck;
            }

            var ndio = new MpImageAnnotation() {
                DetectedImageObjectGuid = System.Guid.NewGuid(),
                CopyItemId = cid,
                Score = c,
                X = x,
                Y = y,
                Width = w,
                Height = h,
                Label = label,
                Description = description,
                HexColor = string.IsNullOrEmpty(hexColor) ? MpSystemColors.LightGray : hexColor
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            
            return ndio;
        }

        public MpImageAnnotation() { }
    }
}
