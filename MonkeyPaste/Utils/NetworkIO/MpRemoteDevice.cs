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
using System.Linq;

namespace MonkeyPaste {
    public class MpRemoteDevice : IComparable {
        public MpDeviceEndpoint RemoteEndpoint { get; set; }
        public object RemoteSocket { get; set; }

        public bool IsSelected { get; set; } = false;

        public bool IsWebSocket {
            get {
                if(RemoteSocket == null) {
                    return false;
                }
                return RemoteSocket is ClientWebSocket;
            }
        }

        public CancellationTokenSource Cts { get; set; }
        public CancellationToken Ct { get; set; }

        public static MpUserDevice GetUserDeviceByGuid(string deviceGuid) {
            return MpDb.Instance.GetItems<MpUserDevice>().Where(x => x.UserDeviceGuid.ToString() == deviceGuid).FirstOrDefault();
        }

        public static MpUserDevice GetUserDeviceById(int udid) {
            return MpDb.Instance.GetItems<MpUserDevice>().Where(x => x.Id == udid).FirstOrDefault();
        }

        public MpRemoteDevice() {
            Cts = new CancellationTokenSource();
            Ct = Cts.Token;
        }

        public MpRemoteDevice(object s, MpDeviceEndpoint rep) : this() {
            RemoteSocket = s;
            RemoteEndpoint = rep;
        }

        public int CompareTo(object obj) {
            if(obj != null && obj is MpRemoteDevice ord) {
                return RemoteEndpoint.CompareTo(ord);
            }
            return -1;
        }
    }
}
