using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpCopyItemAnnotation : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemAnnotationId")]
        public override int Id { get; set; }

        [Column("MpCopyItemAnnotationGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        public string AnnotationJsonStr { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid CopyItemAnnotationGuid {
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

        public static async Task<MpCopyItemAnnotation> CreateAsync(
            int copyItemId = 0,
            string jsonStr = "",
            bool suppressWrite = false) {
            if (copyItemId <= 0) {
                throw new Exception("Must have copyitem fk");
            }
            var cia = new MpCopyItemAnnotation() {
                CopyItemAnnotationGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                AnnotationJsonStr = jsonStr
            };

            if (!suppressWrite) {
                await cia.WriteToDatabaseAsync();
            }
            return cia;
        }
        public MpCopyItemAnnotation() { }

    }
}
