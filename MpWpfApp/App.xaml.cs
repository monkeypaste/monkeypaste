using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application {
        private static Mutex SingleMutex;
        public static uint MessageId;

        protected override async void OnStartup(StartupEventArgs e) {
            // single-instance stuff from https://stackoverflow.com/a/5919904/105028

            //PresentationTraceSources.Refresh();
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new MpDebugTraceListener());
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            IntPtr Result;
            IntPtr SendOk;
            Win32.COPYDATASTRUCT CopyData;
            string[] Args;
            IntPtr CopyDataMem;
            bool AllowMultipleInstances = false;

            Args = Environment.GetCommandLineArgs();

            // TODO: Replace {00000000-0000-0000-0000-000000000000} with your application's GUID
            MessageId = Win32.RegisterWindowMessage("2380ae52-7725-4929-8c79-d64223e7f686");
            SingleMutex = new Mutex(false, "MonkeyPaste");

            if (AllowMultipleInstances || (!AllowMultipleInstances && SingleMutex.WaitOne(1, true))) {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


                base.OnStartup(e);
                //await MpWpfBootstrapperViewModel.Init();

                Xamarin.Forms.Forms.Init();
                

                var bootstrapper = new MpWpfBootstrapperViewModel(new MpWpfWrapper());
                await bootstrapper.Init();

                MpDb.OnInitDefaultNativeData += MpDb_OnInitDefaultNativeData;
                base.MainWindow = new MpMainWindow();
            } else if (Args.Length > 1) {
                foreach (Process Proc in Process.GetProcesses()) {
                    SendOk = Win32.SendMessageTimeout(Proc.MainWindowHandle, MessageId, IntPtr.Zero, IntPtr.Zero,
                        Win32.SendMessageTimeoutFlags.SMTO_BLOCK | Win32.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                        2000, out Result);

                    if (SendOk == IntPtr.Zero)
                        continue;
                    if ((uint)Result != MessageId)
                        continue;

                    CopyDataMem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Win32.COPYDATASTRUCT)));

                    CopyData.dwData = IntPtr.Zero;
                    CopyData.cbData = Args[1].Length * 2;
                    CopyData.lpData = Marshal.StringToHGlobalUni(Args[1]);

                    Marshal.StructureToPtr(CopyData, CopyDataMem, false);

                    Win32.SendMessageTimeout(Proc.MainWindowHandle, Win32.WM_COPYDATA, IntPtr.Zero, CopyDataMem,
                        Win32.SendMessageTimeoutFlags.SMTO_BLOCK | Win32.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                        5000, out Result);

                    Marshal.FreeHGlobal(CopyData.lpData);
                    Marshal.FreeHGlobal(CopyDataMem);
                }

                Shutdown(0);
            }

        }

        public void Activate() {
            // Reactivate the main window
            MainWindow.Activate();
        }

        private void MpDb_OnInitDefaultNativeData(object sender, EventArgs e) {
            //only occurs on initial load
            Task.Run(async () => {
                //MpPreferences.MainWindowInitialHeight = MpMeasurements.Instance.MainWindowDefaultHeight;
                // TEMPORARY ONLY WPF (NOTE !! Use env variables to find windows/system32 folder NOT C:)
                // NOTEPAD
                var notepadApp = await MpApp.Create(
                    guid: "9a0085ac-ea3c-4213-9276-08017d0e4ef2",
                    appPath: @"c:\windows\system32\notepad.exe",
                    appName: "Notepad",
                    icon: null);

                var notepadSetting1 = await MpAppClipboardFormatInfo.Create(
                    appId: notepadApp.Id,
                    format: MpClipboardFormatType.Bitmap,
                    ignoreFormat: true);

                var notepadSetting2 = await MpAppClipboardFormatInfo.Create(
                    appId: notepadApp.Id,
                    format: MpClipboardFormatType.Rtf,
                    ignoreFormat: true);

                var notepadSetting3 = await MpAppClipboardFormatInfo.Create(
                    appId: notepadApp.Id,
                    format: MpClipboardFormatType.Text,
                    ignoreFormat: true);

                var notepadSetting4 = await MpAppClipboardFormatInfo.Create(
                   appId: notepadApp.Id,
                   format: MpClipboardFormatType.Csv,
                   ignoreFormat: true);

                var notepadSetting5 = await MpAppClipboardFormatInfo.Create(
                    appId: notepadApp.Id,
                    format: MpClipboardFormatType.FileDrop,
                    formatInfo: "txt");

                //EXPLORER
                var explorerApp = await MpApp.Create(
                    guid: "81c2f520-0568-4b7f-b704-9ca9f9e22c1a",
                    appPath: @"c:\windows\explorer.exe",
                    appName: "Explorer",
                    icon: null);

                var explorerSetting1 = await MpAppClipboardFormatInfo.Create(
                    appId: explorerApp.Id,
                    format: MpClipboardFormatType.Bitmap,
                    ignoreFormat: true);

                var explorerSetting2 = await MpAppClipboardFormatInfo.Create(
                    appId: explorerApp.Id,
                    format: MpClipboardFormatType.Rtf,
                    ignoreFormat: true);

                var explorerSetting3 = await MpAppClipboardFormatInfo.Create(
                    appId: explorerApp.Id,
                    format: MpClipboardFormatType.Text,
                    ignoreFormat: true);

                var explorerSetting4 = await MpAppClipboardFormatInfo.Create(
                    appId: explorerApp.Id,
                    format: MpClipboardFormatType.Csv,
                    ignoreFormat: true);

                var explorerSetting5 = await MpAppClipboardFormatInfo.Create(
                    appId: explorerApp.Id,
                    format: MpClipboardFormatType.FileDrop);

                //MSPAINT
                var paintApp = await MpApp.Create(
                    guid: "ad30c88e-372c-44e7-89df-124e8b874624",
                    appPath: @"c:\windows\system32\mspaint.exe",
                    appName: "Paint",
                    icon: null);

                var paintSetting1 = await MpAppClipboardFormatInfo.Create(
                    appId: paintApp.Id,
                    format: MpClipboardFormatType.Bitmap,
                    ignoreFormat: true);

                var paintSetting2 = await MpAppClipboardFormatInfo.Create(
                    appId: paintApp.Id,
                    format: MpClipboardFormatType.Rtf,
                    ignoreFormat: true);

                var paintSetting3 = await MpAppClipboardFormatInfo.Create(
                    appId: paintApp.Id,
                    format: MpClipboardFormatType.Text,
                    ignoreFormat: true);

                var paintSetting4 = await MpAppClipboardFormatInfo.Create(
                   appId: paintApp.Id,
                   format: MpClipboardFormatType.Csv,
                   ignoreFormat: true);

                var paintSetting5 = await MpAppClipboardFormatInfo.Create(
                    appId: paintApp.Id,
                    format: MpClipboardFormatType.FileDrop,
                    formatInfo: "bmp");
            });            
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            MpConsole.WriteLine(e);
            //Debugger.Break();
        }

        //from https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
        //for exception: System.Runtime.InteropServices.COMException: 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))'
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
          var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040) {
                e.Handled = true;
            }

            if (comException == null) {
                MpConsole.WriteLine("Mp Handled com exception with null value");
            } else {
                MpConsole.WriteLine("Mp Handled com exception: " + comException.ToString());
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            MpWpfApp.Properties.Settings.Default.Save();
            if (Application.Current.MainWindow != null) {
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).Dispose();
            }
        }

        
    }

    public class Win32 {
        public const uint WM_COPYDATA = 0x004A;

        public struct COPYDATASTRUCT {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [Flags]
        public enum SendMessageTimeoutFlags : uint {
            SMTO_NORMAL = 0x0000,
            SMTO_BLOCK = 0x0001,
            SMTO_ABORTIFHUNG = 0x0002,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam,
            SendMessageTimeoutFlags fuFlags, uint uTimeout, out IntPtr lpdwResult);
    }
}
