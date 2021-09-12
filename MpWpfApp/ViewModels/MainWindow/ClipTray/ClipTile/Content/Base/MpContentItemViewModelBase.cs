using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public abstract class MpContentItemViewModelBase : MpUndoableViewModelBase<MpContentItemViewModelBase> {
        public abstract Size GetExpandedSize();
        public abstract Size GetUnexpandedSize();
        public abstract void Resize(Rect newSize);

        public DateTime LastSubSelectedDateTime { get; set; }

        public bool IsSubSelected { get; set; } = false;
        public bool IsSubHovering { get; set; } = false;
        public bool IsSubContextMenuOpen { get; set; } = false;

        public bool IsSubEditingContent { get; set; } = false;
        public bool IsSubEditingTitle { get; set; } = false;

        public bool IsSubDragging { get; set; } = false;
        public bool IsSubDropping { get; set; } = false;
        public Point MouseDownPosition { get; set; }
        public IDataObject DragDataObject { get; set; }

        public void ClearSubDragState() {
            IsSubDragging = false;
            DragDataObject = null;
            MouseDownPosition = new Point();
        }
        public Visibility ContentItemVisibility { get; set; } = Visibility.Visible;

        public MpCopyItem CopyItem { get; set; }

        public abstract List<string> GetDropFileList();

        public abstract MpContentItemViewModelBase Next();
        public abstract MpContentItemViewModelBase Previous();
        public abstract int GetSortOrderIdx();

        public void SaveToDatabase() {
            CopyItem.WriteToDatabase();
        }

        public void RemoveFromDatabase() {
            CopyItem.DeleteFromDatabase();
        }

        public void MoveToArchive() {
            // TODO maybe add archiving
        }

        public abstract ICommand EditContentCommand { get; }
        public abstract ICommand EditTitleCommand { get; }
        public abstract ICommand AssignHotkeyCommand { get; }

    }
}
