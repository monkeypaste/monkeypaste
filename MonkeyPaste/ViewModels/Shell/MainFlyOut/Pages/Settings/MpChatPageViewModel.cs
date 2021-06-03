using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using MonkeyPaste.Messages;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using static System.Net.WebRequestMethods;

namespace MonkeyPaste {
    public class MpChatPageViewModel : MpViewModelBase {
        public readonly MpISyncService ChatService;

        public ObservableCollection<MpMessage> Messages { get; private set; }
        public string Text { get; set; }

        public MpChatPageViewModel(MpISyncService chatService) {
            ChatService = chatService;
            Messages = new ObservableCollection<MpMessage>();
            ChatService.NewMessage += ChatService_NewMessage;

            Task.Run(async () => {
                if (!ChatService.IsConnected) {
                    await ChatService.CreateConnection();
                }
                await ChatService.SendMessage(new MpUserConnectedMessage(User));
            });
        }

        public ICommand Send => new Command(async () => {
            var message = new MpSimpleTextMessage(User) {
                Text = this.Text
            };
            Messages.Add(new MpLocalSimpleTextMessage(message));
            await ChatService.SendMessage(message);
            Text = string.Empty;
        });

        public ICommand Photo => new Command(async () => {
            var options = new PickMediaOptions();
            options.CompressionQuality = 50;

            var photo = await CrossMedia.Current.PickPhotoAsync();

            UserDialogs.Instance.ShowLoading("Uploading photo");

            var stream = photo.GetStream();
            var bytes = ReadFully(stream);

            var base64photo = Convert.ToBase64String(bytes);

            var message = new MpPhotoMessage(User) {
                Base64Photo = base64photo,
                FileEnding = photo.Path.Split('.').Last()
            };

            Messages.Add(message);
            await ChatService.SendMessage(message);

            UserDialogs.Instance.HideLoading();
        });

        private void ChatService_NewMessage(object sender, MpNewMessageEventArgs e) {
            Device.BeginInvokeOnMainThread(() => {
                if (!Messages.Any(x => x.Id == e.Message.Id)) {
                    Messages.Add(e.Message);
                }
            });
        }

        private byte[] ReadFully(Stream input) {
            using (MemoryStream ms = new MemoryStream()) {
                input.CopyTo(ms);                
                return ms.ToArray();
            }
        }
    }
}
