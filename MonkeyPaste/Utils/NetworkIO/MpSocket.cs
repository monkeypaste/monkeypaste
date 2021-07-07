using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public abstract class MpSocket {
        #region Private Variables
        #endregion

        #region Protected Variables
        protected string _lastSend, _lastReceive;
        #endregion

        #region Events
        public event EventHandler<TcpClient> OnDisconnect;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnError;
        #endregion

        #region Properties
        public bool IsRunning { get; protected set; } = false;
        public static string EofToken { get; set; } = "<EOF>";
        #endregion

        #region Public Methods
        public void Read(TcpClient client) {
            try {
                // Get a stream object for reading and writing
                var reader = new BinaryReader(client.GetStream());
                _lastReceive = reader.ReadString();
                Console.WriteLine(@"Received: {0}", _lastReceive);
                OnReceive?.Invoke(this, _lastReceive);
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(@"Socket Read Exception: ", ex);
                OnError?.Invoke(this, ex.ToString());
            }
        }

        public void Write(TcpClient client, string msg) {
            
            try {
                msg += EofToken;
                var writer = new BinaryWriter(client.GetStream());
                writer.Write(msg);
                _lastSend = msg;
                MpConsole.WriteLine("Sent: {0}" + msg);
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(@"Socket write exception: ", ex);
                OnError?.Invoke(this, ex.ToString());
            }
        }

        public void WaitForMessage(TcpClient listener) {
            Task.Run(() => {
                while (true) {
                    try {
                        while (listener.Available == 0) {
                            if (!listener.Connected) {
                                OnDisconnect?.Invoke(this, listener);
                                return;
                            }
                            Thread.Sleep(100);
                        }
                        Read(listener);
                    }
                    catch(Exception ex) {
                        MpConsole.WriteTraceLine(@"Waiting for socket message exception: ", ex);
                        OnError?.Invoke(this, ex.ToString());
                    }
                }
            });
        }
        #endregion

        #region Commands
        #endregion
    }
}
