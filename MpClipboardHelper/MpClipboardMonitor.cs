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

namespace MpClipboardHelper {
    public static class MpClipboardMonitor {
        public delegate void OnClipboardChangeEventHandler(object sender, Dictionary<string, string> data);
        public static event OnClipboardChangeEventHandler OnClipboardChange;

        //public static event EventHandler<Dictionary<string, string>> ClipboardChanged;

        public static void Start() {
            MpClipboardWatcher.Start();
            MpClipboardWatcher.OnClipboardChange += (object sender, Dictionary<string, string> data) => {
                if (OnClipboardChange != null)
                    OnClipboardChange(sender, data);
            };

        }

        public static void Stop() {
            OnClipboardChange = null;
            MpClipboardWatcher.Stop();
        }

        class MpClipboardWatcher : Form {
            // static instance of this form
            private static MpClipboardWatcher mInstance;

            // needed to dispose this form
            static IntPtr nextClipboardViewer;

            public delegate void OnClipboardChangeEventHandler(object sender, Dictionary<string, string> data);
            public static event OnClipboardChangeEventHandler OnClipboardChange;



            // start listening
            public static void Start() {
                // we can only have one instance if this class
                if (mInstance != null)
                    return;

                var t = new Thread(new ParameterizedThreadStart(x => System.Windows.Forms.Application.Run(new MpClipboardWatcher())));
                t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
                t.Start();
            }

            // stop listening (dispose form)
            public static void Stop() {
                mInstance.Invoke(new System.Windows.Forms.MethodInvoker(() => {
                    ChangeClipboardChain(mInstance.Handle, nextClipboardViewer);
                }));
                mInstance.Invoke(new System.Windows.Forms.MethodInvoker(mInstance.Close));

                mInstance.Dispose();

                mInstance = null;
            }

            // on load: (hide this window)
            protected override void SetVisibleCore(bool value) {
                CreateHandle();

                mInstance = this;

                nextClipboardViewer = SetClipboardViewer(mInstance.Handle);

                base.SetVisibleCore(false);
            }

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            protected override void WndProc(ref System.Windows.Forms.Message m) {
                switch (m.Msg) {
                    case WM_DRAWCLIPBOARD:
                        ClipChanged();
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    case WM_CHANGECBCHAIN:
                        if (m.WParam == nextClipboardViewer)
                            nextClipboardViewer = m.LParam;
                        else
                            SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                        break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

            static readonly string[] formats = Enum.GetNames(typeof(ClipboardFormat));

            private void ClipChanged() {
                IDataObject iData = Clipboard.GetDataObject();

                //ClipboardFormat? format = null;

                //foreach (var f in formats) {
                //    if (iData.GetDataPresent(f)) {
                //        format = (ClipboardFormat)Enum.Parse(typeof(ClipboardFormat), f);
                //        break;
                //    }
                //}

                //object data = iData.GetData(format.ToString());

                //if (data == null || format == null)
                //    return;

                //if (OnClipboardChange != null)
                //    OnClipboardChange((ClipboardFormat)format, data);

                var cbo = ConvertManagedFormats(iData);
                OnClipboardChange?.Invoke(this, cbo);
            }
            private readonly string[] _managedDataFormats = {
            DataFormats.UnicodeText,
            DataFormats.Text,
            DataFormats.Html,
            DataFormats.Rtf,
            DataFormats.Bitmap,
            DataFormats.FileDrop,
            DataFormats.CommaSeparatedValue
        };

            private Dictionary<string, string> ConvertManagedFormats(object ido, int retryCount = 5) {
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
                var cbDict = new Dictionary<string, string>();
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
                                        //using (MemoryStream memStream = new MemoryStream()) {
                                        //    binFormatter.Serialize(memStream, img);
                                        //    byte[] bytes = memStream.ToArray();
                                        //    data = Convert.ToBase64String(bytes);
                                        //}

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
                                cbDict.Add(af, data.ToString());
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
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int GetWindowText(int hwnd, StringBuilder text, int count);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowTextLength(int hwnd);

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
    }

    public enum ClipboardFormat : byte {
        /// <summary>Specifies the standard ANSI text format. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Text,
        /// <summary>Specifies the standard Windows Unicode text format. This static field
        /// is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        UnicodeText,
        /// <summary>Specifies the Windows device-independent bitmap (DIB) format. This static
        /// field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Dib,
        /// <summary>Specifies a Windows bitmap format. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Bitmap,
        /// <summary>Specifies the Windows enhanced metafile format. This static field is
        /// read-only.</summary>
        /// <filterpriority>1</filterpriority>
        EnhancedMetafile,
        /// <summary>Specifies the Windows metafile format, which Windows Forms does not
        /// directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        MetafilePict,
        /// <summary>Specifies the Windows symbolic link format, which Windows Forms does
        /// not directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        SymbolicLink,
        /// <summary>Specifies the Windows Data Interchange Format (DIF), which Windows Forms
        /// does not directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Dif,
        /// <summary>Specifies the Tagged Image File Format (TIFF), which Windows Forms does
        /// not directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Tiff,
        /// <summary>Specifies the standard Windows original equipment manufacturer (OEM)
        /// text format. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        OemText,
        /// <summary>Specifies the Windows palette format. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Palette,
        /// <summary>Specifies the Windows pen data format, which consists of pen strokes
        /// for handwriting software, Windows Forms does not use this format. This static
        /// field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        PenData,
        /// <summary>Specifies the Resource Interchange File Format (RIFF) audio format,
        /// which Windows Forms does not directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Riff,
        /// <summary>Specifies the wave audio format, which Windows Forms does not directly
        /// use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        WaveAudio,
        /// <summary>Specifies the Windows file drop format, which Windows Forms does not
        /// directly use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        FileDrop,
        /// <summary>Specifies the Windows culture format, which Windows Forms does not directly
        /// use. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Locale,
        /// <summary>Specifies text consisting of HTML data. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Html,
        /// <summary>Specifies text consisting of Rich Text Format (RTF) data. This static
        /// field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Rtf,
        /// <summary>Specifies a comma-separated value (CSV) format, which is a common interchange
        /// format used by spreadsheets. This format is not used directly by Windows Forms.
        /// This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        CommaSeparatedValue,
        /// <summary>Specifies the Windows Forms string class format, which Windows Forms
        /// uses to store string objects. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        StringFormat,
        /// <summary>Specifies a format that encapsulates any type of Windows Forms object.
        /// This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Serializable,
    }
}
