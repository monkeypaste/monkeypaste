using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Storage;
using Windows.UI.Notifications;

namespace MonkeyPaste.Avalonia {
    public enum MpShareType {
        None = 0,
        Text,
        Uri,
        File,
        Files
    }
    public partial class MpAvShare : MpIShare {
        // from https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Share/Share.shared.cs


        #region Interfaces

        #region MpIShare Implementation
        public async Task ShareTextAsync(string title, string text, object anchor = null) {
            await RequestAsync(new MpAvShareTextRequest {
                Text = text,
                Title = title
            });
        }
        public async Task ShareUri(string title, string uri, object anchor = null) {
            await RequestAsync(new MpAvShareTextRequest {
                Uri = uri,
                Title = title
            });
        }

        #endregion

        #endregion

        public Task RequestAsync(string text) =>
            RequestAsync(new MpAvShareTextRequest(text));

        public Task RequestAsync(string text, string title) =>
            RequestAsync(new MpAvShareTextRequest(text, title));

        public Task RequestAsync(MpAvShareTextRequest request) {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Text) && string.IsNullOrEmpty(request.Uri))
                throw new ArgumentException($"Both the {nameof(request.Text)} and {nameof(request.Uri)} are invalid. Make sure to include at least one of them in the request.");

            return PlatformRequestAsync(request);
        }

        public Task RequestAsync(MpAvShareFileRequest request) {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.File == null)
                throw new ArgumentException(FileNullExeption(nameof(request.File)));

            return PlatformRequestAsync((MpAvShareMultipleFilesRequest)request);
        }

        public Task RequestAsync(MpAvShareMultipleFilesRequest request) {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!(request.Files?.Count() > 0))
                throw new ArgumentException(FileNullExeption(nameof(request.Files)));

            if (request.Files.Any(file => file == null))
                throw new ArgumentException(FileNullExeption(nameof(request.Files)));

            return PlatformRequestAsync(request);
        }

        static string FileNullExeption(string file)
            => $"The {file} parameter in the request files is invalid";
    }

    public abstract class MpAvShareRequestBase {
        public string Title { get; set; }

        public MpRect PresentationSourceBounds { get; set; } = MpRect.Empty;
    }

    public class MpAvShareTextRequest : MpAvShareRequestBase {
        public MpAvShareTextRequest() {
        }

        public MpAvShareTextRequest(string text) => Text = text;

        public MpAvShareTextRequest(string text, string title)
            : this(text) => Title = title;

        public string Subject { get; set; }

        public string Text { get; set; }

        public string Uri { get; set; }
    }

    public class MpAvShareFileRequest : MpAvShareRequestBase {
        public MpAvShareFileRequest() {
        }

        public MpAvShareFileRequest(string title, MpAvShareFile file) {
            Title = title;
            File = file;
        }

        public MpAvShareFileRequest(MpAvShareFile file)
            => File = file;

        public MpAvShareFile File { get; set; }
    }

    public class MpAvShareMultipleFilesRequest : MpAvShareRequestBase {
        public MpAvShareMultipleFilesRequest() {
        }

        public MpAvShareMultipleFilesRequest(IEnumerable<MpAvShareFile> files) =>
            Files = files.ToList();

        public MpAvShareMultipleFilesRequest(string title, IEnumerable<MpAvShareFile> files)
            : this(files) => Title = title;

        public List<MpAvShareFile> Files { get; set; }

        public static explicit operator MpAvShareMultipleFilesRequest(MpAvShareFileRequest request) {
            var requestFiles = new MpAvShareMultipleFilesRequest(request.Title, new MpAvShareFile[] { request.File });
            requestFiles.PresentationSourceBounds = request.PresentationSourceBounds;
            return requestFiles;
        }
    }

    public class MpAvShareFile {
        public string FullPath { get; private set; }
        public string ContentType { get; private set; }

        public MpAvShareFile(string fullPath) {
            FullPath = fullPath;
        }

        public MpAvShareFile(string fullPath, string contentType) : this(fullPath) {
            ContentType = contentType;
        }
    }
}
