using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpSearchCriteriaItemViewModel : MpViewModelBase<MpSearchDetailViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Business Logic

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
        public MpSearchCriteriaItemViewModel() : base(null) { }

        public MpSearchCriteriaItemViewModel(MpSearchDetailViewModel parent) : base(parent) {
        }
        #endregion

        #region Commands

        #endregion
    }
}
