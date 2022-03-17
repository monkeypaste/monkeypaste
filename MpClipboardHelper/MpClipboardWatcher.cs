using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using MonkeyPaste.Plugin;
using System.Threading.Tasks;
using static MpClipboardHelper.WinApi;

namespace MpClipboardHelper {
    public class MpClipboardWatcher : Form, MpIClipboardMonitor, MpIClipboardInterop {
        #region Private Variables

        private readonly string[] _managedDataFormats = {
                //DataFormats.UnicodeText,
                DataFormats.Text,
                DataFormats.Html,
                DataFormats.Rtf,
                DataFormats.Bitmap,
                DataFormats.FileDrop,
                DataFormats.CommaSeparatedValue
            };


        private MpDataObject _LastDataObject = null;
        private MpDataObject _TempDataObject = null; // used when restoring clipboard

        private bool _resetClipboardAfterPaste = false;
        // needed to dispose this form
        private IntPtr _nextClipboardViewer;

        #endregion

        #region Events

        #endregion

        #region MpIClipboardMonitor Implementation

        public event EventHandler<MpDataObject> OnClipboardChanged;

        public bool IgnoreClipboardChangeEvent { get; set; } = false;

        public void StartMonitor() => Start();

        public void StopMonitor() => Stop();

        #endregion

        #region MpIClipboardInterop Implementation

        public MpDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5) {
            throw new NotImplementedException();
        }

        public object ConvertToNativeFormat(MpDataObject portableObj) => 
            ConvertToOleDataObject(portableObj);

        #endregion

        #region Public Methods

        public void Start() {
            // we can only have one instance if this class
            

            var t = new Thread(new ParameterizedThreadStart(x => Application.Run(new MpClipboardWatcher())));
            t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
            t.Start();
        }

        // stop listening (dispose form)
        public void Stop() {
            Invoke(new System.Windows.Forms.MethodInvoker(() => {
                ChangeClipboardChain(Handle, _nextClipboardViewer);
            }));
            Invoke(new System.Windows.Forms.MethodInvoker(Close));

            Dispose();

        }

        public void SetDataObjectWrapper(MpDataObject mpdo) {
            Clipboard.SetDataObject(ConvertToOleDataObject(mpdo));
        }

