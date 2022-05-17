using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonkeyPaste.Plugin;

namespace MpClipboardHelper {
    public class MpWinFormsDataFormatConverter : MpINativeDataFormatConverter {
        private static MpWinFormsDataFormatConverter _instance;
        public static MpWinFormsDataFormatConverter Instance => _instance ?? (_instance = new MpWinFormsDataFormatConverter());


        public string GetNativeFormatName(MpClipboardFormatType portableType, string fallbackName = "") {
            switch(portableType) {
                case MpClipboardFormatType.Text:
                    return DataFormats.Text;
                case MpClipboardFormatType.Html:
                    return DataFormats.Html;
                case MpClipboardFormatType.Rtf:
                    return DataFormats.Rtf;
                case MpClipboardFormatType.Bitmap:
                    return DataFormats.Bitmap;
                case MpClipboardFormatType.FileDrop:
                    return DataFormats.FileDrop;
                case MpClipboardFormatType.Csv:
                    return DataFormats.CommaSeparatedValue;
                case MpClipboardFormatType.InternalContent:
                    return MpPortableDataObject.InternalContentFormat;
                case MpClipboardFormatType.UnicodeText:
                    return DataFormats.UnicodeText;
                case MpClipboardFormatType.OemText:
                    return DataFormats.OemText;
                default:
                    return fallbackName;
            }
        }

    }
}
