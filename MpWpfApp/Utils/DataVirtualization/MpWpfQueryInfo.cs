using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfQueryInfo : MpQueryInfo {
        public event EventHandler InfoChanged;

        public MpWpfQueryInfo() {
            MpTagTrayViewModel.Instance.PropertyChanged += TagTray_PropertyChanged;
            MpSearchBoxViewModel.Instance.PropertyChanged += SearchBox_PropertyChanged;
            MpClipTileSortViewModel.Instance.PropertyChanged += Sort_PropertyChanged;
        }


        private void Sort_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpClipTileSortViewModel.Instance.IsSortDescending):
                    IsDescending = MpClipTileSortViewModel.Instance.IsSortDescending;
                    InfoChanged?.Invoke(this, new EventArgs());
                    break;
                case nameof(MpClipTileSortViewModel.Instance.SelectedSortType):
                    SortType = MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
                    InfoChanged?.Invoke(this, new EventArgs());
                    break;
            }
        }

        private void SearchBox_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpSearchBoxViewModel.Instance.SearchText):
                    SearchText = MpSearchBoxViewModel.Instance.SearchText;
                    InfoChanged?.Invoke(this, new EventArgs());
                    break;
                case nameof(MpSearchBoxViewModel.Instance.FilterType):
                    FilterFlags = MpSearchBoxViewModel.Instance.FilterType;
                    InfoChanged?.Invoke(this, new EventArgs());
                    break;
            }
        }

        private void TagTray_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(MpTagTrayViewModel.Instance.SelectedTagTile):
                    if(MpTagTrayViewModel.Instance.SelectedTagTile != null) {
                        TagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
                    } else {
                        TagId = 0;
                    }
                    InfoChanged?.Invoke(this, new EventArgs());
                    break;
            }
        }
    }
}
