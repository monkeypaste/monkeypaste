using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpDeviceEndpointFactory {
        public static async Task<MpDeviceEndpoint> CreateEndpointAsync() {
            string ip = await MpNetworkHelpers.GetExternalIp4AddressAsync();
            var newEndpoint = new MpDeviceEndpoint() {
                PublicIp4Address = ip,
                PrivateIp4Addresses = new ObservableCollection<string>(MpSyncHelpers.GetAllLocalIp4Addresses()),
                DeviceGuid = MpSyncHelpers.GetThisClientGuid(),
                PublicPortNum = MpSyncHelpers.GetSyncPort(),
                PrivatePortNum = MpSyncHelpers.GetSyncPort(),
                AccessToken = MpStringExtensions.GetNewAccessToken()
            };
            newEndpoint.PrimaryPrivateIp4AddressIdx = newEndpoint.PrivateIp4Addresses.IndexOf(MpSyncHelpers.GetPrimaryLocalIp4Address());
            return newEndpoint;
        }
    }
}
