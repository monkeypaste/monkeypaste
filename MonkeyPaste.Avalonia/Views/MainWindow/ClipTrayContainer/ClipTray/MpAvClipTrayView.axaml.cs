using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
        #region Private Variables

        private ScrollViewer sv;
        private ListBox lb;

        #endregion

        #region Statics

        public static MpAvClipTrayView Instance { get; private set; }

        #endregion

        public MpAvClipTrayView() {
            if (Instance != null) {
                // ensure singleton
                Debugger.Break();
                return;
            }
            Instance = this;
            
            InitializeComponent();

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");            
            lb = this.FindControl<ListBox>("ClipTrayListBox");
            lb.GotFocus += Lb_GotFocus;
            var lb_sv = lb.GetVisualDescendant<ScrollViewer>();
            Dispatcher.UIThread.Post(async () => {
                while(lb_sv == null) {
                    lb_sv = lb.GetVisualDescendant<ScrollViewer>();

                    if(lb_sv == null) {
                        await Task.Delay(100);
                    } else {
                        lb_sv.EffectiveViewportChanged += Lb_EffectiveViewportChanged;
                        return;
                    }
                }
            });
        }

        private void Lb_GotFocus(object sender, global::Avalonia.Input.GotFocusEventArgs e) {
            if (BindingContext.IsTrayEmpty) {
                return;
            }
            if (e.NavigationMethod == NavigationMethod.Tab) {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                    // shift tab from clip tray select last
                    BindingContext.SelectedItem = BindingContext.Items.Last();
                } else {
                    BindingContext.SelectedItem = BindingContext.Items.First();
                }
            }
        }

        private void Lb_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
            // NOTE don't delete below its related to orientation/layout change logic where grid loadMore locks up in vertical mode 
            if(sender is Control control && control.GetVisualDescendant<Canvas>() is Canvas items_panel_canvas) {
                double max_diff = 1.0;
                double w_diff = Math.Abs(items_panel_canvas.Bounds.Width - BindingContext.ClipTrayTotalWidth);
                double h_diff = Math.Abs(items_panel_canvas.Bounds.Height - BindingContext.ClipTrayTotalHeight);
                if (w_diff <= max_diff && h_diff <= max_diff) {
                    // NOTE the lb w/h is bound to total dimensions but only reports screen dimensions
                    // the items panel is CLOSE to actual total dimensions but off by some kind of pixel snapping small value

                    // this is intended to retain scroll position when orientation changes
                    // and this event is triggered twice,
                    // first when the container changes (which the if is trying is set to ignore)
                    // second when the list is transformed                    
                    // the *Begin() is called right before mw rect is changed in mwvm.CycleOrientation

                    //BindingContext.DropScrollAnchor();
                }
            }
            
            return;
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
                case MpMessageType.MainWindowOrientationChangeBegin:                
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
