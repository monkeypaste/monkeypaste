using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public abstract class MpContentContainerViewModelBase : MpUndoableViewModelBase<MpContentContainerViewModelBase> {
        public abstract Size GetTotalExpandedSize();
        public abstract Size GetTotalUnexpandedSize();

        public abstract bool IsDynamicPaste(); // used for template pasting only atm
        public abstract Task UserPreparingDynamicPase();
        public abstract Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false);

        public bool IsAnyContentDragging {
            get {
                return ItemViewModels.Any(x => x.IsSubDragging);
            }
        }

        public bool IsAnyContentDropping {
            get {
                return ItemViewModels.Any(x => x.IsSubDropping);
            }
        }

        public void ClearAllSubDragDropState() {
            foreach (var ivm in ItemViewModels) {
                ivm.ClearSubDragState();
            }
        }

        public bool IsAnySubContextMenuOpened {
            get {
                return ItemViewModels.Any(x => x.IsSubContextMenuOpen);
            }
        }

        public bool IsAnySubSelected {
            get {
                return ItemViewModels.Any(x => x.IsSubSelected);
            }
        }

        public void ResetSubSelection() {
            ClearSubSelection();
            if(VisibleContentItems.Count > 0) {
                VisibleContentItems[0].IsSubSelected = true;
            }
        }

        public void ClearSubSelection() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSubSelected = false;
            }
        }

        public void SubSelectAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSubSelected = true;
            }
        }

        public abstract void ResetContentScroll();

        public abstract MpContentItemViewModelBase Head();
        public abstract MpContentItemViewModelBase Tail();

        public List<MpContentItemViewModelBase> SubSelectedContentItems { 
            get {
                return ItemViewModels.Where(x => x.IsSubSelected == true).ToList();
            }
        }

        public List<MpContentItemViewModelBase> VisibleContentItems {
            get {
                return ItemViewModels.Where(x => x.ItemVisibility == Visibility.Visible).ToList();
            }
        }
        public abstract string GetDetailText(MpCopyItemDetailType detailType);

        public ObservableCollection<MpContentItemViewModelBase> ItemViewModels { get; set; } = new ObservableCollection<MpContentItemViewModelBase>();

        public abstract void InsertRange(int idx, List<MpCopyItem> models);

        public abstract List<MpContentItemViewModelBase> RemoveRange(List<MpCopyItem> models);

        public void SaveAll() {
            foreach(var ivm in ItemViewModels) {
                ivm.SaveToDatabase();
            }            
        }

        public void DeleteAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.RemoveFromDatabase();
            }
        }

        public abstract string GetItemRtf();
        public abstract string GetItemPlainText();
        public abstract string GetItemQuillHtml();
        public abstract string[] GetItemFileList();
    }
}
