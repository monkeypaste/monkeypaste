using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpClient : MpDbObject {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpClientPlatform))]
        public int ClientPlatformId { get; set; }

        public string Ip4Address { get; set; }
        public string AccessToken { get; set; }
        public DateTime LoginDateTime { get; set; }
        public DateTime LogoutDateTime { get; set; }
        [OneToOne]
        public MpClientPlatform ClientPlatform { get; set; }

        public MpClient() : base(typeof(MpClient)) { }
    }
}
