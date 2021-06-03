using MonkeyPaste.Messages;
using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;
using Xamarin.Forms;
using RtfPipe.Tokens;

namespace MonkeyPaste {
    public class MpSyncService : MpISyncService {
        private HttpClient httpClient;
        private HubConnection hub;
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public event EventHandler<MpNewMessageEventArgs> NewMessage;
        public bool IsConnected { get; set; }

        public async Task CreateConnection() {
            await semaphoreSlim.WaitAsync();
            if (httpClient == null) {
                httpClient = new HttpClient();
            }
            var result = await httpClient.GetStringAsync("https://mpfunction.azurewebsites.net/api/GetSignalRInfo");
            var info = JsonConvert.DeserializeObject<MpConnectionInfo>(result);

            var connectionBuilder = new HubConnectionBuilder();
            connectionBuilder.WithUrl(info.Url, (Microsoft.AspNetCore.Http.Connections.Client.HttpConnectionOptions obj) => {
                obj.AccessTokenProvider = () => Task.Run(() => info.AccessToken);
            });
            hub = connectionBuilder.Build();
            hub.On<object>("newMessage", (message) => {
                var json = message.ToString();
                var obj = JsonConvert.DeserializeObject<MpMessage>(json);
                var msg = (MpMessage)JsonConvert.DeserializeObject(json, obj.TypeInfo);
                NewMessage?.Invoke(this, new MpNewMessageEventArgs(msg));
            });
            await hub.StartAsync();

            IsConnected = true;
            semaphoreSlim.Release();
        }

        public async Task SendMessage(MpMessage message) {
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if (httpClient == null) {
                httpClient = new HttpClient();
            }

            await httpClient.PostAsync("https://mpfunction.azurewebsites.net/api/messages", content);
        }

        public async Task Dispose() {
            await semaphoreSlim.WaitAsync();
            if (hub != null) {
                await hub.StopAsync();
                await hub.DisposeAsync();
            }
            httpClient = null;
            IsConnected = false;
            semaphoreSlim.Release();
        }
    }
}
