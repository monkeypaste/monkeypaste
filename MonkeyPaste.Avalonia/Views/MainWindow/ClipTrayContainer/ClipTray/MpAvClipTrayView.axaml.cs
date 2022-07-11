using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Media.TextFormatting;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTrayView : MpAvUserControl<MpAvClipTrayViewModel> {
        private static MpAvClipTrayView _instance;
        public static MpAvClipTrayView Instance => _instance ?? (_instance = new MpAvClipTrayView());

        private ScrollViewer sv;
        private ItemsRepeater ir;

        public ItemsRepeater TrayRepeater => Instance.ir;


        public MpAvClipTrayView() {
            InitializeComponent();
            sv = this.FindControl<ScrollViewer>("ClipTrayScrollViewer");
            ir = this.FindControl<ItemsRepeater>("ClipTrayItemsRepeater");

            sv.ScrollChanged += Sv_ScrollChanged;
            this.DataContextChanged += MpAvClipTrayView_DataContextChanged;
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var sv = (ScrollViewer)sender;
            if(e.OffsetDelta.Y != 0) {
                var distanceToEnd = sv.Extent.Height - (sv.Offset.Y + sv.Viewport.Height);

                // trigger if within 2 viewports of the end
                //if (distanceToEnd <= 2.0 * sv.Viewport.Height
                //        && MyItemsSource.HasMore && !itemsSource.Busy) {
                //    // show an indeterminate progress UI
                //    myLoadingIndicator.Visibility = Visibility.Visible;

                //    await MyItemsSource.LoadMoreItemsAsync(/*DataFetchSize*/);

                //    loadingIndicator.Visibility = Visibility.Collapsed;
                //}
            }
            
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
            if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                e.TemplateKey = "ClipTileHorizontalTemplate";
            } else {
                e.TemplateKey = "ClipTileVerticalTemplate";
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChanged:
                case MpMessageType.TrayLayoutChanged:
                    var mwvm = MpAvMainWindowViewModel.Instance;

                    double minTileSize = MpAvClipTrayViewModel.Instance.ZoomFactor;

                    if (BindingContext.LayoutType == MpAvClipTrayLayoutType.Grid) {
                        // grid
                        if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                            // horizontal grid
                            ir.Layout = new UniformGridLayout {
                                Orientation = Orientation.Horizontal,
                                MinItemHeight = minTileSize,
                                MinItemWidth = minTileSize,
                                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                                ItemsJustification = UniformGridLayoutItemsJustification.Start,
                                MinRowSpacing = 5,
                                MinColumnSpacing = 5
                            };
                            ir.MaxWidth = mwvm.MainWindowWidth;
                            ir.MaxHeight = double.PositiveInfinity;
                        } else {
                            // vertical grid
                            ir.Layout = new UniformGridLayout {
                                Orientation = Orientation.Horizontal,
                                MinItemHeight = minTileSize,
                                MinItemWidth = minTileSize,
                                ItemsStretch = UniformGridLayoutItemsStretch.Fill,
                                ItemsJustification = UniformGridLayoutItemsJustification.Start,
                                MinRowSpacing = 5,
                                MinColumnSpacing = 5
                            };
                            ir.MaxWidth = mwvm.MainWindowWidth; 
                            ir.MaxHeight = double.PositiveInfinity;
                        }
                    } else {
                        // stack
                        if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                            // horizontal stack
                            ir.Layout = new StackLayout {
                                Orientation = Orientation.Horizontal,
                                Spacing = 5,
                            };
                            ir.MaxWidth = double.PositiveInfinity;
                            ir.MaxHeight = double.PositiveInfinity;
                        } else {
                            // vertical stack
                            ir.Layout = new StackLayout {
                                Orientation = Orientation.Vertical,
                                Spacing = 5
                            };
                            ir.MaxWidth = double.PositiveInfinity;
                            ir.MaxHeight = double.PositiveInfinity;
                        }

                    }
                    ir.InvalidateMeasure();
                    sv.InvalidateMeasure();
                    break;
            }
        }
    }
}
