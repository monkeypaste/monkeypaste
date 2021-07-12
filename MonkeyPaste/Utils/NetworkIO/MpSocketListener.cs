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
        private List<TcpClient> _clients = new List<TcpClient>();
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
            Task.Run(async () => {
                TcpListener server = null;
                try {
                    server = new TcpListener(thisEndPoint.PrivateConnectIPEndPoint);
                    server.Start();
                    IsRunning = true;
                    while (IsRunning) {
                        MpConsole.WriteLine($"Waiting for a connection ({_clients.Count} clients connected)... ");

                        var client = server.AcceptTcpClient();
                        //since this for local sync clients will always discover listener before connecting
                        //so when client count becomes even the connection right before this one was a discovery
                        //so replace this client without to avoid delegating discovery

                        //if(_clients.Count % 2 == 1) {
                        //    //the client count is about to become even so remove last client since it was a discovery connection
                        //    int discoverIdx = _clients.Count - 1;
                        //    _clients[discoverIdx].Close();
                        //    _clients[discoverIdx].Dispose();
                        //    _clients.RemoveAt(discoverIdx);
                        //} else {
                        //    MpConsole.WriteLine("Connected!");
                        //}
                        MpConsole.WriteLine("Connected!");

                        _clients.Add(client);
                        await WaitForMessage(client);
                        //OnConnect?.Invoke(this, client);
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
