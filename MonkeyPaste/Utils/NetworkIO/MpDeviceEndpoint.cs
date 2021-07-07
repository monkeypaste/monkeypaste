using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpoint {
        #region Properties
        public string PublicIp4Address { get; set; }
        public int PublicPortNum { get; set; }

        public string PrivateIp4Address { get; set; }
        public int PrivatePortNum { get; set; }

        public string AccessToken { get; set; }

        public DateTime LoginDateTime { get; set; }

        public string DeviceGuid { get; set; }

        public IPEndPoint PrivateIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PrivateIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PrivateIp4Address),
                    PrivatePortNum);
            }
        }

        public IPEndPoint PublicIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PublicIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PublicIp4Address),
                    PublicPortNum);
            }
        }

        public ObservableCollection<MpDeviceEndpoint> ConnectedEndpoints { get; set; } = new ObservableCollection<MpDeviceEndpoint>();

        public bool IsPrivateListening { get; set; } = false;

        public bool IsPublicListening { get; set; } = false;

        public bool IsListening {
            get {
                return IsPrivateListening || IsPublicListening;
            }
        }
        #endregion

        #region Public Methods
        public static MpDeviceEndpoint Parse(string destr) {
            if(string.IsNullOrEmpty(destr)) {
                return null;
            }
            var epParts = destr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if(epParts.Length != 7) {
                throw new Exception("EP string must have 6 elements: " + destr);
            }
            var ep = new MpDeviceEndpoint() {
                PublicIp4Address = epParts[0],
                PublicPortNum = Convert.ToInt32(epParts[1]),
                PrivateIp4Address = epParts[2],
                PrivatePortNum = Convert.ToInt32(epParts[3]),
                AccessToken = epParts[4],
                DeviceGuid = epParts[5],
                LoginDateTime = DateTime.Parse(epParts[6])
            };
            return ep;
        }
        public MpDeviceEndpoint() { }

        public override string ToString() {
            return string.Format(@"{0},{1},{2},{3},{4},{5},{6}", PublicIp4Address, PublicPortNum, PrivateIp4Address, PrivatePortNum, AccessToken, DeviceGuid,LoginDateTime);
        }
        #endregion
    }
}