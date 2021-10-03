using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSortTypeComboBoxItemViewModel : MpViewModelBase<MpClipTileSortViewModel> {
        #region Properties
        public string Header { get; set; }

        public MpClipTileSortType SortType { get; set; }

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpSortTypeComboBoxItemViewModel(
            MpClipTileSortViewModel parent, 
            string header,
            MpClipTileSortType sortType,
            bool isVisible = true) : base(parent) {
            Header = header;
            SortType = sortType;
            IsVisible = isVisible;
        }
        public override string ToString() {
            return Header;
        }
        #endregion

        #region Commands
        #endregion

    }
}
