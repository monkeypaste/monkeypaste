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

        public bool IsPublic { get; set; } = false;

        public string Ip4Address {
            get {
                return IsPublic ? PublicIp4Address : PrivateIp4Address;
            }
        }

        public int PortNum {
            get {
                return IsPublic ? PublicPortNum : PrivatePortNum;
            }
        }

        public string DeviceGuid { get; set; }

        public IPEndPoint IPEndPoint {
            get {
                return new IPEndPoint(IPAddress.Parse(Ip4Address), PortNum);
            }
        }
        #endregion

        #region Public Methods
        public MpDeviceEndpoint() { }

        public MpDeviceEndpoint(MpISync sync, int port, string at,DateTime dt, int priport = -1) {
            PublicIp4Address = sync.GetExternalIp4Address();
            PrivateIp4Address = sync.GetLocalIp4Address();
            PublicPortNum = port;
            PrivatePortNum = priport < 0 ? PublicPortNum : priport;
            AccessToken = at;
            LoginDateTime = dt;
        }

        public override string ToString() {
            return $"Endpoint {LoginDateTime.ToString()}   Public: {PublicIp4Address}:{PublicPortNum} Private: {PrivateIp4Address}:{PrivatePortNum} Active:{(IsPublic ? "Public":"Private")} {AccessToken}";
        }
        #endregion
    }
}
