using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileDetailView : MpAvUserControl<MpAvClipTileDetailCollectionViewModel> {
        public MpAvClipTileDetailView() {
            InitializeComponent();
            //this.PointerEnter += MpAvClipTileDetailView_PointerEnter;
            var tb = this.FindControl<TextBlock>("ClipTile_Detail_TextBlock");
            tb.PointerLeave += MpAvClipTileDetailView_PointerLeave;
            //this.PointerPressed += MpAvClipTileDetailView_PointerLeave;
        }

        //private void MpAvClipTileDetailView_PointerEnter(object sender, PointerEventArgs e) {
        //    var ctv = this.GetVisualAncestor<MpAvClipTileView>();
        //    ctv.GetVisualDescendant<MpAvClipTileContentView>().IsHitTestVisible = false;
        //}

        private void MpAvClipTileDetailView_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e) {
            BindingContext.CycleDetailCommand.Execute(null);
            //var ctv = this.GetVisualAncestor<MpAvClipTileView>();
            //ctv.GetVisualDescendant<MpAvClipTileContentView>().IsHitTestVisible = false;

            //if (BindingContext == null) {
            //    return;
            //}
            ////var tbc = sender as Control;
            ////var mp = e.GetClientMousePoint(tbc);
            ////var b = tbc.Bounds.ToPortableRect();
            ////if(!b.Contains(mp)) {
            ////}

            //BindingContext.CycleDetailCommand.Execute(null);
        }


        //private void MpAvClipTileDetailView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
        //    //if (BindingContext == null) {
        //    //    return;
        //    //}

        //    //var sdvm = BindingContext.SelectedItem;

        //    //bool can_goto = sdvm != null && sdvm.IsUriEnabled && e.KeyModifiers.HasFlag(KeyModifiers.Control);
        //    //if (can_goto) {
        //    //    if (OperatingSystem.IsWindows()) {
        //    //        Process.Start("explorer", sdvm.DetailUri);
        //    //    } else {
        //    //        using (var myProcess = new Process()) {
        //    //            myProcess.StartInfo.UseShellExecute = true;
        //    //            myProcess.StartInfo.FileName = sdvm.DetailUri;
        //    //            myProcess.Start();
        //    //        }
        //    //    }
        //    //    return;
        //    //}

        //    //BindingContext.CycleDetailCommand.Execute(null);
        //}
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
