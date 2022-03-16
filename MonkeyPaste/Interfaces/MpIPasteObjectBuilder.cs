using System;
using System.Collections.Generic;
using System.Text;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public interface MpIPasteObjectBuilder {
        string GetFormat(
            MpClipboardFormat format, 
            string data, 
            string fileNameWithoutExtension = "", 
            string directory = "", 
            string textFormat = ".rtf",
            string imageFormat = ".png", 
            bool isTemporary = false);

        string GetFormat(
            MpClipboardFormat format,
            string[] data,
            string[] fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false,
            bool isCopy = false);
    }
}
