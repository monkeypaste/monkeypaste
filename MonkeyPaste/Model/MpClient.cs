using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpClient : MpDBObject {
        public int MpClientId { get; set; }
        public int MpPlatformId { get; set; }
        public string Ip4Address { get; set; }
        public string AccessToken { get; set; }
        public DateTime LoginDateTime { get; set; }
        public DateTime LogoutDateTime { get; set; }

        public override void LoadDataRow(DataRow dr) {
            throw new NotImplementedException();
        }

        public override void WriteToDatabase() {
            throw new NotImplementedException();
        }
    }
}
