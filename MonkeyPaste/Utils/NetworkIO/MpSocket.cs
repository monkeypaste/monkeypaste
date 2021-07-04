using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpSocket : IDisposable {
        #region Private Variables
        //private NetworkStream _stream;
        #endregion

        #region Protected Variables
        protected string _lastSend, _lastReceive;
        #endregion

        #region Properties
        public bool IsRunning { get; set; } = false;
        public string EofToken { get; set; } = "<EOF>";
        //public MpDeviceEndpoint ThisEndPoint { get; set; }
        #endregion

        #region Events
        public event EventHandler<object> OnReceive;

        public void Dispose() {
            //throw new NotImplementedException();
        }
        #endregion

        #region Public Methods
        //public abstract Task<bool> Connect(MpDeviceEndpoint otherEndPoint);
        //public abstract Task<object> Read();
        //public abstract Task<bool> Write();
        //public abstract Task<bool> Disconnect(MpDeviceEndpoint otherEndPoint);
        #endregion
    }
}
