using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSocketListener : MpSocket {
        #region Private Variables
        private ObservableCollection<TcpClient> _clients = new ObservableCollection<TcpClient>();
        #endregion

        #region Events
        public event EventHandler<TcpClient> OnConnect;
        public event EventHandler<string> OnListenerError;
        #endregion

        #region Public Methods
        public MpSocketListener() {
            OnConnect += MpSocketListener_OnConnect;
            OnDisconnect += MpSocketListener_OnDisconnect;
            OnError += MpSocketListener_OnError;
        }       

        public void StartListening(MpDeviceEndpoint thisEndPoint) {
            Task.Run(() => {
                TcpListener server = null;
                try {
                    server = new TcpListener(thisEndPoint.PrivateIPEndPoint);
                    server.Start();
                    IsRunning = true;
                    while (IsRunning) {
                        MpConsole.WriteLine("Waiting for a connection... ");

                        var client = server.AcceptTcpClient();
                        MpConsole.WriteLine("Connected!");

                        OnConnect?.Invoke(this, client);
                    }
                    server.Stop();                    
                }
                catch (Exception ex) {
                    Console.WriteLine("SocketException: {0}", ex);
                    OnListenerError?.Invoke(this, ex.ToString());
                } finally {
                    // Stop listening for new clients.
                    server.Stop();
                }
            });
        }

        public void StopListening() {
            IsRunning = false;
            foreach(var client in _clients) {                
                client.Close();
            }
            _clients.Clear();
        }

        public void Send(string msg) {
            if(_clients.Count == 0) {
                MpConsole.WriteTraceLine(@"No clients to send msg: " + msg);
                return;
            }
            Write(_clients[0], msg);
            WaitForMessage(_clients[0]);
        }
        #endregion

        #region Private Methods
        private void MpSocketListener_OnConnect(object sender, TcpClient client) {
            _clients.Add(client);
            WaitForMessage(client);
        }

        private void MpSocketListener_OnDisconnect(object sender, TcpClient client) {
            MpConsole.WriteLine(@"Disconnecting client");
            _clients.Remove(client);
            client.Close();
            client.Dispose();
        }
        private void MpSocketListener_OnError(object sender, string e) {
            OnListenerError?.Invoke(this, e);
        }
        #endregion
    }
}
