using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvFilterMenuViewModel : 
        MpViewModelBase {
        #region Statics

        private static MpAvFilterMenuViewModel _instance;
        public static MpAvFilterMenuViewModel Instance => _instance ?? (_instance = new MpAvFilterMenuViewModel());

        #endregion

        #region Properties

        #region Layout

        public double FilterMenuHeight => 40;
        public double SearchBoxObservedWidth { get; set; }
        public double ClipTileSortViewWidth { get; set; }
        public double PlayPauseButtonWidth { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpAvFilterMenuViewModel() {
            PropertyChanged += MpAvTitleMenuViewModel_PropertyChanged;
        }

        #endregion

        #region Private Methods

        private void MpAvTitleMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SearchBoxObservedWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;
                case nameof(ClipTileSortViewWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;
                case nameof(PlayPauseButtonWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;

            }
        }
        #endregion
    }
}
