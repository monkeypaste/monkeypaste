using System;
using System.Windows;
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application {

        //from https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
        //for exception: System.Runtime.InteropServices.COMException: 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))'
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040) {
                e.Handled = true;
            }

            if (comException == null) {
                Console.WriteLine("Mp Handled com exception with null value");
            } else {
                Console.WriteLine("Mp Handled com exception: " + comException.ToString());
            }
        }
    }
}
