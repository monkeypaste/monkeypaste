using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MonkeyPaste;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Specialized;
using System.Windows;

namespace MpWpfApp {
    public class MpWpfPasteObjectBuilder : MpIPasteObjectBuilder {
        public string GetFormat(
            MpClipboardFormat format, 
            string data, 
            string fileNameWithoutExtension = "", 
            string directory = "", 
            string textFormat = ".rtf", 
            string imageFormat = ".png", 
            bool isTemporary = false) {
            // NOTE directory only used for non- file content to give a reference for interop
                        
            switch (format) {
                case MpClipboardFormat.Text:
                case MpClipboardFormat.UnicodeText:
                    return data.ToPlainText();
                case MpClipboardFormat.Rtf:
                    return data.ToRichText();
                case MpClipboardFormat.Csv:
                    return data.ToCsv();
                case MpClipboardFormat.Html:
                    return data.ToQuillText();
                case MpClipboardFormat.Bitmap:
                    if(data.IsStringBase64()) {
                        return data;
                    }
                    return data.ToFlowDocument().ToBitmapSource().ToBase64String();
                case MpClipboardFormat.FileDrop:
                    // TODO this will be where syncing file items will occur so when file/folder does not exist
                    // this should request, receive and return the path

                    if(data.IsFileOrDirectory()) {
                        return data;
                    }

                    fileNameWithoutExtension = fileNameWithoutExtension != null ? fileNameWithoutExtension :
                                                                      Path.GetFileNameWithoutExtension(
                                                                          Path.GetTempFileName());
                    directory = directory != null ? directory :
                                            Path.GetTempPath();
                    if (!directory.IsFileOrDirectory()) {
                        try {
                            Directory.CreateDirectory(directory);
                        }
                        catch (Exception ex) {
                            string tempPath = Path.GetTempPath();
                            MpConsole.WriteLine($"Error, directory '{directory}' did not exist and cannot create. Using '{tempPath}' instead.  with exception: ");
                            MpConsole.WriteTraceLine(ex);
                            directory = tempPath;
                        }
                    }
                    string outputPath = string.Empty;
                    if (data.IsStringBase64()) {
                        outputPath = Path.Combine(directory, fileNameWithoutExtension, imageFormat);
                        return MpFileIo.WriteByteArrayToFile(outputPath, data.ToByteArray(), isTemporary);
                    }
                    try {
                        var fileListParts = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        if (fileListParts.All(x => x.IsFileOrDirectory())) {
                            return data;
                        }

                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Warning, if data '{data}' is a file list this is an error ", ex);
                    }
                    outputPath = Path.Combine(directory, fileNameWithoutExtension, textFormat);

                    return MpFileIo.WriteTextToFile(outputPath, data.ToRichText(), isTemporary);
            }

            throw new Exception($"Cannot convert '{data}' to format '{format}'");
        }

        public string GetFormat(
            MpClipboardFormat format,
            string[] datas,
            string[] fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false, 
            bool isCopy = false) {
            // NOTE directory only used for non- file content to give a reference for interop
            string outputData = string.Empty;
            for (int i = 0; i < datas.Length; i++) {
                string data = datas[i];
                string fileName = fileNameWithoutExtension == null || i >= fileNameWithoutExtension.Length ?
                                        "" : fileNameWithoutExtension[i];
                switch (format) {
                    case MpClipboardFormat.Text:
                    case MpClipboardFormat.UnicodeText:
                        outputData += data.ToPlainText();
                        break;
                    case MpClipboardFormat.Rtf:
                        outputData = outputData.ToFlowDocument().Combine(data.ToRichText()).ToRichText();
                        break;
                    case MpClipboardFormat.Csv:
                        outputData += data.ToCsv();
                        break;
                    case MpClipboardFormat.Html:
                        outputData = outputData.ToFlowDocument().Combine(data.ToRichText()).ToRichText().ToQuillText();
                        break;
                    case MpClipboardFormat.Bitmap:
                        BitmapSource curBmpSrc = null;
                        if (data.IsStringBase64()) {
                            curBmpSrc = data.ToBitmapSource();
                        } else {
                            curBmpSrc = data.ToFlowDocument().ToBitmapSource();
                        }
                        BitmapSource outputBmpSrc = outputData.ToBitmapSource();
                        Size size = new Size(Math.Max(outputBmpSrc.Width, curBmpSrc.Width), Math.Max(outputBmpSrc.Height, curBmpSrc.Height));
                        outputData = MpWpfImagingHelper.MergeImages(new List<BitmapSource> { outputBmpSrc, curBmpSrc }, size).ToBase64String();
                        break;
                    case MpClipboardFormat.FileDrop:
                        // TODO this will be where syncing file items will occur so when file/folder does not exist
                        // this should request, receive and return the path
                        string curOutputPath = string.Empty;
                        if (data.IsFileOrDirectory()) {
                            curOutputPath = data + Environment.NewLine;
                        } else {
                            fileName = fileName != null ? fileName : Path.GetFileNameWithoutExtension(
                                                                              Path.GetTempFileName());
                            directory = directory != null ? directory :
                                                    Path.GetTempPath();
                            if (!directory.IsFileOrDirectory()) {
                                try {
                                    Directory.CreateDirectory(directory);
                                }
                                catch (Exception ex) {
                                    string tempPath = Path.GetTempPath();
                                    MpConsole.WriteLine($"Error, directory '{directory}' did not exist and cannot create. Using '{tempPath}' instead.  with exception: ");
                                    MpConsole.WriteTraceLine(ex);
                                    directory = tempPath;
                                }
                            }
                            if (data.IsStringBase64()) {
                                curOutputPath = Path.Combine(directory, fileName, imageFormat);
                                curOutputPath = MpFileIo.WriteByteArrayToFile(curOutputPath, data.ToByteArray(), isTemporary);
                            }
                            try {
                                var fileListParts = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                                if (fileListParts.All(x => x.IsFileOrDirectory())) {
                                    return data;
                                }

                            }
                            catch (Exception ex) {
                                MpConsole.WriteTraceLine($"Warning, if data '{data}' is a file list this is an error ", ex);
                            }
                            curOutputPath = Path.Combine(directory, fileName, textFormat);
                            curOutputPath = MpFileIo.WriteTextToFile(curOutputPath, data.ToRichText(), isTemporary);
                        }
                        outputData += curOutputPath + Environment.NewLine;
                        break;                        
                }
            }
            return outputData;
        }
    }
}
