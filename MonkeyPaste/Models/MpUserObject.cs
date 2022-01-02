using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpUserObject : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUserObjectId")]
        public override int Id { get; set; } = 0;

        [Column("MpUserObjectGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        #endregion

        #region Properties

        [Ignore]
        public Guid UserObjectGuid {
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

        public static async Task<MpUserObject> Create() {
            var uo = new MpUserObject() {
                UserObjectGuid = System.Guid.NewGuid()
            };

            await uo.WriteToDatabaseAsync();

            return uo;
        }

        public MpUserObject() { }
    }
}
