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
    public partial class MpAvTagTreeView : MpAvUserControl<MpAvTagTrayViewModel>, MpAvISidebarContentView {
        #region Private Variables
        #endregion
        public MpAvTagTreeView() {
            InitializeComponent();

            var ttv = this.FindControl<TreeView>("TagTreeView");
            //ttv.EnableItemsControlAutoScroll(false);
        }

    }
}
