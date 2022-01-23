using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpointFactory {
        public static MpDeviceEndpoint CreateEndpoint() {
            var newEndpoint = new MpDeviceEndpoint() {
                PublicIp4Address = MpSyncHelper.GetExternalIp4Address(),
                PrivateIp4Addresses = new ObservableCollection<string>(MpSyncHelper.GetAllLocalIp4Addresses()),
                DeviceGuid = MpSyncHelper.GetThisClientGuid(),
                PublicPortNum = MpSyncHelper.GetSyncPort(),
                PrivatePortNum = MpSyncHelper.GetSyncPort(),
                AccessToken = MpStringHelpers.GetNewAccessToken()
            };
            newEndpoint.PrimaryPrivateIp4AddressIdx = newEndpoint.PrivateIp4Addresses.IndexOf(MpSyncHelper.GetPrimaryLocalIp4Address());
            return newEndpoint;
        }
    }
}
