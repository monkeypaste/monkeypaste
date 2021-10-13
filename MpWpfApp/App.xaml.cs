using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application {
        protected override async void OnStartup(StartupEventArgs e) {
            //PresentationTraceSources.Refresh();
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new MpDebugTraceListener());
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;

            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            MonkeyPaste.MpPreferences.Instance.Init(new MpWpfPreferences());
            await MonkeyPaste.MpDb.Instance.Init(new MpWpfDbInfo());

            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Debugger.Break();
            MonkeyPaste.MpConsole.WriteLine(e);
        }

        //from https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
        //for exception: System.Runtime.InteropServices.COMException: 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))'
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040) {
                e.Handled = true;
            }

            if (comException == null) {
                MonkeyPaste.MpConsole.WriteLine("Mp Handled com exception with null value");
            } else {
                MonkeyPaste.MpConsole.WriteLine("Mp Handled com exception: " + comException.ToString());
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            MpWpfApp.Properties.Settings.Default.Save();
            if (Application.Current.MainWindow != null) {
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).Dispose();
            }
        }
    }
}
