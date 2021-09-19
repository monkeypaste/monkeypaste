using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpSearchElementViewModel : MpViewModelBase<MpSearchBoxViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        private MpSearchBoxViewModel _searchBoxViewModel = null;
        public MpSearchBoxViewModel SearchBoxViewModel {
            get {
                return _searchBoxViewModel;
            }
            set {
                if(_searchBoxViewModel != value) {
                    _searchBoxViewModel = value;
                    OnPropertyChanged(nameof(SearchBoxViewModel));
                }
            }
        }
        #endregion

        #region Business Logic
        private TextRange _textRange = null;
        public TextRange TextRange {
            get {
                return _textRange;
            }
            set {
                if(_textRange != value) {
                    _textRange = value;
                    OnPropertyChanged(nameof(TextRange));
                }
            }
        }

        private int _sortOrderIdx = 0;
        public int SortOrderIdx {
            get {
                return _sortOrderIdx;
            }
            set {
                if (_sortOrderIdx != value) {
                    _sortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpSearchElementViewModel() : base(null) { }

        public MpSearchElementViewModel(MpSearchBoxViewModel sbvm, TextRange tr) : base(sbvm) {
            SearchBoxViewModel = sbvm;
            TextRange = tr;
        }
        #endregion

        #region Commands

        #endregion
    }
}
