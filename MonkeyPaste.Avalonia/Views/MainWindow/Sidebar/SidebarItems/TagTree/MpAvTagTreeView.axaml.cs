using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTreeView : MpAvUserControl<MpAvTagTrayViewModel> {

        public MpAvTagTreeView() {
            InitializeComponent();

            //var tagTreeView = this.FindControl<TreeView>("TagTreeView");
            //tagTreeView.ItemContainerGenerator.Materialized += ItemContainerGenerator_Materialized;
            //tagTreeView.SelectionChanged += TagTreeView_SelectionChanged;
        }

        //private void TagTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        //    return;
        //}

        //private void ItemContainerGenerator_Materialized(object sender, global::Avalonia.Controls.Generators.ItemContainerEventArgs e) {
        //    foreach(var ici in e.Containers) {
        //        TreeViewItem tvi = ici.ContainerControl as TreeViewItem;
        //        tvi.GetObservable(IsPointerOverProperty).Subscribe(value => TreeViewItem_IsPointerOverChange(tvi, value));
        //        //tvi.PointerPressed += Tvi_PointerPressed;
        //    }
        //    return;
        //}

        //private void Tvi_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
        //    if(sender is TreeViewItem tvi && 
        //        tvi.DataContext is MpAvTagTileViewModel ttvm) {
        //        if(e.GetCurrentPoint(tvi).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) {
        //            //ttvm.Parent.SelectTagCommand.Execute(ttvm);
        //        } else if (e.GetCurrentPoint(tvi).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
        //            //MpAvMenuExtension.ShowContextMenu(tvi);
        //        }
        //    }
        //}

        //void TreeViewItem_IsPointerOverChange(TreeViewItem tvi, bool isPointerOver) {
        //    if(tvi.DataContext is MpAvTagTileViewModel ttvm) {
        //        //ttvm.IsHovering = isPointerOver;
        //    }
        //}

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
