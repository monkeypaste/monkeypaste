using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using RtfString = System.String;
using PlainTextString = System.String;
using CsvString = System.String;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public static class MpCopyItemMerger {
        
        public static string MergeRtf(List<MpCopyItem> cil) {
            var fd = string.Empty.ToFlowDocument();
            foreach(var ci in cil) {
                switch(ci.ItemType) {
                    case MpCopyItemType.Image:
                        fd = fd.Combine(
                                ci.ItemDescription.ToFlowDocument(),null,true);
                        break;
                    default:
                        fd = fd.Combine(
                                ci.ItemData.ToFlowDocument(),null, true);
                        break;
                }
            }
            return fd.ToString();
        }

        public static string MergePlainText(List<MpCopyItem> cil) {
            return MergeRtf(cil).ToPlainText();
        }

        public static BitmapSource MergeBitmaps(List<MpCopyItem> cil) {
            var bmp = (BitmapSource)new BitmapImage();
            foreach (var ci in cil) {
                switch (ci.ItemType) {
                    case MpCopyItemType.Image:
                        bmp = MpWpfImagingHelper.CombineBitmap(
                            new List<BitmapSource> { bmp, ci.ItemData.ToBitmapSource() });
                        break;
                    default:
                        var fd = ci.ItemData.ToFlowDocument();
                        var rtfImg = fd.ToBitmapSource();

                        bmp = MpWpfImagingHelper.CombineBitmap(
                            new List<BitmapSource> { bmp, rtfImg });
                        break;
                }
            }
            return bmp;
        }

        public static string[] MergeFilePaths(List<MpCopyItem> cil) {
            var fpl = new List<string>();
            foreach(var ci in cil) {
                var fl = GetFileList(ci);
                fpl.AddRange(fl);
            }
            return fpl.ToArray();
        }

        public static List<string> GetFileList(MpCopyItem CopyItem,string baseDir = "", MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            bool isTemp = true;
            var fileList = new List<string>();
            if (CopyItem.ItemType == MpCopyItemType.FileList) {
                if (forceType == MpCopyItemType.Image) {
                    fileList.Add(MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), CopyItem.ItemData.ToBitmapSource()));
                } else if (forceType == MpCopyItemType.Text) {
                    fileList.Add(MpHelpers.WriteTextToFile(Path.GetTempFileName(), CopyItem.ItemData.ToRichText()));
                } else {
                    isTemp = false;
                    var splitArray = CopyItem.ItemData.ToPlainText().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitArray == null || splitArray.Length == 0) {
                        throw new Exception("CopyItem GetFileList error, file list should not be empty");
                    } else {
                        foreach (string p in splitArray) {
                            if (!string.IsNullOrEmpty(p.Trim())) {
                                fileList.Add(p);
                            }
                        }
                    }
                }
            } else {
                string op = Path.GetTempFileName();// MpHelpers.GetUniqueFileName((forceType == MpCopyItemType.None ? CopyItemType:forceType),Title,baseDir);
                //file extension
                switch (CopyItem.ItemType) {
                    case MpCopyItemType.Text:
                        if (forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, CopyItem.ItemData.ToBitmapSource()));
                        } else {
                            fileList.Add(MpHelpers.WriteTextToFile(op, CopyItem.ItemData.ToRichText()));
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.Text) {
                            fileList.Add(MpHelpers.WriteTextToFile(op, CopyItem.ItemData.ToPlainText()));
                        } else {
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, CopyItem.ItemData.ToBitmapSource()));
                        }
                        break;
                }
            }

            if (isTemp && string.IsNullOrEmpty(baseDir) && Application.Current.MainWindow.DataContext != null) {
                //for temporary files add to mwvm list for shutdown cleanup
                foreach (var fp in fileList) {
                    if(!fp.IsFileOrDirectory()) {
                        continue;
                    }
                    MpTempFileManager.AddTempFilePath(fp);
                }
            }
            // add temp files to 
            return fileList;
        }
    }
}
