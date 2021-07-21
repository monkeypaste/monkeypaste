using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace MonkeyPaste {
    public class MpRemoteDevice {
        public MpDeviceEndpoint RemoteEndpoint { get; set; }
        public Socket RemoteSocket { get; set; } 

        public MpRemoteDevice() { }
        public MpRemoteDevice(Socket s, MpDeviceEndpoint rep) {
            RemoteSocket = s;
            RemoteEndpoint = rep;
        }        
    }
}
