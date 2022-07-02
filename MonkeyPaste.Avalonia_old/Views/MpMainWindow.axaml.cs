using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Rendering;
using System;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Linq;
using PropertyChanged;

namespace MonkeyPaste.Avalonia
{

    [DoNotNotify]
    public partial class MpMainWindow : Window
    {
        public MpMainWindow()
        {
            
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MpMainWindow_AttachedToVisualTree1(object? sender, VisualTreeAttachmentEventArgs e) {
            var screens = this.Screens.All;
            var scaling = ((IRenderRoot)this).RenderScaling;

            //if (screens != null) {
            //    MpPlatformWrapper.Services.ScreenInfoCollection.Screens = screens.Select((x,i) => new MpAvScreenInfo() {
            //        Bounds = new MpRect(new MpPoint(x.Bounds.X,x.Bounds.Y),new MpSize(x.Bounds.Width,x.Bounds.Height)),
            //        IsPrimary = x.Primary,
            //        Name = $"Monitor {i}",
            //        WorkArea = new MpRect(new MpPoint(x.WorkingArea.X, x.WorkingArea.Y), new MpSize(x.WorkingArea.Width, x.WorkingArea.Height))
            //    });
            //}
        }
        //private void Button_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
        //    var btn = sender as Button;
        //    this.Close();
        //}

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
