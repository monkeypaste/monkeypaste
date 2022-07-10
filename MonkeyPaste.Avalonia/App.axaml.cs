using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using MonkeyPaste.Common;
using PropertyChanged;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class App : Application {
        public App() {
            DataContext = new MpAvAppViewModel();
        }
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);            
        }

        public override async void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                await MpAvWrapper.Instance.InitializeAsync();
                await MpPlatformWrapper.InitAsync(MpAvWrapper.Instance);
                var bootstrapper = new MpAvBootstrapperViewModel();
                await bootstrapper.InitAsync();

                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}
