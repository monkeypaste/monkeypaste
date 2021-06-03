using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Net;
using Java.Security;
using Javax.Net.Ssl;
using MonkeyPaste;
using MonkeyPaste.Droid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MpWebSocketClient_Android))]
namespace MonkeyPaste {
    public class MpWebSocketClient_Android  {
        #region Singleton
        private static readonly Lazy<MpWebSocketClient_Android> _Lazy = new Lazy<MpWebSocketClient_Android>(() => new MpWebSocketClient_Android());
        public static MpWebSocketClient_Android Instance { get { return _Lazy.Value; } }

        private MpWebSocketClient_Android() {
            client = new ClientWebSocket();
            cts = new CancellationTokenSource();
            messages = new ObservableCollection<Message>();

            username = @"TestUsername";
            
        }
        #endregion
        CancellationTokenSource cts;
        public ClientWebSocket client;
        string username;
        ObservableCollection<Message> messages;
        string messageText;

        public bool IsConnected => client.State == WebSocketState.Open;

        public async void Configure() {
            
        }

        public async Task Connect() {
            string ip4Address = @"107.242.121.54";
            string port = @"44362";
            await client.ConnectAsync(new Uri($"ws://{ip4Address}:{port}"), cts.Token);
            UpdateClientState();

            await Task.Factory.StartNew(async () => {
                while (true) {
                    WebSocketReceiveResult result;
                    var message = new ArraySegment<byte>(new byte[4096]);
                    do {
                        result = await client.ReceiveAsync(message, cts.Token);
                        var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
                        string serialisedMessae = Encoding.UTF8.GetString(messageBytes);

                        try {
                            var msg = JsonConvert.DeserializeObject<Message>(serialisedMessae);
                            messages.Add(msg);
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"Invalide message format. {ex.Message}");
                        }

                    } while (!result.EndOfMessage);
                }
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            void UpdateClientState() {
                Console.WriteLine($"Websocket state {client.State}");
            }
        }

        public void Connection_OnMessage(string obj) {
            throw new NotImplementedException();
        }

        public void Connection_OnOpened() {
            throw new NotImplementedException();
        }

        public void Send() {
            throw new NotImplementedException();
        }
        async void SendMessageAsync(string message) {
            var msg = new Message {
                Name = username,
                MessagDateTime = DateTime.Now,
                Text = message,
                UserId = "DefaultUserId"
            };

            string serialisedMessage = JsonConvert.SerializeObject(msg);

            var byteMessage = Encoding.UTF8.GetBytes(serialisedMessage);
            var segmnet = new ArraySegment<byte>(byteMessage);

            await client.SendAsync(segmnet, WebSocketMessageType.Text, true, cts.Token);
            //MessageText = string.Empty;
        }

        bool CanSendMessage(string message) {
            return IsConnected && !string.IsNullOrEmpty(message);
        }

        public ICommand SendMessageommand => new Command<string>(SendMessageAsync,CanSendMessage);
    }

    public class Message {
        public string Text { get; set; }
        public DateTime MessagDateTime { get; set; }

        //public bool IsIncoming => UserId != CrossDeviceInfo.Current.Id;

        public string Name { get; set; }
        public string UserId { get; set; }
    }
}