using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIconViewModel : MpViewModelBase<MpIconCollectionViewModel> {
        #region Properties

        #region Appearance

        public BitmapSource IconBitmapSource { get; set; }

        public BitmapSource IconBorderBitmapSource { get; set; }

        #endregion

        #region Model

        public ObservableCollection<string> PrimaryIconColorList {
            get {
                if (Icon == null) {
                    return new ObservableCollection<string>();
                }
                return new ObservableCollection<string>(Icon.HexColors);
            }
        }

        public int IconId {
            get {
                if(Icon == null) {
                    return 0;
                }
                return Icon.Id;
            }
        }

        public MpIcon Icon { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpIconViewModel() : base(null) { }

        public MpIconViewModel(MpIconCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpIconViewModel_PropertyChanged;
        }
        
        public async Task InitializeAsync(MpIcon i) {
            IsBusy = true;

            Icon = i;

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpIconViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Icon):
                    IconBitmapSource = Icon.IconImage.ImageBase64.ToBitmapSource();
                    IconBorderBitmapSource = Icon.IconBorderImage.ImageBase64.ToBitmapSource();

                    break;
            }
        }

        #endregion
    }
}
