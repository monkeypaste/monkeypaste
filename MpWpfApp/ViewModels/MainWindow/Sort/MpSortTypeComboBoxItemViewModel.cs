using System.Windows.Input;

namespace MpWpfApp {
    public class MpSortTypeComboBoxItemViewModel : MpViewModelBase<MpClipTileSortViewModel> {
        #region Properties
        public string Header { get; set; }

        public string SortPath { get; set; }

        public bool IsVisible { get; set; } = true;
        #endregion

        #region Public Methods
        public MpSortTypeComboBoxItemViewModel(
            MpClipTileSortViewModel parent, 
            string header, 
            string sortPath,
            bool isVisible = true) : base(parent) {
            Header = header;
            SortPath = sortPath;
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
