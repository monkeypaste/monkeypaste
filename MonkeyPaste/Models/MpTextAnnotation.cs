using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;
using Xamarin.Forms;
using SQLiteNetExtensions.Extensions.TextBlob;
using System.Text;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpTextAnnotation : MpDbModelBase, MpIAnnotation {

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTextAnnotationId")]
        public override int Id { get; set; }

        [Column("MpTextAnnotationGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid TextAnnotationGuid {
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
                
        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [Column("fk_MpSourceId")]
        [ForeignKey(typeof(MpSource))]
        public int SourceId { get; set; }

        public string HexColor { get; set; }
        
        public string Label { get; set; }

        public double Score { get; set; }

        public string MatchValue { get; set; }

        public string Description { get; set; }

        #endregion

        #region Fk Models

        //[ManyToOne]
        //public MpCopyItem CopyItem { get; set; }
        #endregion

        public static async Task<MpTextAnnotation> Create(
            int copyItemId = 0,
            int sourceId = 0,
            string label = "",
            string matchValue = "",
            string description = "",
            double score = 1,
            string hexColor = "",
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetTextAnnotationByData(copyItemId, sourceId,label,matchValue,description); 
            if (dupCheck != null) {
                MpConsole.WriteLine($"Duplicate Text Annotation detected during create, ignoring");
                return dupCheck;
            }
            if(score > 1) {
                throw new Exception("Score must be normalized between 0-1");
            }
            var newTextToken = new MpTextAnnotation() {
                TextAnnotationGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                SourceId = sourceId,
                Label = label,
                MatchValue = matchValue,
                Description = description,
                Score = score,
                HexColor = string.IsNullOrEmpty(hexColor) ? MpHelpers.GetRandomColor().ToHex() : hexColor
            };

            if(!suppressWrite) {
                await newTextToken.WriteToDatabaseAsync();
            }

            return newTextToken;
        }

        public MpTextAnnotation() : base() { }
    }
}
