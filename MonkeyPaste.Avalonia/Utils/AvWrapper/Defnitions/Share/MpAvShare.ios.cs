using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Graphics;
using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;
//using Xamarin.Essentials;


namespace MonkeyPaste.Avalonia {
    public partial class MpAvShare {

        async Task PlatformRequestAsync(MpAvShareTextRequest request) {
            var xam_req = new ShareTextRequest() {
                Text = request.Text,
                Title = request.Title,
                Uri = request.Uri,
                Subject = request.Subject,
                PresentationSourceBounds = ToMauiRect(request.PresentationSourceBounds)
            };
            await Share.RequestAsync(xam_req);
        }

        async Task PlatformRequestAsync(MpAvShareMultipleFilesRequest request) {
            var xam_req = new ShareMultipleFilesRequest() {
                PresentationSourceBounds = ToMauiRect(request.PresentationSourceBounds),
                Title = request.Title,
                Files = request.Files.Select(x => new ShareFile(x.FullPath, x.ContentType)).ToList()
            };
            await Share.RequestAsync(xam_req);
        }

        private Rect ToMauiRect(MpRect rect) {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
