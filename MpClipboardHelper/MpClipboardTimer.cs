using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using MonkeyPaste;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace MpClipboardHelper {
    public static partial class MpClipboardManager {
        internal class MpClipboardTimer {
            #region Private Varibles


            private static bool _isStopped = false;
            private static MpDataObject _lastCbo;
            private static Thread _workThread;

            private static IDictionary<string, object> _lastDataObject = null;
            #endregion

            #region Properties

            public static bool IgnoreClipboardChangeEvent { get; set; } = false;

            #endregion

            #region Events

            public static event EventHandler<MpDataObject> ClipboardChanged;

            #endregion

            #region Constructor

            #endregion

            #region Public Methods

            public static void Start() {
                UIPermission clipBoard = new UIPermission(PermissionState.Unrestricted);
                clipBoard.Clipboard = UIPermissionClipboard.AllClipboard;

                if (_workThread != null && _workThread.IsAlive) {                                        
                    _isStopped = false;
                } else {
                    _workThread = new Thread(new ThreadStart(CheckClipboardThread));
                    _workThread.SetApartmentState(ApartmentState.STA);
                    _workThread.IsBackground = true;
                    _workThread.Start();
                }
            }

            public void Stop() {
                _isStopped = false;
            }


            #endregion

            #region Private Methods

            #region IDataObject Wrapper Helper methods

            private static void CheckClipboardThread() {
                //setting last here will ensure item on cb isn't added when starting
                _lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                while (true) {
                    while (_isStopped || IgnoreClipboardChangeEvent) {
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(500);

                    //string test = GetOpenClipboardWindowText();
                    var cbo = ConvertManagedFormats(Clipboard.GetDataObject());
                    if (HasChanged(cbo)) {
                        _lastCbo = cbo;


                        ClipboardChanged?.Invoke(typeof(MpClipboardTimer).ToString(), cbo);
                        // NOTE word 2007 does weird stuff and alters cb after read
                        // this attempts to circumvent that by waiting a second
                        // then replacing _last with current
                        // NOTE 2 commenting this out because it itermittently
                        // creates duplicates...
                        //Thread.Sleep(1000);
                        //_lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                    }
                    Thread.Sleep(500);
                }
            }

            private static IDataObject ConvertToOleDataObject(MpDataObject mpdo) {
                DataObject dobj = new DataObject();
                foreach (var kvp in mpdo.DataFormatLookup) {
                    SetDataWrapper(ref dobj, kvp.Key, kvp.Value);
                }
                return dobj;
            }

            private static void SetDataWrapper(ref DataObject dobj, MpClipboardFormat format, string dataStr) {
                switch (format) {
                    case MpClipboardFormat.Bitmap:
                        byte[] bytes = Convert.FromBase64String(dataStr);
                        Image image;
                        using (MemoryStream ms = new MemoryStream(bytes)) {
                            image = Image.FromStream(ms);
                            dobj.SetData(DataFormats.Bitmap, image);
                        }

                        break;
                    case MpClipboardFormat.FileDrop:
                        var fl = dataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        var sc = new StringCollection();
                        sc.AddRange(fl);
                        dobj.SetFileDropList(sc);
                        break;
                    default:
                        dobj.SetData(DataFormats.FileDrop, dataStr);
                        break;
                }
            }


            private static MpDataObject ConvertManagedFormats(IDataObject ido, int retryCount = 5) {
                /*
                from: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.dataobject?view=windowsdesktop-6.0&viewFallbackFrom=net-5.0
                Special considerations may be necessary when using the metafile format with the Clipboard. 
                Due to a limitation in the current implementation of the DataObject class, the metafile format 
                used by the .NET Framework may not be recognized by applications that use an older metafile format.
                In this case, you must interoperate with the Win32 Clipboard application programming interfaces (APIs).

                An object must be serializable for it to be put on the Clipboard. 
                See System.Runtime.Serialization for more information on serialization. 
                If your target application requires a very specific data format, the headers 
                added to the data in the serialization process may prevent the application from 
                recognizing your data. To preserve your data format, add your data as a Byte array 
                to a MemoryStream and pass the MemoryStream to the SetData method.
                */

                string[] _managedDataFormats = {
                    //DataFormats.UnicodeText,
                    DataFormats.Text,
                    DataFormats.Html,
                    DataFormats.Rtf,
                    DataFormats.Bitmap,
                    DataFormats.FileDrop,
                    DataFormats.CommaSeparatedValue
                };
            var ndo = new MpDataObject();
                if (retryCount == 0) {
                    MpConsole.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                    return ndo;
                }
                try {
                    if (ido == null) {
                        ido = Clipboard.GetDataObject();
                    }
                    bool autoConvert = false;
                    if(MpDataObject.SupportedFormats.Contains(MpClipboardFormat.Text) && 
                        ido.GetDataPresent(DataFormats.Text,autoConvert)) {
                        string data = ido.GetData(DataFormats.Text, autoConvert) as string;
                        if(!string.IsNullOrEmpty(data)) {
                            ndo.DataFormatLookup.Add(MpClipboardFormat.Text, data);
                        }
                    } 
                    if (MpDataObject.SupportedFormats.Contains(MpClipboardFormat.Html) && 
                        ido.GetDataPresent(DataFormats.Html, autoConvert)) {
                        string data = ido.GetData(DataFormats.Html, autoConvert) as string;
                        if (!string.IsNullOrEmpty(data)) {
                            ndo.DataFormatLookup.Add(MpClipboardFormat.Html, data);
                        }
                    } 
                    if (MpDataObject.SupportedFormats.Contains(MpClipboardFormat.Rtf) && 
                        ido.GetDataPresent(DataFormats.Rtf, autoConvert)) {                        
                        string data = ido.GetData(DataFormats.Rtf, autoConvert) as string;
                        if (!string.IsNullOrEmpty(data)) {
                            ndo.DataFormatLookup.Add(MpClipboardFormat.Rtf, data);
                        }
                    } 
                    if (MpDataObject.SupportedFormats.Contains(MpClipboardFormat.Bitmap) && 
                        ido.GetDataPresent(DataFormats.Bitmap, autoConvert)) {
                        
                        Image img = Clipboard.GetImage();
                        if (img == null) {
                            img = MpClipboardHelper.MpClipoardImageHelpers.GetClipboardImage(ido as DataObject);
                        }
                        using (MemoryStream memoryStream = new MemoryStream()) {
                            
                            img.Save(memoryStream, ImageFormat.Bmp);
                            byte[] imageBytes = memoryStream.ToArray();
                            string data = Convert.ToBase64String(imageBytes);
                            if (!string.IsNullOrEmpty(data)) {
                                ndo.DataFormatLookup.Add(MpClipboardFormat.Bitmap, data);
                            }
                        }
                        img.Dispose();
                    } 
                    if (MpDataObject.SupportedFormats.Contains(MpClipboardFormat.FileDrop) && 
                        ido.GetDataPresent(DataFormats.FileDrop, autoConvert)) {
                        string[] sa = ido.GetData(DataFormats.FileDrop, autoConvert) as string[];
                        if(sa.Length > 0) {
                            string data = string.Join(Environment.NewLine, sa);
                            if (!string.IsNullOrEmpty(data)) {
                                ndo.DataFormatLookup.Add(MpClipboardFormat.FileDrop, data);
                            }
                        }
                    } 
                    if (MpDataObject.SupportedFormats.Contains(MpClipboardFormat.Csv) && 
                        ido.GetDataPresent(DataFormats.CommaSeparatedValue, autoConvert)) {
                        string data = ido.GetData(DataFormats.CommaSeparatedValue, autoConvert) as string;
                        if (!string.IsNullOrEmpty(data)) {
                            ndo.DataFormatLookup.Add(MpClipboardFormat.Csv, data);
                        }
                    }
                    return ndo;
                }
                catch (Exception ex) {
                    MpConsole.WriteLine($"Error accessing clipboard {retryCount} attempts remaining", ex);
                    Thread.Sleep((5 - retryCount) * 100);
                    return ConvertManagedFormats(ido, retryCount--);
                }
            }

            private static bool HasChanged(MpDataObject nco) {
                if (_lastCbo == null && nco != null) {
                    return true;
                }
                if (_lastCbo != null && nco == null) {
                    return true;
                }
                if (_lastCbo.DataFormatLookup.Count != nco.DataFormatLookup.Count) {
                    return true;
                }
                foreach (var nce in nco.DataFormatLookup) {
                    if (!_lastCbo.DataFormatLookup.ContainsKey(nce.Key)) {
                        return true;
                    }
                    if (!_lastCbo.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                        //if (nce.Key == DataFormats.Rtf) {
                        //    // NOTE when clipboard has data from MS Word (maybe other office apps too)
                        //    // it alters the rtf returned (probably makes unique for each return).
                        //    // To account for this convert each operand to plain text and compare...
                        //    string lastPt = _lastCbo[nce.Key].ToPlainText();
                        //    string curPt = nce.Value.ToPlainText();
                        //    if (!lastPt.Equals(curPt)) {
                        //        return true;
                        //    }
                        //    return false;
                        //}
                        return true;
                    }
                }
                return false;
            }

            
            #endregion

            #endregion

            #region IDispose & Destructor

            private bool _disposed;
            protected virtual void Dispose(bool disposing) {
                if (_disposed) {
                    return;
                }
                if (disposing) {
                    if (_workThread != null) {
                        _workThread.Abort();
                    }
                }
                _disposed = true;
            }

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~MpClipboardTimer() {
                Dispose(false);
            }
            #endregion
        }
    }
}
