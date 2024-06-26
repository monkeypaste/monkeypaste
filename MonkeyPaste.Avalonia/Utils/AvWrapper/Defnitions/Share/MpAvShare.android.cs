﻿using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvShare {

        async Task PlatformRequestAsync(MpAvShareTextRequest request) {
            var xam_req = new ShareTextRequest() {
                Text = request.Text,
                Title = request.Title,
                Uri = request.Uri,
                Subject = request.Subject,
                PresentationSourceBounds = request.PresentationSourceBounds.ToSysDrawRect()
            };
            await Share.RequestAsync(xam_req);
        }

        async Task PlatformRequestAsync(MpAvShareMultipleFilesRequest request) {
            var xam_req = new ShareMultipleFilesRequest() {
                PresentationSourceBounds = request.PresentationSourceBounds.ToSysDrawRect(),
                Title = request.Title,
                Files = request.Files.Select(x => new ShareFile(x.FullPath, x.ContentType)).ToList()
            };
            await Share.RequestAsync(xam_req);
        }
    }
}
