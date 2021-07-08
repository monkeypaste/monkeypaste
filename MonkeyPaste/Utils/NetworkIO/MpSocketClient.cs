using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSocketClient : MpSocket {
        #region Private Variables
        private TcpClient _listener;
        #endregion

        #region Properties
        public bool IsConnected => _listener != null && _listener.Connected;
        #endregion

        #region Events
        public event EventHandler<string> OnConnect;
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

            //Task.Run(async () => {
            //    await WaitForMessage(_listener);
            //});            
        }

        public string Receive() {
            if(_listener == null || !_listener.Connected || _listener.Available == 0) {
                return null;
            }
            var streamReader = new StreamReader(_listener.GetStream());
            _lastReceive = streamReader.ReadToEnd();
            return _lastReceive;
        }

        public void Connect(string ip,int port, bool isPublic) {
            try {
                if (isPublic) {

                } else {
                    SetListener(new TcpClient(ip, port));
                }
            }
            catch (Exception e) {
                Console.WriteLine("ArgumentNullException: {0}", e);
                OnClientError?.Invoke(this, e.ToString());
            }
        }

        public void SetListener(TcpClient client) {
            _listener = client;

            if (IsConnected) {
                MpConsole.WriteLine(@"Connected to listener");
                OnConnect?.Invoke(this, @"Connect"+EofToken);
            }
        }

        public async Task<string> WaitForResponse() {
            string response = null;
            do {
                response = Receive();
                await Task.Delay(100);
            }
            while (response == null);
            return response;
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
