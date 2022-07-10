using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        private ScrollViewer sv;
        private ItemsRepeater ir;

        public MpAvClipTrayView() {
            InitializeComponent();
            sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");
            ir = this.FindControl<ItemsRepeater>("ClipTrayItemsRepeater");
            this.DataContextChanged += MpAvClipTrayView_DataContextChanged;
        }

        private void MpAvClipTrayView_DataContextChanged(object sender, EventArgs e) {
            if(DataContext == null) {
                return;
            }

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


        public void OnSelectTemplateKey(object sender, SelectTemplateEventArgs e) {
            //if (e.DataContext is ItemsRepeaterPageViewModel.Item item) {
            //    e.TemplateKey = (item.Index % 2 == 0) ? "even" : "odd";
            //}
            e.TemplateKey = "ClipTileTemplate";
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOrientationChanged:
                case MpMessageType.TrayLayoutChanged:
                    if(BindingContext.LayoutType == MpAvClipTrayLayoutType.Grid) {
                        // grid
                        if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                            // horizontal grid
                            ir.Layout = new WrapLayout {
                                Orientation = Orientation.Horizontal, 
                                HorizontalSpacing = 5,
                                VerticalSpacing = 5
                            };
                        } else {
                            // vertical grid
                            ir.Layout = new WrapLayout {
                                Orientation = Orientation.Vertical,
                                HorizontalSpacing = 5,
                                VerticalSpacing = 5
                            };
                        }
                    } else {
                        // stack
                        if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                            // horizontal stack
                            ir.Layout = new StackLayout {
                                Orientation = Orientation.Horizontal,
                                Spacing = 5,
                            };
                        } else {
                            // vertical stack
                            ir.Layout = new StackLayout {
                                Orientation = Orientation.Vertical,
                                Spacing = 5
                            };
                        }
                        //if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                        //    // horizontal stack
                        //    ir.Layout = new UniformGridLayout {
                        //        Orientation = Orientation.Horizontal,
                        //        //MaximumRowsOrColumns = int.MaxValue,
                        //        MinItemHeight = 250,
                        //        MinItemWidth = 250,
                        //        MinColumnSpacing = 5
                        //    };
                        //} else {
                        //    // vertical stack
                        //    ir.Layout = new UniformGridLayout {
                        //        Orientation = Orientation.Vertical,
                        //        MaximumRowsOrColumns = 1,
                        //        MinItemHeight = 250,
                        //        MinItemWidth = 250,
                        //        MinColumnSpacing = 5
                        //    };
                        //}

                    }
                    ir.InvalidateMeasure();
                    sv.InvalidateMeasure();
                    break;
            }
        }
    }
}
