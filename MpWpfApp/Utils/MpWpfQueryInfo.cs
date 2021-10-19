using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfQueryInfo : MpQueryInfo {

        public override bool IsDescending {
            get {
                return MpClipTileSortViewModel.Instance.IsSortDescending;
            }
        }

        public override MpContentSortType SortType { 
            get {
                if(MpClipTileSortViewModel.Instance.SelectedSortType == null) {
                    return MpContentSortType.None;
                }
                return MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            }        
        }

        public override int TagId {
            get {
                if(MpTagTrayViewModel.Instance.SelectedTagTile == null) {
                    return MpTag.RecentTagId;
                }
                return MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            }
        }

        public override string SearchText {
            get {
                if(!MpSearchBoxViewModel.Instance.HasText) {
                    return string.Empty;
                }
                return MpSearchBoxViewModel.Instance.Text;
            }
        }
               

        public override MpContentFilterType FilterFlags {
            get {
                return MpSearchBoxViewModel.Instance.FilterType;
            }
        }
        public event EventHandler InfoChanged;

        public MpWpfQueryInfo() {
            MpTagTrayViewModel.Instance.OnTagSelectionChanged += Instance_OnTagSelectionChanged;
            MpSearchBoxViewModel.Instance.OnFilterFlagsChanged += Instance_OnFilterFlagsChanged;
            MpSearchBoxViewModel.Instance.OnSearchTextChanged += Instance_OnSearchTextChanged;
            MpClipTileSortViewModel.Instance.OnIsDescendingChanged += Instance_OnIsDescendingChanged;
            MpClipTileSortViewModel.Instance.OnSortTypeChanged += Instance_OnSortTypeChanged;
        }

        private void Instance_OnSortTypeChanged(object sender, MpContentSortType e) {
            InfoChanged?.Invoke(this, new EventArgs());
        }

        private void Instance_OnIsDescendingChanged(object sender, bool e) {
            InfoChanged?.Invoke(this, new EventArgs());
        }

        private void Instance_OnSearchTextChanged(object sender, string e) {
            InfoChanged?.Invoke(this, new EventArgs());
        }

        private void Instance_OnFilterFlagsChanged(object sender, MpContentFilterType e) {
            InfoChanged?.Invoke(this, new EventArgs());
        }

        private void Instance_OnTagSelectionChanged(object sender, int e) {
            InfoChanged?.Invoke(this, new EventArgs());
        }
    }
}
