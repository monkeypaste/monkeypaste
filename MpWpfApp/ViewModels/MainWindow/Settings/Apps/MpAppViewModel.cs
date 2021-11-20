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
    public class MpAppViewModel : MpViewModelBase<MpAppCollectionViewModel> {
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
                return App != null && AppId == 0;
            }
        }
        #endregion

        #region Visibility
        public Visibility RejectAppVisibility {
            get {
                return App == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility AddButtonVisibility {
            get {
                return App == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region Model
        public int AppId {
            get {
                if(App == null) {
                    return 0;
                }
                return App.Id;
            }
        }

        public string AppPath {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppPath;
            }
        }

        public string AppName {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppName;
            }
        }

        public bool IsAppRejected {
            get {
                if (App == null) {
                    return false;
                }
                return App.IsAppRejected;
            }
            set {
                if(App != null && App.IsAppRejected != value) {
                    App.IsAppRejected = value;
                    OnPropertyChanged(nameof(IsAppRejected));
                    OnPropertyChanged(nameof(App));
                }
            }
        }

        public BitmapSource IconImage {
            get {
                if (App == null) {
                    return new BitmapImage();
                }
                return App.Icon.IconImage.ImageBase64.ToBitmapSource();
            }
        }

        public ObservableCollection<string> PrimaryIconColorList {
            get {
                if(App == null || App.Icon == null) {
                    return new ObservableCollection<string>();
                }
                return new ObservableCollection<string>() { App.Icon.HexColor1, App.Icon.HexColor2, App.Icon.HexColor3, App.Icon.HexColor4, App.Icon.HexColor5 };
            }
            //set {
            //    if(App != null && PrimaryIconColorList != value) {
            //        App.Icon.PrimaryIconColorList = value;
            //        OnPropertyChanged(nameof(PrimaryIconColorList));
            //    }
            //}
        }

        private MpApp _app;
        public MpApp App {
            get {
                return _app;
            }
            set {
                //if (_app != value) 
                {
                    _app = value;
                    OnPropertyChanged(nameof(App));
                    OnPropertyChanged(nameof(AppId));
                    OnPropertyChanged(nameof(AppPath));
                    OnPropertyChanged(nameof(AppName));
                    OnPropertyChanged(nameof(IsAppRejected));
                    OnPropertyChanged(nameof(IconImage));
                    OnPropertyChanged(nameof(RejectAppVisibility));
                    OnPropertyChanged(nameof(AddButtonVisibility));
                    OnPropertyChanged(nameof(PrimaryIconColorList));
                }

                
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpAppViewModel() : base(null) { }

        public MpAppViewModel(MpAppCollectionViewModel parent, MpApp app) : base(parent) {
            PropertyChanged += MpAppViewModel_PropertyChanged;
            App = app;
        }

        private void MpAppViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsAppRejected):
                    Task.Run(async () => {
                        bool isRejected = await MpAppCollectionViewModel.Instance.UpdateRejection(this, IsAppRejected);
                        if(isRejected != App.IsAppRejected) {
                            await App.WriteToDatabaseAsync();
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
