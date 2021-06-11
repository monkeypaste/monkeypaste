using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbImage : MpDbModelBase {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        //public byte[] ImageBytes { get; set; }

        public string ImageBase64 { get; set; }

        public static async Task<MpDbImage> GetDbImageById(int id) {
            var allicons = await MpDb.Instance.GetItems<MpDbImage>();
            return allicons.Where(x => x.Id == id).FirstOrDefault();
        }
        public MpDbImage() : base(typeof(MpDbImage)) { }
    }
}
