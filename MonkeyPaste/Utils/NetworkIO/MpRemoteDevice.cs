using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MonkeyPaste {
    public class MpRemoteDevice {
        public MpDeviceEndpoint RemoteEndpoint { get; set; }
        public object RemoteSocket { get; set; }

        public MpRemoteDevice() { }
        public MpRemoteDevice(object s, MpDeviceEndpoint rep) {
            RemoteSocket = s;
            RemoteEndpoint = rep;
        }
    }
}
