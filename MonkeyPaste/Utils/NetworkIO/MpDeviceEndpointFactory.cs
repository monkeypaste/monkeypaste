using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpointFactory {
        public static MpDeviceEndpoint CreateEndpoint(MpISync sync) {
            var newEndpoint = new MpDeviceEndpoint() {
                PublicIp4Address = sync.GetExternalIp4Address(),
                PrivateIp4Addresses = new ObservableCollection<string>(sync.GetAllLocalIp4Addresses()),
                DeviceGuid = sync.GetThisClientGuid(),
                PublicPortNum = sync.GetSyncPort(),
                PrivatePortNum = sync.GetSyncPort(),
                AccessToken = MpHelpers.Instance.GetNewAccessToken()
            };
            newEndpoint.PrimaryPrivateIp4AddressIdx = newEndpoint.PrivateIp4Addresses.IndexOf(sync.GetPrimaryLocalIp4Address());
            return newEndpoint;
        }
    }
}
