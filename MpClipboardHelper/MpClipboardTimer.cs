using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using MonkeyPaste.Plugin;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MpClipboardHelper {
    public class MpClipboardTimer : MpIClipboardInterop, MpIClipboardMonitor {
        #region Private Varibles

        private bool _isStopped = false;
        private MpDataObject _lastCbo;
        private Thread _workThread;

        #endregion

        #region Properties

        public uint CF_HTML, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT,CF_OEM_TEXT;

        public bool IgnoreClipboardChangeEvent { get; set; } = false;

        #endregion

        #region Events

        public event EventHandler<MpDataObject> OnClipboardChanged;

        #endregion

        #region MpIClipboardInterop Implementation

        public MpDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5) => 
            ConvertManagedFormats((IDataObject)nativeDataObj, retryCount);

        public object ConvertToNativeFormat(MpDataObject portableObj) => 
            ConvertToWinFormsDataObject(portableObj);

        public void SetDataObjectWrapper(MpDataObject portableObj) => 
            SetDataObjectWrapper(portableObj);

        #endregion

        #region MpIClipboardMonitor Implementation

        public event EventHandler<MpDataObject> OnClipboardChange;

        public void StartMonitor() => Start();

        public void StopMonitor() => Stop();

        #endregion

        #region Constructor

        #endregion

        #region Public Methods

        public void Start() {
            UIPermission clipBoard = new UIPermission(PermissionState.Unrestricted);
            clipBoard.Clipboard = UIPermissionClipboard.AllClipboard;

            CF_UNICODE_TEXT = WinApi.RegisterClipboardFormatA("UnicodeText");
            CF_BITMAP = WinApi.RegisterClipboardFormatA("Bitmap");
            CF_OEM_TEXT = WinApi.RegisterClipboardFormatA("OemText");
            CF_HTML = WinApi.RegisterClipboardFormatA("HTML Format");
            CF_RTF = WinApi.RegisterClipboardFormatA("Rich Text Format");
            CF_CSV = WinApi.RegisterClipboardFormatA(DataFormats.CommaSeparatedValue);

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

        private void CheckClipboardThread() {
            while(MpClipboardManager.ThisAppHandle == null || MpClipboardManager.ThisAppHandle == IntPtr.Zero) {
                Thread.Sleep(100);
            }
            //setting last here will ensure item on cb isn't added when starting
            _lastCbo = ConvertManagedFormats();
            while (true) {
                while (_isStopped || IgnoreClipboardChangeEvent) {
                    Thread.Sleep(100);
                }
                Thread.Sleep(500);

                var cbo = ConvertManagedFormats();
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;


                    OnClipboardChanged?.Invoke(typeof(MpClipboardTimer).ToString(), cbo);
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

        private IDataObject ConvertToWinFormsDataObject(MpDataObject mpdo) {
            DataObject dobj = new DataObject();
            foreach (var kvp in mpdo.DataFormatLookup) {
                SetDataWrapper(ref dobj, kvp.Key, kvp.Value);
            }
            return dobj;
        }

        private void SetDataWrapper(ref DataObject dobj, MpClipboardFormatType format, string dataStr) {
            string nativeTypeName = MpWinFormsDataFormatConverter.Instance.GetNativeFormatName(format);
            switch (format) {
                case MpClipboardFormatType.Bitmap:
                    byte[] bytes = Convert.FromBase64String(dataStr);
                    Image image;
                    using (MemoryStream ms = new MemoryStream(bytes)) {
                        image = Image.FromStream(ms);
                        dobj.SetData(DataFormats.Bitmap, image);
                    }
                    break;
                case MpClipboardFormatType.FileDrop:
                    var fl = dataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var sc = new StringCollection();
                    sc.AddRange(fl);
                    dobj.SetFileDropList(sc);
                    break;
                default:
                    dobj.SetData(nativeTypeName, dataStr);
                    break;
            }
        }

        private string GetClipboardData(string nativeFormatStr) {
            uint format = GetWin32FormatId(nativeFormatStr);
            if (format != 0) {
                if(WinApi.IsClipboardFormatAvailable(format)) {
                    WinApi.OpenClipboard(MpClipboardManager.ThisAppHandle);

                    //Get pointer to clipboard data in the selected format
                    IntPtr ClipboardDataPointer = WinApi.GetClipboardData(format);

                    //Do a bunch of crap necessary to copy the data from the memory
                    //the above pointer points at to a place we can access it.
                    UIntPtr Length = WinApi.GlobalSize(ClipboardDataPointer);
                    IntPtr gLock = WinApi.GlobalLock(ClipboardDataPointer);
                    if(gLock == IntPtr.Zero) {
                        return string.Empty;
                    }
                    //Init a buffer which will contain the clipboard data
                    byte[] Buffer = new byte[(int)Length];

                    //Copy clipboard data to buffer
                    Marshal.Copy(gLock, Buffer, 0, (int)Length);

                    WinApi.GlobalUnlock(gLock); //unlock gLock

                    WinApi.CloseClipboard();

                    return System.Text.Encoding.UTF8.GetString(Buffer);
                }
            }

            return null;
        }

        private uint GetWin32FormatId(string nativeFormatStr) {
            if(nativeFormatStr == DataFormats.Text) {
                return CF_TEXT;
            }
            if(nativeFormatStr == DataFormats.Bitmap) {
                return CF_BITMAP;
            }
            if(nativeFormatStr == DataFormats.CommaSeparatedValue) {
                return CF_CSV;
            }
            if(nativeFormatStr == DataFormats.FileDrop) {
                return CF_HDROP;
            }
            if(nativeFormatStr == DataFormats.Html) {
                return CF_HTML;
            }
            if(nativeFormatStr == DataFormats.Rtf) {
                return CF_RTF;
            }
            return 0;
        }

        private MpDataObject ConvertManagedFormats(IDataObject ido = null, int retryCount = 5) {
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
            if(ido != null) {
                //ido = Clipboard.GetDataObject();
                //Debugger.Break();
            }
            var ndo = new MpDataObject();
            if (retryCount == 0) {
                MpConsole.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                return ndo;
            }
            try {
                bool autoConvert = false;
                foreach (MpClipboardFormatType supportedType in MpDataObject.SupportedFormats) {
                    string nativeTypeName = MpWinFormsDataFormatConverter.Instance.GetNativeFormatName(supportedType);
                    while(IsClipboardOpen()) {
                        Thread.Sleep(100);
                    }
                    if(ido != null) {
                        if (ido.GetDataPresent(nativeTypeName, autoConvert) == false) {
                            continue;
                        }
                    } else {
                        try {
                            var curData = GetClipboardData(nativeTypeName); //Clipboard.GetData(nativeTypeName);
                            if(curData == null) {
                                continue;
                            }
                        } catch(Exception ex) {
                            MpConsole.WriteTraceLine("Clipboard timer error: " + ex);
                        }
                    }
                    string data = null;
                    switch (supportedType) {
                        case MpClipboardFormatType.Bitmap:
                            Image img = Clipboard.GetImage();
                            if (img == null) {
                                img = MpClipoardImageHelpers.GetClipboardImage(ido as DataObject);
                            }
                            using (MemoryStream memoryStream = new MemoryStream()) {
                                img.Save(memoryStream, ImageFormat.Bmp);
                                byte[] imageBytes = memoryStream.ToArray();
                                data = Convert.ToBase64String(imageBytes);
                            }
                            img.Dispose();
                            break;
                        case MpClipboardFormatType.FileDrop:
                            string[] sa = null;
                            if(ido == null) {
                                sa = Clipboard.GetData(nativeTypeName) as string[];
                            } else {
                                sa = ido.GetData(DataFormats.FileDrop, autoConvert) as string[];
                            }
                            if (sa != null && sa.Length > 0) {
                                data = string.Join(Environment.NewLine, sa);
                            }
                            break;
                        default:
                            if(ido == null) {
                                data = GetClipboardData(nativeTypeName);
                            } else {
                                data = ido.GetData(nativeTypeName, autoConvert) as string;
                            }
                            break;
                    }
                    if (!string.IsNullOrEmpty(data)) {
                        ndo.DataFormatLookup.Add(supportedType, data);
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

        private bool HasChanged(MpDataObject nco) {
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

        private bool IsClipboardOpen() {
            var hwnd = WinApi.GetOpenClipboardWindow();
            return hwnd != IntPtr.Zero;

            //if (hwnd == IntPtr.Zero) {
            //    return "Unknown";
            //}
            //Debugger.Break();
            //var int32Handle = hwnd.ToInt32();
            //var len = GetWindowTextLength(int32Handle);
            //var sb = new StringBuilder(len);
            //GetWindowText(int32Handle, sb, len);
            //return sb.ToString();
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
