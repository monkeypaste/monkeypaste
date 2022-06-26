using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            var button = this.FindControl<Button>("CloseWindowButton");
            button.Click += Button_Click;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            Window w = (Window)VisualRoot!;
            w.PositionChanged += (sender, args) => InvalidateVisual();
        }


        private void Button_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var btn = sender as Button;
            this.Close();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
