using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDeviceEndpoint : MpISyncableDbObject {
        #region Properties
        public string PublicIp4Address { get; set; }
        public int PublicConnectPortNum { get; set; }
        public int PublicSyncPortNum { get; set; }

        public string PrivateIp4Address { get; set; }
        public int PrivateConnectPortNum { get; set; }
        public int PrivateSyncPortNum { get; set; }

        public string AccessToken { get; set; }

        public DateTime ConnectDateTime { get; set; }

        public string DeviceGuid { get; set; }

        public IPEndPoint PrivateConnectIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PrivateIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PrivateIp4Address),
                    PrivateConnectPortNum);
            }
        }

        public IPEndPoint PublicConnectIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PublicIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PublicIp4Address),
                    PublicConnectPortNum);
            }
        }

        public IPEndPoint PrivateSyncIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PrivateIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PrivateIp4Address),
                    PrivateSyncPortNum);
            }
        }

        public IPEndPoint PublicSyncIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PublicIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PublicIp4Address),
                    PublicSyncPortNum);
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

        public MpDeviceEndpoint() { }
        

        public override string ToString() {
            return SerializeDbObject();
        }

        public static MpDeviceEndpoint Parse(string str) {
            return new MpDeviceEndpoint().DeserializeDbObject(str).Result as MpDeviceEndpoint;
        }

        public string SerializeDbObject(string parseToken = @"^(@!@") {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}", 
                PublicIp4Address, 
                PublicConnectPortNum, 
                PrivateIp4Address, 
                PrivateConnectPortNum, 
                AccessToken, 
                DeviceGuid, 
                ConnectDateTime,
                PublicSyncPortNum,
                PrivateSyncPortNum);
        }

        public Type GetDbObjectType() {
            return typeof(MpDeviceEndpoint);
        }

        public async Task<object> DeserializeDbObject(string objStr,string parseToken= @"^(@!@") {
            var epParts = objStr.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);
            var ep = new MpDeviceEndpoint() {
                PublicIp4Address = epParts[0],
                PublicConnectPortNum = Convert.ToInt32(epParts[1]),
                PrivateIp4Address = epParts[2],
                PrivateConnectPortNum = Convert.ToInt32(epParts[3]),
                AccessToken = epParts[4],
                DeviceGuid = epParts[5],
                ConnectDateTime = DateTime.Parse(epParts[6]),
                PublicSyncPortNum = Convert.ToInt32(epParts[7]),
                PrivateSyncPortNum = Convert.ToInt32(epParts[8])
            };
            await Task.Delay(1);
            return ep;
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            throw new NotImplementedException();
        }
        #endregion
    }
}