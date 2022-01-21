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


            // TODO add color pallete colors and all app.xaml icons as MpIcons and store guids (for initDefaultData)
            await Temp();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var bs = MpBootstrapper.Init();
            await bs.Initialize();
            //MpPreferences.Init(new MpWpfPreferences());

            MpDb.OnInitDefaultNativeData += MpDb_OnInitDefaultNativeData;
            //await MpDb.Init(new MpWpfDbInfo());

            //await MpMainWindowViewModel.Instance.Init();

            //
            
            base.OnStartup(e);
            base.MainWindow = new MpMainWindow();
        }
        private async Task Temp() {
            List<List<Brush>> _colors = new List<List<Brush>> {
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(248, 160, 174)),
                    new SolidColorBrush(Color.FromRgb(243, 69, 68)),
                    new SolidColorBrush(Color.FromRgb(229, 116, 102)),
                    new SolidColorBrush(Color.FromRgb(211, 159, 161)),
                    new SolidColorBrush(Color.FromRgb(191, 53, 50))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(252, 168, 69)),
                    new SolidColorBrush(Color.FromRgb(251, 108, 40)),
                    new SolidColorBrush(Color.FromRgb(253, 170, 130)),
                    new SolidColorBrush(Color.FromRgb(189, 141, 103)),
                    new SolidColorBrush(Color.FromRgb(177, 86, 55))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(215, 157, 60)),
                    new SolidColorBrush(Color.FromRgb(168, 123, 82)),
                    new SolidColorBrush(Color.FromRgb(214, 182, 133)),
                    new SolidColorBrush(Color.FromRgb(162, 144, 122)),
                    new SolidColorBrush(Color.FromRgb(123, 85, 72))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(247, 245, 144)),
                    new SolidColorBrush(Color.FromRgb(252, 240, 78)),
                    new SolidColorBrush(Color.FromRgb(239, 254, 185)),
                    new SolidColorBrush(Color.FromRgb(198, 193, 127)),
                    new SolidColorBrush(Color.FromRgb(224, 200, 42))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(189, 254, 40)),
                    new SolidColorBrush(Color.FromRgb(143, 254, 115)),
                    new SolidColorBrush(Color.FromRgb(217, 231, 170)),
                    new SolidColorBrush(Color.FromRgb(172, 183, 38)),
                    new SolidColorBrush(Color.FromRgb(140, 157, 45))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(50, 255, 76)),
                    new SolidColorBrush(Color.FromRgb(68, 199, 33)),
                    new SolidColorBrush(Color.FromRgb(193, 214, 135)),
                    new SolidColorBrush(Color.FromRgb(127, 182, 99)),
                    new SolidColorBrush(Color.FromRgb(92, 170, 58))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(54, 255, 173)),
                    new SolidColorBrush(Color.FromRgb(32, 195, 178)),
                    new SolidColorBrush(Color.FromRgb(170, 206, 160)),
                    new SolidColorBrush(Color.FromRgb(160, 201, 197)),
                    new SolidColorBrush(Color.FromRgb(32, 159, 148))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(96, 255, 227)),
                    new SolidColorBrush(Color.FromRgb(46, 238, 249)),
                    new SolidColorBrush(Color.FromRgb(218, 253, 233)),
                    new SolidColorBrush(Color.FromRgb(174, 193, 208)),
                    new SolidColorBrush(Color.FromRgb(40, 103, 146))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(149, 204, 243)),
                    new SolidColorBrush(Color.FromRgb(43, 167, 237)),
                    new SolidColorBrush(Color.FromRgb(215, 244, 248)),
                    new SolidColorBrush(Color.FromRgb(153, 178, 198)),
                    new SolidColorBrush(Color.FromRgb(30, 51, 160))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(99, 141, 227)),
                    new SolidColorBrush(Color.FromRgb(22, 127, 193)),
                    new SolidColorBrush(Color.FromRgb(201, 207, 233)),
                    new SolidColorBrush(Color.FromRgb(150, 163, 208)),
                    new SolidColorBrush(Color.FromRgb(52, 89, 170))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(157, 176, 255)),
                    new SolidColorBrush(Color.FromRgb(148, 127, 220)),
                    new SolidColorBrush(Color.FromRgb(216, 203, 233)),
                    new SolidColorBrush(Color.FromRgb(180, 168, 192)),
                    new SolidColorBrush(Color.FromRgb(109, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(221, 126, 230)),
                    new SolidColorBrush(Color.FromRgb(186, 141, 200)),
                    new SolidColorBrush(Color.FromRgb(185, 169, 231)),
                    new SolidColorBrush(Color.FromRgb(203, 178, 200)),
                    new SolidColorBrush(Color.FromRgb(170, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(225, 103, 164)),
                    new SolidColorBrush(Color.FromRgb(252, 74, 210)),
                    new SolidColorBrush(Color.FromRgb(238, 233, 237)),
                    new SolidColorBrush(Color.FromRgb(195, 132, 163)),
                    new SolidColorBrush(Color.FromRgb(205, 60, 117))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(223, 223, 223)),
                    new SolidColorBrush(Color.FromRgb(187, 187, 187)),
                    new SolidColorBrush(Color.FromRgb(137, 137, 137)),
                    new SolidColorBrush(Color.FromRgb(65, 65, 65))
                }
            };
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
    }
}
