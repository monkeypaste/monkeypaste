using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace iosKeyboardTest {
    public partial class App : Application {
        public const string WAIT_FOR_DEBUG_ARG = "--wait-for-attach";
        public const string BREAK_ON_ATTACH_ARG = "--break-on-attach";
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainViewModel()
                };
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainView {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        public static void WaitForDebug(object[] args) {
            if (!args.Contains(WAIT_FOR_DEBUG_ARG)) {
                return;
            }
            Console.WriteLine("Attach debugger and use 'Set next statement'");
            while (true) {
                Thread.Sleep(100);
                if (Debugger.IsAttached) {
                    if (args.Contains(BREAK_ON_ATTACH_ARG)) {
                        Debugger.Break();
                    }
                    break;
                }
            }
        }
    }
}

