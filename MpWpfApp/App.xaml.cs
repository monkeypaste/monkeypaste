using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MonkeyPaste;

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


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var bs = MpBootstrapper.Init();
            await bs.Initialize();

            MpDb.OnInitDefaultNativeData += MpDb_OnInitDefaultNativeData;

            base.OnStartup(e);
            base.MainWindow = new MpMainWindow();
        }

        private void MpDb_OnInitDefaultNativeData(object sender, EventArgs e) {
            //only occurs on initial load
            //MpPreferences.MainWindowInitialHeight = MpMeasurements.Instance.MainWindowDefaultHeight;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            MonkeyPaste.MpConsole.WriteLine(e);
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

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            var mivm = (sender as FrameworkElement).DataContext as MpMenuItemViewModel;
            mivm.Command.Execute(mivm.CommandParameter);
            if(sender is MenuItem mi) {
                MpContextMenuView.CloseMenu();
            }
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var mivm = (sender as FrameworkElement).DataContext as MpMenuItemViewModel;
            mivm.Command.Execute(mivm.CommandParameter);
            MpContextMenuView.CloseMenu();
        }
    }
}
