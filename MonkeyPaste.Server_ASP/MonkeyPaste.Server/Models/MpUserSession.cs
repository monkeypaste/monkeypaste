using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Server {
    public class MpUserSession {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string AccessToken { get; set; }

        public string Ip4Address { get; set; }

        public int PortNum { get; set; } = 11000;

        public DateTime LoginDateTime { get; set; }
    }
}
