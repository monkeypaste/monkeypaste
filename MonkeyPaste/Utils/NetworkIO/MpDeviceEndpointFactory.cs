using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDeviceEndpointFactory {
        public static MpDeviceEndpoint CreateEndpoint(MpISync sync) {
            var newEndpoint = new MpDeviceEndpoint() {
                PublicIp4Address = sync.GetExternalIp4Address(),
                PrivateIp4Address = sync.GetLocalIp4Address(),
                DeviceGuid = sync.GetThisClientGuid(),
                PublicPortNum = sync.GetSyncPort(),
                PrivatePortNum = sync.GetSyncPort(),
                AccessToken = MpHelpers.Instance.GetNewAccessToken()
            };
            return newEndpoint;
        }
    }
}
