using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbImage : MpDbModelBase {
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpDbImageId")]
        public override int Id { get; set; }

        [Column("MpDbImageGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid DbImageGuid {
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
        //public byte[] ImageBytes { get; set; }

        public string ImageBase64 { get; set; }

        public static async Task<MpDbImage> GetDbImageById(int id) {
            var allicons = await MpDb.Instance.GetItemsAsync<MpDbImage>();
            return allicons.Where(x => x.Id == id).FirstOrDefault();
        }
        public MpDbImage() {
        }
    }
}
