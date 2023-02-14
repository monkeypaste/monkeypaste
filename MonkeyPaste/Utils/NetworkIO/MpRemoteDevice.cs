using System;
using System.Net.WebSockets;
using System.Threading;

namespace MonkeyPaste {
    public class MpRemoteDevice : IComparable {
        public MpDeviceEndpoint RemoteEndpoint { get; set; }
        public object RemoteSocket { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsWebSocket {
            get {
                if (RemoteSocket == null) {
                    return false;
                }
                return RemoteSocket is ClientWebSocket;
            }
        }

        public CancellationTokenSource Cts { get; set; }
        public CancellationToken Ct { get; set; }


        public MpRemoteDevice() {
            Cts = new CancellationTokenSource();
            Ct = Cts.Token;
        }

        public MpRemoteDevice(object s, MpDeviceEndpoint rep) : this() {
            RemoteSocket = s;
            RemoteEndpoint = rep;
        }

        public int CompareTo(object obj) {
            if (obj != null && obj is MpRemoteDevice ord) {
                return RemoteEndpoint.CompareTo(ord);
            }
            return -1;
        }
    }
}
