using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpointFactory {
        public static MpDeviceEndpoint CreateEndpoint() {
            var newEndpoint = new MpDeviceEndpoint() {
                PublicIp4Address = MpSyncHelpers.GetExternalIp4Address(),
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
