using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpoint {
        #region Properties
        public string Ip4Address { get; set; }
        public int PortNum { get; set; }
        public string AccessToken { get; set; }
        public DateTime LoginDateTime { get; set; }

        public IPEndPoint IPEndPoint {
            get {
                return new IPEndPoint(IPAddress.Parse(Ip4Address), PortNum);
            }
        }
        #endregion

        #region Public Methods
        public MpDeviceEndpoint() : this(string.Empty, -1, string.Empty,DateTime.Now) { }

        public MpDeviceEndpoint(string ip,int port,string at,DateTime dt) {
            Ip4Address = ip;
            PortNum = port;
            AccessToken = at;
            LoginDateTime = dt;
        }

        public override string ToString() {
            return $"Endpoint {LoginDateTime.ToString()}   {Ip4Address}:{PortNum} {AccessToken}";
        }
        #endregion
    }
}
