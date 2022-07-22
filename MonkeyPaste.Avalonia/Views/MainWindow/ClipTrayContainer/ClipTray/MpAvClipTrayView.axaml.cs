using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        private static MpAvClipTrayView _instance;
        public static MpAvClipTrayView Instance => _instance ?? (_instance = new MpAvClipTrayView());

        private ScrollViewer sv;
        private ListBox lb;

        public MpAvClipTrayView() {
            //DataContext = MpAvClipTrayViewModel.Instance;
            InitializeComponent();            

            sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");
            lb = this.FindControl<ListBox>("ClipTrayListBox");

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void MpAvClipTrayView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            if (Design.IsDesignMode) {
                return;
            }
            //simulate change to initialize layout
            ReceivedGlobalMessage(MpMessageType.TrayLayoutChanged);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChanged:                
                case MpMessageType.TrayLayoutChanged:

                    //sv?.InvalidateArrange();
                    //lb?.InvalidateArrange();
                    //lb_canvas?.InvalidateArrange();

                    //sv?.InvalidateMeasure();
                    //lb?.InvalidateMeasure();
                    //lb_canvas?.InvalidateMeasure();

                    //sv?.InvalidateVisual();
                    //lb?.InvalidateVisual();
                    //lb_canvas?.InvalidateVisual();


                    //lb_canvas?.InvalidateArrange();
                    this.InvalidateMeasure();
                    break;
            }
        }
    }
}
