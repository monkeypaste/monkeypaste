﻿using System;
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


        public string GetNativeFormatName(MpClipboardFormatType portableType) {
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
                default:
                    throw new Exception("Unknown portable format: " + portableType.ToString());
            }
        }

        public MpClipboardFormatType GetPortableFormatType(string nativeFormatName) {
            if(DataFormats.Text == nativeFormatName) {
                return MpClipboardFormatType.Text;
            }
            if (DataFormats.Html == nativeFormatName) {
                return MpClipboardFormatType.Html;
            }
            if (DataFormats.Rtf == nativeFormatName) {
                return MpClipboardFormatType.Rtf;
            }
            if (DataFormats.Bitmap == nativeFormatName) {
                return MpClipboardFormatType.Bitmap;
            }
            if (DataFormats.FileDrop == nativeFormatName) {
                return MpClipboardFormatType.FileDrop;
            }
            if (DataFormats.CommaSeparatedValue == nativeFormatName) {
                return MpClipboardFormatType.Csv;
            }
            throw new Exception("Unknown native format name: " + nativeFormatName);
        }
    }
}