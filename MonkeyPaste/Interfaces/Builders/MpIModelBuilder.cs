using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {

    public interface MpICopyItemBuilder {
        Task<MpCopyItem> CreateAsync(MpPortableDataObject pdo, bool suppressWrite = false);
    }

    public interface MpIUrlBuilder {
        Task<MpUrl> CreateAsync(string url, string title = "");
    }


    public interface MpIProcessIconBuilder {
        MpIIconBuilder IconBuilder { get; set; }
        string GetBase64BitmapFromFolderPath(string filepath);
        string GetBase64BitmapFromFilePath(string filepath);
    }

    public interface MpIAppBuilder {
        Task<MpApp> CreateAsync(object handleOrAppPath, string appName = "");
    }

    public interface MpIIconBuilder {
        Task<MpIcon> CreateAsync(string iconBase64, bool createBorder = true);

        string CreateBorder(string iconBase64, double scale, string hexColor);
        List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);
        string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32);
    }
}
