using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs;
using MonkeyPaste.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace MonkeyPaste.Functions {
    public static class MpMessages {
        [FunctionName("Messages")]
        public async static Task SendMessages(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
             [SignalR(HubName = "MonkeyPaste")] IAsyncCollector<SignalRMessage> signalRMessages) {
            var jsonObject = (JObject)message;
            var msg = jsonObject.ToObject<MpMessage>();

            if (msg.TypeInfo.Name == nameof(MpPhotoMessage)) {
                var photoMessage = jsonObject.ToObject<MpPhotoMessage>();

                var bytes = Convert.FromBase64String(photoMessage.Base64Photo);

                var stream = new MemoryStream(bytes);
                var subscriptionKey = Environment.GetEnvironmentVariable("ComputerVisionKey");
                var computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey), new DelegatingHandler[] { });

                computerVision.Endpoint = Environment.GetEnvironmentVariable("ComputerVisionEndpoint");

                var features = new List<VisualFeatureTypes>() { VisualFeatureTypes.Adult };

                var result = await computerVision.AnalyzeImageInStreamAsync(stream, (IList<VisualFeatureTypes?>)features);

                if (result.Adult.IsAdultContent) {
                    return;
                }

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

            await signalRMessages.AddAsync(new SignalRMessage {
                Target = "newMessage",
                Arguments = new[] { message }
            });
        }
    }
}
