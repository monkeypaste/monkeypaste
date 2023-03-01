using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTagTreeView : MpAvUserControl<MpAvTagTrayViewModel> {
        #region Private Variables
        #endregion
        public MpAvTagTreeView() {
            AvaloniaXamlLoader.Load(this);
            InitDragDrop();
        }

        #region Drag Drop
        private void InitDragDrop() {
            var ttv = this.FindControl<TreeView>("TagTreeView");
            DragDrop.SetAllowDrop(ttv, true);
            ttv.AddHandler(DragDrop.DragOverEvent, TagTreeListBox_DragOver);
            ttv.AddHandler(DragDrop.DragLeaveEvent, TagTreeListBox_DragLeave);
        }
        private void TagTreeListBox_DragOver(object sender, DragEventArgs e) {
            e.DragEffects = DragDropEffects.None;
            var ttv = this.FindControl<TreeView>("TagTreeView");
            ttv.AutoScrollItemsControl(e);
        }
        private void TagTreeListBox_DragLeave(object sender, DragEventArgs e) {
            var ttv = this.FindControl<TreeView>("TagTreeView");
            ttv.AutoScrollItemsControl(null);
        }

        #endregion
    }
}
