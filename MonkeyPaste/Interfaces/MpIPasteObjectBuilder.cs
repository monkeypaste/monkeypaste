using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIPasteObjectBuilder {
        string GetFormat(
            string format, 
            string data, 
            string fileNameWithoutExtension = "", 
            string directory = "", 
            string textFormat = ".rtf",
            string imageFormat = ".png", 
            bool isTemporary = false);

        string GetFormat(
            string format,
            string[] data,
            string[] fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false,
            bool isCopy = false);
    }
}
