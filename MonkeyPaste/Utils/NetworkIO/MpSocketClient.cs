using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSocketClient : MpSocket {
        #region Private Variables
        private TcpClient _listener;
        #endregion

        #region Events
        public event EventHandler<TcpClient> OnConnect;
        public event EventHandler<string> OnClientError;
        #endregion

        #region Public Methods
        public MpSocketClient() {
            OnDisconnect += MpSocketClient_OnDisconnect;
            OnError += MpSocketClient_OnError;
        }

        public void Send(string msg) {
            if(_listener == null) {
                MpConsole.WriteTraceLine(@"No listener to send to available for msg: ", msg);
                return;
            }
            Write(_listener, msg);
            WaitForMessage(_listener);
        }

        public void Connect(string ip,int port, bool isPublic) {
            try {
                if (isPublic) {

                } else {
                    _listener = new TcpClient(ip,port);
                    OnConnect?.Invoke(this, _listener);
                }
            }
            catch (Exception e) {
                Console.WriteLine("ArgumentNullException: {0}", e);
                OnClientError?.Invoke(this, e.ToString());
            }

        }

        #endregion

        #region Private Methods
        private void MpSocketClient_OnDisconnect(object sender, TcpClient listener) {
            if (listener == null) {
                return;
            }
            IsRunning = false;
            listener.Close();
            listener.Dispose();
        }

        private void MpSocketClient_OnError(object sender, string e) {            
            OnClientError?.Invoke(this, e);
        }
        #endregion
    }
}
