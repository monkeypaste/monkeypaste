using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
namespace MonkeyPaste {
    public class MpLoaderBalloonViewModel : MpViewModelBase {

        #region Static Variables
        #endregion

        #region Private Variables
        private int _updateCount = 0;
        #endregion

        #region View Models
        #endregion

        #region Properties

        public bool IsLoaded => PercentLoaded >= 1.0;

        public string PostLoadedMessage { get; set; }

        public string Info { get; set; }

        public string LoadingLabel { get; set; }

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth => ProgressTotalBarWidth * PercentLoaded;

        public double PercentLoaded { get; set; }

        public string PercentLabel {
            get {
                int percent = (int)(PercentLoaded * 100);
                return $"{percent} %";
            }
        }

        #endregion

        #region Public Methods
        public MpLoaderBalloonViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }


        #endregion


        #region Private Methods

        private void MpStandardBalloonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(PercentLoaded):
                    if(PercentLoaded > 1.0) {
                        PercentLoaded = 1.0;
                    }
                    
                    OnPropertyChanged(nameof(ProgressBarCurrentWidth));
                    OnPropertyChanged(nameof(PercentLabel));
                    OnPropertyChanged(nameof(IsLoaded));

                    int dotCount = _updateCount % 4;
                    LoadingLabel = "LOADING";
                    for(int i = 0;i < dotCount;i++) {
                        LoadingLabel += ".";
                    }
                    _updateCount++;
                    break;
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}