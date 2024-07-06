using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is not ISingleViewApplicationLifetime singleViewPlatform) {
                return;
            }
            singleViewPlatform.MainView = new MainView {
                DataContext = new MainViewModel()
            };
            base.OnFrameworkInitializationCompleted();
        }
    }
}
