using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using MonkeyPaste;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MpClipboardHelper.WinApi;

namespace MpClipboardHelper {
    public static class MpClipboardManager {
        #region Private Variables

        private static MpDataObject _LastDataObject = null;
        private static MpDataObject _TempDataObject = null; // used when restoring clipboard
        public static bool IgnoreClipboardChangeEvent = false;

        #endregion

        #region Events

        public static event EventHandler<MpDataObject> OnClipboardChange;

        #endregion

        #region Public Methods

        public static void Start() {
            MpClipboardWatcher.Start();
            MpClipboardWatcher.OnClipboardChange += MpClipboardWatcher_OnClipboardChange;
        }

        public static async Task PasteDataObject(MpDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false) {
            await MpClipboardWatcher.PasteDataObject(mpdo, handle);
            if (finishWithEnterKey) {
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
        }

        public static void SetDataObjectWrapper(MpDataObject mpdo) {
            MpClipboardWatcher.SetDataObjectWrapper(mpdo);
        }

        public static void Stop() {
            OnClipboardChange = null;
            MpClipboardWatcher.Stop();
        }
        #endregion

        #region Private Methods

        private static void MpClipboardWatcher_OnClipboardChange(object sender, MpDataObject e) {
            OnClipboardChange?.Invoke(sender, e);
        }

        #endregion

        #region (internal) ClipboardWatcher

        internal class MpClipboardWatcher : Form {
            #region Private Variables

            private readonly string[] _managedDataFormats = {
                DataFormats.UnicodeText,
                DataFormats.Text,
                DataFormats.Html,
                DataFormats.Rtf,
                DataFormats.Bitmap,
                DataFormats.FileDrop,
                DataFormats.CommaSeparatedValue
            };

            // static instance of this form
            private static MpClipboardWatcher _instance;

            // needed to dispose this form
            private static IntPtr _nextClipboardViewer;

            #endregion

            #region Events

            public static event EventHandler<MpDataObject> OnClipboardChange;

            #endregion

            #region Public Methods

            public static void Start() {
                // we can only have one instance if this class
                if (_instance != null)
                    return;

                var t = new Thread(new ParameterizedThreadStart(x => Application.Run(new MpClipboardWatcher())));
                t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
                t.Start();
            }

            // stop listening (dispose form)
            public static void Stop() {
                _instance.Invoke(new System.Windows.Forms.MethodInvoker(() => {
                    ChangeClipboardChain(_instance.Handle, _nextClipboardViewer);
                }));
                _instance.Invoke(new System.Windows.Forms.MethodInvoker(_instance.Close));

                _instance.Dispose();

                _instance = null;
            }

            public static void SetDataObjectWrapper(MpDataObject mpdo) {
                Clipboard.SetDataObject(ConvertToOleDataObject(mpdo));
            }

            public static async Task PasteDataObject(MpDataObject dataObject, IntPtr handle) {
                //to prevent cb listener thread from thinking there's a new item
                IgnoreClipboardChangeEvent = true;
                try {
                    if (MpPreferences.Instance.ResetClipboardAfterMonkeyPaste) {
                        _TempDataObject = _LastDataObject;
                    }

                    Clipboard.SetDataObject(dataObject);
                    SetForegroundWindow(handle);
                    SetActiveWindow(handle);

                    await Task.Delay(300);
                    System.Windows.Forms.SendKeys.SendWait("^v");

                    if (MpPreferences.Instance.ResetClipboardAfterMonkeyPaste) {
                        //from https://stackoverflow.com/a/52438404/105028
                        var clipboardThread = new Thread(new ThreadStart(ResetClipboard));
                        clipboardThread.SetApartmentState(ApartmentState.STA);
                        clipboardThread.Start();
                    }
                    IgnoreClipboardChangeEvent = false;
                }
                catch (Exception e) {
                    MonkeyPaste.MpConsole.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
                }
                //Mouse.OverrideCursor = null;
            }

            #endregion

            #region Protected Methods

            protected override void SetVisibleCore(bool value) {
                // on load: (hide this window)
                CreateHandle();

                _instance = this;

                _nextClipboardViewer = SetClipboardViewer(_instance.Handle);

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
                OnClipboardChange?.Invoke(this, _LastDataObject);
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
                        if (dobj.GetDataPresent(af)) {
                            switch (af) {
                                case nameof(DataFormats.Text):
                                    data = dobj.GetText(TextDataFormat.Text);
                                    break;
                                case nameof(DataFormats.UnicodeText):
                                    data = dobj.GetText(TextDataFormat.UnicodeText);
                                    break;
                                case nameof(DataFormats.Rtf):
                                    data = dobj.GetText(TextDataFormat.Rtf);
                                    break;
                                case nameof(DataFormats.CommaSeparatedValue):
                                    data = dobj.GetText(TextDataFormat.CommaSeparatedValue);
                                    break;
                                case nameof(DataFormats.Html):
                                    data = dobj.GetText(TextDataFormat.Html);
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
                                    break;
                                case nameof(DataFormats.FileDrop):
                                    StringCollection sc = dobj.GetFileDropList();
                                    string[] sa = new string[sc.Count];
                                    try {
                                        sc.CopyTo(sa, 0);
                                        data = string.Join(Environment.NewLine, sa);
                                    }
                                    catch { }


                                    break;
                            }
                            if (data == null) {
                                data = dobj.GetData(af, true);
                            }
                            if (data != null) {
                                cbDict.DataFormatLookup.Add(af, data.ToString());
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

            private static IDataObject ConvertToOleDataObject(MpDataObject mpdo) {
                DataObject dobj = new DataObject();
                foreach (var kvp in mpdo.DataFormatLookup) {
                    SetDataWrapper(ref dobj, kvp.Key, kvp.Value);
                }
                return dobj;
            }

            private static void SetDataWrapper(ref DataObject dobj, string format, string dataStr) {
                switch (format) {
                    case nameof(DataFormats.Bitmap):
                        byte[] bytes = Convert.FromBase64String(dataStr);
                        Image image;
                        using (MemoryStream ms = new MemoryStream(bytes)) {
                            image = Image.FromStream(ms);
                            dobj.SetData(format, image);
                        }

                        break;
                    case nameof(DataFormats.FileDrop):
                        var fl = dataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        var sc = new StringCollection();
                        sc.AddRange(fl);
                        dobj.SetFileDropList(sc);
                        break;
                    default:
                        dobj.SetData(format, dataStr);
                        break;
                }
            }

            private static void ResetClipboard() {
                if (_TempDataObject == null) {
                    return;
                }
                Clipboard.SetDataObject(ConvertToOleDataObject(_TempDataObject));
            }

            private static bool IsClipboardOpen() {
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

        #endregion
    }
}
