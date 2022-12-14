using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public interface MpIIconBuilder {
        Task<MpIcon> CreateAsync(string iconBase64, bool createBorder = true);

        string CreateBorder(string iconBase64, double scale, string hexColor);
        List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);
        string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32);
    }
}
