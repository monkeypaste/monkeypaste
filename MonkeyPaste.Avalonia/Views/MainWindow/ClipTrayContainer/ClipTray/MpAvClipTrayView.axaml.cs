using Avalonia;
using Avalonia.Controls;
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

            //ir.PointerWheelChanged += ClipTrayScrollView_PointerWheelChanged;
        }

        private void ClipTrayScrollView_PointerWheelChanged(object sender, global::Avalonia.Input.PointerWheelEventArgs e) {
            double xOffset = e.Delta.Y > 0 ? -20 : -20;
            var newOffset = new Vector(
                Math.Max(0, Math.Min(sv.Extent.Width, sv.Offset.X + xOffset)), 
                sv.Offset.Y);

            sv.Offset = newOffset;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void MpAvClipTrayView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            if (Design.IsDesignMode) {
                return;
            }
            this.BindingContext.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Debugger.Break();
        }

        public void OnSelectTemplateKey(object sender, SelectTemplateEventArgs e) {
            //if (e.DataContext is ItemsRepeaterPageViewModel.Item item) {
            //    e.TemplateKey = (item.Index % 2 == 0) ? "even" : "odd";
            //}
            e.TemplateKey = "ClipTileTemplate";
        }
    }
}
