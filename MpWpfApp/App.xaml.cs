using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;


namespace MpWpfApp {
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application {

        //from https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
        //for exception: System.Runtime.InteropServices.COMException: 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN))'
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040) {
                e.Handled = true;
            }

            Console.WriteLine("Mp Handled com exception: " + comException.ToString());                
        }
    }
}
