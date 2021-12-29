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
    public class MpUrlViewModel : MpViewModelBase<MpUrlCollectionViewModel> {
        #region Properties

        #region State
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsNew {
            get {
                return Url != null && UrlId == 0;
            }
        }
        #endregion

        #region Visibility
        public Visibility RejectUrlVisibility {
            get {
                return Url == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility AddButtonVisibility {
            get {
                return Url == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region Model
        public int UrlId {
            get {
                if(Url == null) {
                    return 0;
                }
                return Url.Id;
            }
        }

        public string UrlPath {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.UrlPath;
            }
        }

        public string UrlDomainPath {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.UrlDomainPath;
            }
        }

        public bool IsDomainRejected {
            get {
                if (Url == null) {
                    return false;
                }
                return Url.IsDomainRejected;
            }
            set {
                if(Url != null && Url.IsDomainRejected != value) {
                    Url.IsDomainRejected = value;
                    OnPropertyChanged(nameof(IsDomainRejected));
                    OnPropertyChanged(nameof(Url));
                    HasModelChanged = true;
                }
            }
        }

        public bool IsUrlRejected {
            get {
                if (Url == null) {
                    return false;
                }
                return Url.IsUrlRejected;
            }
            set {
                if (Url != null && Url.IsUrlRejected != value) {
                    Url.IsUrlRejected = value;
                    OnPropertyChanged(nameof(IsUrlRejected));
                    OnPropertyChanged(nameof(Url));
                    HasModelChanged = true;
                }
            }
        }

        public string IconImageStr {
            get {
                if (Url == null) {
                    return string.Empty;
                }
                return Url.Icon.IconImage.ImageBase64;
            }
        }

        public ObservableCollection<string> PrimaryIconColorList {
            get {
                if(Url == null || Url.Icon == null) {
                    return new ObservableCollection<string>();
                }
                return new ObservableCollection<string>() { Url.Icon.HexColor1, Url.Icon.HexColor2, Url.Icon.HexColor3, Url.Icon.HexColor4, Url.Icon.HexColor5 };
            }
            //set {
            //    if(Url != null && PrimaryIconColorList != value) {
            //        Url.Icon.PrimaryIconColorList = value;
            //        OnPropertyChanged(nameof(PrimaryIconColorList));
            //    }
            //}
        }

        private MpUrl _Url;
        public MpUrl Url {
            get {
                return _Url;
            }
            set {
                //if (_Url != value) 
                {
                    _Url = value;
                    OnPropertyChanged(nameof(Url));
                    OnPropertyChanged(nameof(UrlId));
                    OnPropertyChanged(nameof(UrlPath));
                    OnPropertyChanged(nameof(UrlDomainPath));
                    OnPropertyChanged(nameof(IsDomainRejected));
                    OnPropertyChanged(nameof(IsUrlRejected));
                    OnPropertyChanged(nameof(IconImageStr));
                    OnPropertyChanged(nameof(RejectUrlVisibility));
                    OnPropertyChanged(nameof(AddButtonVisibility));
                    OnPropertyChanged(nameof(PrimaryIconColorList));
                }

                
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpUrlViewModel() : base(null) { }

        public MpUrlViewModel(MpUrlCollectionViewModel parent, MpUrl url) : base(parent) {
            PropertyChanged += MpUrlViewModel_PropertyChanged;
            IsBusy = true;
            Url = url;
            IsBusy = false;
        }

        private void MpUrlViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsDomainRejected):
                    if(IsBusy || Parent.IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        bool isRejected = await MpUrlCollectionViewModel.Instance.UpdateDomainRejection(this, IsDomainRejected);
                        if(isRejected != Url.IsDomainRejected) {
                            await Url.WriteToDatabaseAsync();
                        }
                    });
                    break;
                case nameof(IsUrlRejected):
                    if (IsBusy || Parent.IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        bool isRejected = await MpUrlCollectionViewModel.Instance.UpdateUrlRejection(this, IsUrlRejected);
                        if (isRejected != Url.IsUrlRejected) {
                            await Url.WriteToDatabaseAsync();
                        }
                    });
                    break;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
