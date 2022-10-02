using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvFilterMenuViewModel : MpViewModelBase {
        #region Statics

        private static MpAvFilterMenuViewModel _instance;
        public static MpAvFilterMenuViewModel Instance => _instance ?? (_instance = new MpAvFilterMenuViewModel());

        #endregion

        #region Properties

        #region Layout

        private double _filterMenuHeight = 40;
        public double FilterMenuHeight {
            get {

                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
                   MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening ||
                   MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    _filterMenuHeight = 40;
                }
                return _filterMenuHeight;
            }
            set {
                if(FilterMenuHeight != value) {
                    _filterMenuHeight = value;
                    OnPropertyChanged(nameof(FilterMenuHeight));
                }
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvFilterMenuViewModel() {
            PropertyChanged += MpAvTitleMenuViewModel_PropertyChanged;
        }

        #endregion

        #region Private Methods

        private void MpAvTitleMenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch(e.PropertyName) {

            //}
        }
        #endregion
    }
}
