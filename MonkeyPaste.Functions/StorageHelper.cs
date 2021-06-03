using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using MonkeyPaste.Messages;

namespace MonkeyPaste.Functions {
    public static class StorageHelper {
        private static CloudBlobContainer GetContainer() {
            string storageConnectionString = Environment.GetEnvironmentVariable("StorageConnection");
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("chatimages");
            return container;
        }

        public static async Task<string> Upload(byte[] bytes, string fileEnding) {
            var container = GetContainer();
            var blob = container.GetBlockBlobReference($"{ Guid.NewGuid().ToString()}.{ fileEnding}");
            var stream = new MemoryStream(bytes);
            await blob.UploadFromStreamAsync(stream);
            return blob.Uri.AbsoluteUri;
        }

        public static async Task Clear() {
            var container = GetContainer();
            var blobList = await container.ListBlobsSegmentedAsync(string.Empty, false, BlobListingDetails.None, int.MaxValue, null, null, null);
            foreach (var blob in blobList.Results.OfType<CloudBlob>()) {
                if (blob.Properties.Created.Value.AddHours(1) < DateTime.Now) {
                    await blob.DeleteAsync();
                }
            }
        }

        [FunctionName("Messages")]
        public async static Task SendMessages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
            [SignalR(HubName = "MonkeyPaste")] IAsyncCollector<SignalRMessage> signalRMessages) {
            var jsonObject = (JObject)message;
            var msg = jsonObject.ToObject<MpMessage>();
            if (msg.TypeInfo.Name == nameof(MpPhotoMessage)) {
                //ToDo: Upload the photo to blob storage.
            }
            await signalRMessages.AddAsync(new SignalRMessage {
                Target = "newMessage",
                Arguments = new[] { message }
            });

            if (msg.TypeInfo.Name == nameof(MpPhotoMessage)) {
                var photoMessage = jsonObject.ToObject<MpPhotoMessage>();
                var bytes = Convert.FromBase64String(photoMessage.Base64Photo);
                var url = await StorageHelper.Upload(bytes, photoMessage.FileEnding);
                msg = new MpPhotoUrlMessage(photoMessage.Username) {
                    Id = photoMessage.Id,
                    Timestamp = photoMessage.Timestamp,
                    Url = url
                };
                await signalRMessages.AddAsync(new SignalRMessage {
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
                return;
            }

        }
    }
}
