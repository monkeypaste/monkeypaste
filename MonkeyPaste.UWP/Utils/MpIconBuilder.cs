using System.Collections.Generic;

namespace MonkeyPaste.UWP {
    public class MpIconBuilder : MpIIconBuilder {
        public string CreateBorder(string iconBase64, double scale, string hexColor) {
            return string.Empty;
        }

        public List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            return new List<string>();
        }
    }
}