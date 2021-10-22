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
    }
}
