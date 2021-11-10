using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSearchDetailViewModel : MpViewModelBase<MpSearchBoxViewModel> {

        #region Properties

        #region View Models

        public ObservableCollection<MpTagTileViewModel> SelectedTagTiles { get; set; } = new ObservableCollection<MpTagTileViewModel>();

        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();
        #endregion

        #endregion
        #region Constructors
        public MpSearchDetailViewModel() : base(null) { }

        public MpSearchDetailViewModel(MpSearchBoxViewModel parent) : base(parent) {

        }
        #endregion
    }
}
