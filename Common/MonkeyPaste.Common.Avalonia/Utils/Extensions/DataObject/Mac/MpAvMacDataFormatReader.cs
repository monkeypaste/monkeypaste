
#if MAC
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpAvMacDataFormatReader {
        public static bool TryRead(string mac_format, object mac_data, out string data_str) {
            data_str = Read(mac_format, mac_data);
            return data_str != null;
        }

        private static string Read(string mac_format, object mac_data) {
            if(mac_data is string mac_str) {
                return mac_str;
            }
            string data_str = null;
            try {
                switch (mac_format) {
                    case MpPortableDataFormats.MacImage2:
                        data_str = ReadPublicTff(mac_data);
                        break;
                    case MpPortableDataFormats.MacImage1:
                        data_str = ReadPublicPng(mac_data);
                        break;
                    case MpPortableDataFormats.MacChromeUrl1:
                    case MpPortableDataFormats.MacChromeUrl2:
                    case MpPortableDataFormats.MacText1:
                    case MpPortableDataFormats.MacText2:
                    case MpPortableDataFormats.MacText3:
                    case MpPortableDataFormats.MacUrl1:
                    case MpPortableDataFormats.MacUrl2:
                    case MpPortableDataFormats.MacHtml1:
                    case MpPortableDataFormats.MacRtf1:
                    case MpPortableDataFormats.MacRtf2:
                        data_str = ReadStrData(mac_format, mac_data);
                        break;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reading mac format '{mac_format}'", ex);
                if (ex is MpUnhandledDataTypeConversion) {
                    MpDebug.Break($"Unhandled mac data type '{mac_data.GetType()}' for format '{mac_format}'");
                }
            }
            return data_str;
        }

        private static string ReadStrData(string mac_format, object mac_data) {
            if (mac_data is not byte[] bytes) {
                throw new MpUnhandledDataTypeConversion();
            }
            using (var ms = new MemoryStream(bytes)) {
                NSStringEncoding enc = DetectEncoding(mac_format);
                if (NSData.FromStream(ms) is { } data &&
                    NSString.FromData(data, enc) is { } nsstring) {
                    return nsstring.ToString();
                }
            }
            return null;
        }
        private static string ReadPublicPng(object mac_data) {
            if (mac_data is not byte[] bytes) {
                throw new MpUnhandledDataTypeConversion();
            }
            try {
                using (var ms = new MemoryStream(bytes)) {
                    var data = NSData.FromStream(ms);
                    var bitmap = new NSBitmapImageRep(data);
                    var pngdata = bitmap.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new NSDictionary());
                    using (var stream = pngdata.AsStream()) {
                        using (var ms2 = new MemoryStream()) {
                            stream.CopyTo(ms2);
                            var pngbytes = ms2.ToArray();
                            string base64 = pngbytes.ToAvBitmap().ToBase64String();
                            return base64;
                        }
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reading public.png.", ex);
            }
            return null;
        }

        private static string ReadPublicTff(object mac_data) {
            if (mac_data is not byte[] bytes) {
                throw new MpUnhandledDataTypeConversion();
            }
            try {
                using (var ms = new MemoryStream(bytes)) {
                    var nsimg = NSImage.FromStream(ms);
                    var data = nsimg.AsTiff();
                    var bitmap = new NSBitmapImageRep(data);
                    var pngdata = bitmap.RepresentationUsingTypeProperties(NSBitmapImageFileType.Png, new NSDictionary());
                    using (var stream = pngdata.AsStream()) {
                        using (var ms2 = new MemoryStream()) {
                            stream.CopyTo(ms2);
                            var pngbytes = ms2.ToArray();
                            string base64 = pngbytes.ToAvBitmap().ToBase64String();
                            return base64;
                        }
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reading public.png.", ex);
            }
            return null;
        }

        #region Helpers

        private static NSStringEncoding DetectEncoding(string mac_format) {
            switch (mac_format) {
                default:
                    return NSStringEncoding.UTF8;
                case MpPortableDataFormats.MacText2:
                    return NSStringEncoding.UTF16LittleEndian;
                case MpPortableDataFormats.MacText3:
                    return NSStringEncoding.ASCIIStringEncoding;
            }
        }
        #endregion
    }
    public class MpUnhandledDataTypeConversion : MpInternalExceptionBase {

    }
}

#endif