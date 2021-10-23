using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIIconBuilder {
        Task<string> CreateBorder(string iconBase64, double scale, string hexColor);
        Task<List<string>> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);
    }
}