        public async Task PasteDataObject(MpDataObject dataObject, IntPtr handle) {
            //to prevent cb listener thread from thinking there's a new item
            IgnoreClipboardChangeEvent = true;
            try {
                if (_resetClipboardAfterPaste) {
                    _TempDataObject = _LastDataObject;
                }

                var ido = ConvertToOleDataObject(dataObject);
                Clipboard.SetDataObject(ido);
                SetForegroundWindow(handle);
                SetActiveWindow(handle);

                await Task.Delay(300);
                System.Windows.Forms.SendKeys.SendWait("^v");

                if (_resetClipboardAfterPaste) {
                    //from https://stackoverflow.com/a/52438404/105028
                    var clipboardThread = new Thread(new ThreadStart(ResetClipboard));
                    clipboardThread.SetApartmentState(ApartmentState.STA);
                    clipboardThread.Start();
                }
                IgnoreClipboardChangeEvent = false;
            }
            catch (Exception e) {
                MpConsole.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
            //Mouse.OverrideCursor = null;
        }

        #endregion

        #region Protected Methods

        protected override void SetVisibleCore(bool value) {
            // on load: (hide this window)
            CreateHandle();

            _nextClipboardViewer = SetClipboardViewer(Handle);

            base.SetVisibleCore(false);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            switch (m.Msg) {
                case WM_DRAWCLIPBOARD:
                    if (IgnoreClipboardChangeEvent) {
                        MpConsole.WriteLine("Ignoring clipboard changed event");
                        return;
                    }
                    ClipChanged();
                    SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ClipChanged() {
            IDataObject iData = Clipboard.GetDataObject();
            _LastDataObject = ConvertManagedFormats(iData);
            OnClipboardChanged?.Invoke(this, _LastDataObject);
        }

        private MpDataObject ConvertManagedFormats(object ido, int retryCount = 5) {
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
            var cbDict = new MpDataObject();
            if (retryCount == 0) {
                Console.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                return cbDict;
            }
            try {
                if (ido == null) {
                    ido = Clipboard.GetDataObject();
                }
                DataObject dobj = (DataObject)ido;
                if (dobj == null) {
                    return cbDict;
                }
                foreach (var af in _managedDataFormats) {
                    object data = null;
                    MpClipboardFormatType cf = MpClipboardFormatType.None;
                    if (dobj.GetDataPresent(af)) {
                        switch (af) {
                            case nameof(DataFormats.Text):
                                data = dobj.GetText(TextDataFormat.Text);
                                cf = MpClipboardFormatType.Text;
                                break;
                            case nameof(DataFormats.UnicodeText):
                                data = dobj.GetText(TextDataFormat.UnicodeText);
                                cf = MpClipboardFormatType.UnicodeText;
                                break;
                            case nameof(DataFormats.Rtf):
                                data = dobj.GetText(TextDataFormat.Rtf);
                                cf = MpClipboardFormatType.Rtf;
                                break;
                            case nameof(DataFormats.CommaSeparatedValue):
                                data = dobj.GetText(TextDataFormat.CommaSeparatedValue);
                                cf = MpClipboardFormatType.Csv;
                                break;
                            case nameof(DataFormats.Html):
                                data = dobj.GetText(TextDataFormat.Html);
                                cf = MpClipboardFormatType.Html;
                                break;
                            case nameof(DataFormats.Bitmap):
                                BinaryFormatter binFormatter = new BinaryFormatter();
                                using (Image img = Clipboard.GetImage()) {
                                    using (MemoryStream memoryStream = new MemoryStream()) {
                                        img.Save(memoryStream, ImageFormat.Bmp);
                                        byte[] imageBytes = memoryStream.ToArray();
                                        data = Convert.ToBase64String(imageBytes);
                                    }
                                }
                                cf = MpClipboardFormatType.Bitmap;
                                break;
                            case nameof(DataFormats.FileDrop):
                                StringCollection sc = dobj.GetFileDropList();
                                string[] sa = new string[sc.Count];
                                try {
                                    sc.CopyTo(sa, 0);
                                    data = string.Join(Environment.NewLine, sa);
                                    cf = MpClipboardFormatType.FileDrop;
                                }
                                catch { }


                                break;
                        }
                        if (data == null) {
                            data = dobj.GetData(af, true);
                        }
                        if (data != null) {
                            cbDict.DataFormatLookup.Add(cf, data.ToString());
                        }
                    }
                }
                return cbDict;
            }
            catch (Exception ex) {
                Console.WriteLine($"Error accessing clipboard {retryCount} attempts remaining");
                Thread.Sleep(500);
                retryCount--;
                return ConvertManagedFormats(ido, retryCount);
            }
        }

        private IDataObject ConvertToOleDataObject(MpDataObject mpdo) {
            DataObject dobj = new DataObject();
            foreach (var kvp in mpdo.DataFormatLookup) {
                SetDataWrapper(ref dobj, kvp.Key, kvp.Value);
            }
            return dobj;
        }

        private void SetDataWrapper(ref DataObject dobj, MpClipboardFormatType format, string dataStr) {
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
                case MpClipboardFormatType.Rtf:
                    dobj.SetData(DataFormats.Rtf, dataStr);
                    break;
                case MpClipboardFormatType.UnicodeText:
                case MpClipboardFormatType.Text:
                    dobj.SetData(DataFormats.Text, dataStr);
                    break;
                default:
                    dobj.SetData(DataFormats.FileDrop, dataStr);
                    break;
            }
        }

        private void ResetClipboard() {
            if (_TempDataObject == null) {
                return;
            }
            Clipboard.SetDataObject(ConvertToOleDataObject(_TempDataObject));
        }

        private bool IsClipboardOpen() {
            var hwnd = GetOpenClipboardWindow();
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

    }
}
