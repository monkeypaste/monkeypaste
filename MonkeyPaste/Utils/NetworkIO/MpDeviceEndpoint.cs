using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpoint {
        #region Properties
        public string PublicIp4Address { get; set; }
        public string PrivateIp4Address { get; set; }

        public int PublicPortNum { get; set; }
        public int PrivatePortNum { get; set; }

        public string AccessToken { get; set; }
        public DateTime LoginDateTime { get; set; }

        public IPEndPoint PublicIPEndPoint {
            get {
                return new IPEndPoint(IPAddress.Parse(PublicIp4Address), PublicPortNum);
            }
        }

        public IPEndPoint PrivateIPEndPoint {
            get {
                return new IPEndPoint(IPAddress.Parse(PrivateIp4Address), PublicPortNum);
            }
        }
        #endregion

        #region Public Methods
        public MpDeviceEndpoint() : this(string.Empty, string.Empty, -1, string.Empty,DateTime.Now) { }

        public MpDeviceEndpoint(string pubip, string priip,int port,string at,DateTime dt,int priport = -1) {
            PublicIp4Address = pubip;
            PrivateIp4Address = priip;
            PublicPortNum = port;
            PrivatePortNum = priport < 0 ? PublicPortNum : priport;
            AccessToken = at;
            LoginDateTime = dt;
        }

        public override string ToString() {
            return $"Endpoint {LoginDateTime.ToString()}   {PublicIp4Address}:{PublicPortNum} {AccessToken}";
        }
        #endregion
    }
}
