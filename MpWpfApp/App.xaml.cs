using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MonkeyPaste;
using MonkeyPaste.Plugin;

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
            //MpPreferences.MainWindowInitialHeight = MpMeasurements.Instance.MainWindowDefaultHeight;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            var mivm = (sender as FrameworkElement).DataContext as MpMenuItemViewModel;
            mivm.Command.Execute(mivm.CommandParameter);
            if(sender is MenuItem mi) {
                MpContextMenuView.Instance.CloseMenu();
            }
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var mivm = (sender as FrameworkElement).DataContext as MpMenuItemViewModel;
            mivm.Command.Execute(mivm.CommandParameter);
            MpContextMenuView.Instance.CloseMenu();
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
