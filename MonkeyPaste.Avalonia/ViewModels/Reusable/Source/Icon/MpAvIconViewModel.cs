using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconViewModel :
        MpViewModelBase<MpAvIconCollectionViewModel> {
        #region Properties

        #region Appearance


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
                if (Icon == null) {
                    return 0;
                }
                return Icon.Id;
            }
        }

        public string IconBase64 {
            get {
                if (IconImage == null) {
                    return string.Empty;
                }
                return IconImage.ImageBase64;
            }
        }

        public string IconBorderBase64 {
            get {
                if (IconBorderImage == null) {
                    return string.Empty;
                }
                return IconBorderImage.ImageBase64;
            }
        }

        public MpDbImage IconImage { get; private set; }
        public MpDbImage IconBorderImage { get; private set; }

        public MpIcon Icon { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvIconViewModel() : base(null) { }

        public MpAvIconViewModel(MpAvIconCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpIconViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpIcon i) {
            IsBusy = true;

            Icon = i;
            if (Icon != null && Icon.IconImageId > 0) {
                IconImage = await MpDataModelProvider.GetItemAsync<MpDbImage>(Icon.IconImageId);
                OnPropertyChanged(nameof(IconBase64));
            }
            if (Icon != null && Icon.IconBorderImageId > 0) {
                IconBorderImage = await MpDataModelProvider.GetItemAsync<MpDbImage>(Icon.IconBorderImageId);

            }

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Protected Methods

        #endregion
        #region Private Methods

        private void MpIconViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Icon):
                    //IconBitmapSource = Icon.IconImage.ImageBase64.ToBitmapSource();
                    //IconBorderBitmapSource = Icon.IconBorderImage.ImageBase64.ToBitmapSource();

                    break;
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}
