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
        protected MpISync _localSync;
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
        public static string EosToken { get; set; } = "<EOS>";
        #endregion

        #region Protected Methods
        protected string OpenMessage(string e) {
            if (!ValidateMessagee(e)) {
                throw new Exception("Message either has a bad check sum or no <Eof>");
            }
            return e.Replace(MpSocket.EofToken, string.Empty).Replace(MpSocket.EosToken,string.Empty);
        }

        protected bool ValidateMessagee(string msg) {
            if (msg == null) {
                return false;
            }
            return msg.Contains(MpSocket.EofToken) || msg.Contains(MpSocket.EosToken);
        }
        #endregion

        #region Public Methods        

        public void Write(TcpClient client, string msg) {
            
            try {
                msg += EofToken;
                var streamWriter = new StreamWriter(client.GetStream());
                streamWriter.Write(msg);
                //var msgBytes = Encoding.ASCII.GetBytes(msg);
                //client.GetStream().Write(msgBytes, 0, msgBytes.Length);
                _lastSend = msg;
                MpConsole.WriteLine("Sent: {0}" + msg);
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(@"Socket write exception: ", ex);
                OnError?.Invoke(this, ex.ToString());
            }
        }
        public void Read(TcpClient client) {
            try {
                // Get a stream object for reading and writing
                //var reader = new BinaryReader(client.GetStream());
                //_lastReceive = reader.ReadString();
                var streamReader = new StreamReader(client.GetStream());
                _lastReceive = streamReader.ReadToEnd();
                MpConsole.WriteLine($"Received: {_lastReceive}");
                OnReceive?.Invoke(this, _lastReceive);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Socket Read Exception: ", ex);
                OnError?.Invoke(this, ex.ToString());
            }
        }

        public async Task WaitForMessage(TcpClient listener) {
            //Task.Run(() => {
               // while (IsRunning) {
                    try {
                        while (listener.Available == 0) {
                            if (!listener.Connected) {
                                OnDisconnect?.Invoke(this, listener);
                                return;
                            }
                            //Thread.Sleep(100);
                            await Task.Delay(100);
                        }
                        Read(listener);
                    }
                    catch(Exception ex) {
                        MpConsole.WriteTraceLine(@"Waiting for socket message exception: ", ex);
                        OnError?.Invoke(this, ex.ToString());
                    }
               // }
            //});
        }
        #endregion

        #region Commands
        #endregion
    }
}
