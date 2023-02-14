using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDeviceEndpoint : MpISyncableDbObject, IComparable {
        #region Private Variables
        #endregion

        #region Properties
        public const string ParseToken = @"$%#@";

        public int PrimaryPrivateIp4AddressIdx { get; set; } = -1;

        public string PublicIp4Address { get; set; }
        public int PublicPortNum { get; set; }

        public string PrimaryPrivateIp4Address {
            get {
                if (PrimaryPrivateIp4AddressIdx >= 0 && PrimaryPrivateIp4AddressIdx < PrivateIp4Addresses.Count) {
                    return PrivateIp4Addresses[PrimaryPrivateIp4AddressIdx];
                }
                return "0.0.0.0";
            }
        }
        public int PrivatePortNum { get; set; }
        public ObservableCollection<string> PrivateIp4Addresses { get; set; } = new ObservableCollection<string>();
        public string PrivateIp4AddressesCsv {
            get {
                var sb = new StringBuilder();
                foreach (var ip in PrivateIp4Addresses) {
                    sb.Append(ip + ",");
                }
                return sb.ToString();
            }
        }

        public string AccessToken { get; set; }

        public DateTime ConnectDateTime { get; set; }

        public string DeviceGuid { get; set; }

        public ObservableCollection<MpDeviceEndpoint> ConnectedEndpoints { get; set; } = new ObservableCollection<MpDeviceEndpoint>();

        public bool IsPrivateListening { get; set; } = false;

        public bool IsPublicListening { get; set; } = false;

        public bool IsListening {
            get {
                return IsPrivateListening || IsPublicListening;
            }
        }

        #region IPEndPoints
        public IPEndPoint PrivateIPEndPoint {
            get {
                if (string.IsNullOrEmpty(PrimaryPrivateIp4Address)) {
                    return null;
                }
                return new IPEndPoint(
                    IPAddress.Parse(PrimaryPrivateIp4Address),
                    PrivatePortNum);
            }
        }
        #endregion

        #endregion

        #region Public Methods

        public MpDeviceEndpoint() { }


        public bool IsLocal(MpDeviceEndpoint oep) {
            if (oep == null || string.IsNullOrEmpty(oep.PublicIp4Address)) {
                return false;
            }
            return PublicIp4Address == oep.PublicIp4Address;
        }

        public async Task<string> ToStringAsync() {
            string result = await SerializeDbObjectAsync();
            return result;
        }

        public static async Task<MpDeviceEndpoint> Parse(string str) {
            var temp = new MpDeviceEndpoint();
            var result = await temp.DeserializeDbObjectAsync(str);

            return result as MpDeviceEndpoint;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}",
                ParseToken,
                PublicIp4Address,
                PublicPortNum,
                PrivateIp4AddressesCsv,
                PrimaryPrivateIp4AddressIdx,
                PrivatePortNum,
                AccessToken,
                DeviceGuid,
                ConnectDateTime,
                IsPrivateListening ? "1" : "0",
                @"test@test.com",
                @"password");
        }

        public Type GetDbObjectType() {
            return typeof(MpDeviceEndpoint);
        }

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            var epParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var ep = new MpDeviceEndpoint() {
                PublicIp4Address = epParts[0],
                PublicPortNum = Convert.ToInt32(epParts[1]),
                PrivateIp4Addresses = new ObservableCollection<string>(epParts[2].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)),
                PrimaryPrivateIp4AddressIdx = Convert.ToInt32(epParts[3]),
                PrivatePortNum = Convert.ToInt32(epParts[4]),
                AccessToken = epParts[5],
                DeviceGuid = epParts[6],
                ConnectDateTime = DateTime.Parse(epParts[7]),
                IsPrivateListening = epParts[8] == "1"
            };
            await Task.Delay(1);
            return ep;
        }

        public Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            throw new NotImplementedException();
        }

        public Task<object> CreateFromLogsAsync(string dboGuid, List<MpDbLog> logs, string fromClientGuid) {
            throw new NotImplementedException();
        }

        public int CompareTo(object obj) {
            if (obj != null && obj is MpDeviceEndpoint ode) {
                if (ode.PrimaryPrivateIp4Address == PrimaryPrivateIp4Address && ode.PublicIp4Address == PublicIp4Address) {
                    return 0;
                }
                return ConnectDateTime.CompareTo(ode.ConnectDateTime);
            }
            return -1;
        }
        #endregion
    }
}