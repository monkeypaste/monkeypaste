using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public interface MpIClipTileContentViewModelBase {
        MpCopyItem GetModel();
        void SetModel(MpCopyItem newModel);
        bool IsAnySubDragging();
        bool IsAnySubSelected();
        void ClearSubSelection();
        void ClearHovering();
        List<string> GetFileDropList();
        FrameworkElement GetFrameworkElement();
        void Save();
        Size GetContentSize();
        bool IsContextMenuOpened();
        string GetDetail(int detailIdx);
        void Delete();
    }
}
