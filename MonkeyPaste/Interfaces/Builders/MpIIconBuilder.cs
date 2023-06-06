using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public interface MpIIconBuilder {
        Task<MpIcon> CreateAsync(string iconBase64, bool createBorder = true, bool allowDup = false, bool suppressWrite = false);

        string CreateBorder(string iconBase64, double scale, string hexColor);
        List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);
        string GetPathIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32);

        bool IsStringBase64Image(string base64Str);
    }
}
