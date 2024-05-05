using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIPathToPlatformIcon {

        string GetPathIconBase64(string path, MpIconSize iconSize = MpIconSize.MediumIcon32);
        string GetPathIconBase64(string path, nint handle, MpIconSize iconSize = MpIconSize.MediumIcon32);
    }

    public interface MpIIconBuilder : MpIPathToPlatformIcon {
        Task<MpIcon> CreateAsync(string iconBase64, bool allowDup = false, bool suppressWrite = false);

        List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);

        bool IsStringBase64Image(string base64Str);
    }
}
