using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpoint {
        #region Properties
        public string Ip4Address { get; set; }
        public int PortNum { get; set; }
        public string AccessToken { get; set; }
        #endregion

        #region Public Methods
        public MpDeviceEndpoint() : this(string.Empty, -1, string.Empty) { }

        public MpDeviceEndpoint(string ip,int port,string at) {
            Ip4Address = ip;
            PortNum = port;
            AccessToken = at;
        }

        public override string ToString() {
            return $"Endpoint {Ip4Address}:{PortNum} {AccessToken}";
        }
        #endregion
    }
}
