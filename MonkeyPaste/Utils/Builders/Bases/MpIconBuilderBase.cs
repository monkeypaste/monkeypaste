using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpIconBuilderBase  {
        public abstract string CreateBorder(string iconBase64, double scale, string hexColor);
        public abstract List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5);
    }
}
