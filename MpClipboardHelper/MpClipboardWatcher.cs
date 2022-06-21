using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Collections.Generic;
using System.Linq;

namespace MpClipboardHelper {
    public class MpClipboardWatcher : Form, 
        MpIClipboardMonitor, 
        MpIPlatformDataObjectRegistrar {
        #region Private Variables
        private IEnumerable<MpIClipboardPluginComponent> _clipboardHandlers =>
            MpPluginManager.Plugins.Where(x => x.Value.Component is MpIClipboardPluginComponent)
                                   .Select(x => x.Value.Component)
                                   .Cast<MpIClipboardPluginComponent>();

        private readonly string[] _managedDataFormats = {
                //DataFormats.UnicodeText,
                DataFormats.Text,
                DataFormats.Html,
                DataFormats.Rtf,
                DataFormats.Bitmap,
                DataFormats.FileDrop,
                DataFormats.CommaSeparatedValue
            };


        private MpPortableDataObject _LastDataObject = null;
        private MpPortableDataObject _TempDataObject = null; // used when restoring clipboard

        private bool _resetClipboardAfterPaste = false;
        // needed to dispose this form
        private IntPtr _nextClipboardViewer;

        #endregion

        #region MpIPlatfromatDataObjectRegistrar Implmentation

        public int RegisterFormat(string format) {
            return (int)WinApi.RegisterClipboardFormatA(format);

        }

        #endregion

        #region MpIClipboardMonitor Implementation

        public event EventHandler<MpPortableDataObject> OnClipboardChanged;

        public bool IgnoreNextClipboardChangeEvent { get; set; } = false;

        public void StartMonitor() => Start();

        public void StopMonitor() => Stop();

        #endregion

        #region Public Methods

        public void Start() {
            var t = new Thread(
                new ParameterizedThreadStart(x => Application.Run(this)));
            t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
            t.Start();
        }

        // stop listening (dispose form)
        public void Stop() {
            Invoke(new System.Windows.Forms.MethodInvoker(() => {
                WinApi.ChangeClipboardChain(Handle, _nextClipboardViewer);
            }));
            Invoke(new System.Windows.Forms.MethodInvoker(Close));

            Dispose();

        }

        public void SetDataObjectWrapper(MpPortableDataObject mpdo) {
            Clipboard.SetDataObject(ConvertToOleDataObject(mpdo));
        }

        public async Task PasteDataObject(MpPortableDataObject dataObject, IntPtr handle) {
            //to prevent cb listener thread from thinking there's a new item
            IgnoreNextClipboardChangeEvent = true;
            try {
                if (_resetClipboardAfterPaste) {
                    _TempDataObject = _LastDataObject;
                }

                var ido = ConvertToOleDataObject(dataObject);
                Clipboard.SetDataObject(ido);
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);

                await Task.Delay(300);
                System.Windows.Forms.SendKeys.SendWait("^v");

                if (_resetClipboardAfterPaste) {
                    //from https://stackoverflow.com/a/52438404/105028
                    var clipboardThread = new Thread(new ThreadStart(ResetClipboard));
                    clipboardThread.SetApartmentState(ApartmentState.STA);
                    clipboardThread.Start();
                }
                IgnoreNextClipboardChangeEvent = false;
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

            _nextClipboardViewer = WinApi.SetClipboardViewer(Handle);

            base.SetVisibleCore(false);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            switch (m.Msg) {
                case WinApi.WM_DRAWCLIPBOARD:
                    if (IgnoreNextClipboardChangeEvent) {
                        MpConsole.WriteLine("Ignoring this clipboard changed event");
                        IgnoreNextClipboardChangeEvent = false;

                        WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        return;
                    }
                    ClipChanged();
                    WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WinApi.WM_CHANGECBCHAIN:
                    if (m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ClipChanged() {
            //IDataObject iData = Clipboard.GetDataObject();
            //_LastDataObject = ConvertManagedFormats(iData);
            //OnClipboardChanged?.Invoke(this, _LastDataObject);
            var ndo = new MpPortableDataObject();

            foreach (var clipboardHandler in _clipboardHandlers) {
                ndo = clipboardHandler.GetClipboardData();
            }
            if(ndo.DataFormatLookup.Where(x=>x.Value != null).Count() > 0) {
                MpConsole.WriteLine("CB Changed: " + DateTime.Now);
                OnClipboardChanged?.Invoke(typeof(MpClipboardWatcher).ToString(), ndo);
            }
        }

        private MpPortableDataObject ConvertManagedFormats(object ido, int retryCount = 5) {
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
            var cbDict = new MpPortableDataObject();
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
                            cbDict.DataFormatLookup.Add(MpPortableDataFormats.GetDataFormat(cf.ToString()), data.ToString());
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

        private IDataObject ConvertToOleDataObject(MpPortableDataObject mpdo) {
            DataObject dobj = new DataObject();
            foreach (var kvp in mpdo.DataFormatLookup) {
                SetDataWrapper(ref dobj, kvp.Key.Name, kvp.Value.ToString());
            }
            return dobj;
        }

        private void SetDataWrapper(ref DataObject dobj, string format, string dataStr) {
            switch (format) {
                case MpPortableDataFormats.Bitmap:
                    byte[] bytes = Convert.FromBase64String(dataStr);
                    Image image;
                    using (MemoryStream ms = new MemoryStream(bytes)) {
                        image = Image.FromStream(ms);
                        dobj.SetData(DataFormats.Bitmap, image);
                    }

                    break;
                case MpPortableDataFormats.FileDrop:
                    var fl = dataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var sc = new StringCollection();
                    sc.AddRange(fl);
                    dobj.SetFileDropList(sc);
                    break;
                case MpPortableDataFormats.Rtf:
                    dobj.SetData(DataFormats.Rtf, dataStr);
                    break;
                case MpPortableDataFormats.Text:
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


        #endregion

    }
}
