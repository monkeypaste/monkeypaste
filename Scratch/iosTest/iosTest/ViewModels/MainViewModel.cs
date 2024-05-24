using SQLitePCL;

namespace iosTest.ViewModels {
    public class MainViewModel : ViewModelBase {
#pragma warning disable CA1822 // Mark members as static
        public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
        public MainViewModel() {
            Batteries_V2.Init();
        }
    }
}
