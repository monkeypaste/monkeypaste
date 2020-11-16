using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpAppViewModel : MpViewModelBase {
        #region Properties
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

        private int _appId = 0;
        public int AppId {
            get {
                return _appId;
            }
            set {
                if(_appId != value) {
                    _appId = value;
                    OnPropertyChanged(nameof(AppId));
                }
            }
        }

        private int _iconId = 0;
        public int IconId {
            get {
                return _iconId;
            }
            set {
                if (_iconId != value) {
                    _iconId = value;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        private string _appPath = String.Empty;
        public string AppPath {
            get {
                return _appPath;
            }
            set {
                if (_appPath != value) {
                    _appPath = value;
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        private string _appName = String.Empty;
        public string AppName {
            get {
                return _appName;
            }
            set {
                if (_appName != value) {
                    _appName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        private bool _isAppRejected = false;
        public bool IsAppRejected {
            get {
                return _isAppRejected;
            }
            set {
                if (_isAppRejected != value) {
                    _isAppRejected = value;
                    OnPropertyChanged(nameof(IsAppRejected));
                }
            }
        }

        private MpIcon _icon = null;
        public MpIcon Icon {
            get {
                return _icon;
            }
            set {
                if (_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public bool IsNew {
            get {
                return App != null && App.AppId == 0;
            }
        }

        public BitmapSource IconImage {
            get {
                return Icon?.IconImage;
            }
        }

        private Visibility _deleteButtonVisibility;
        public Visibility DeleteButtonVisibility {
            get {
                return _deleteButtonVisibility;
            }
            set {
                if (_deleteButtonVisibility != value) {
                    _deleteButtonVisibility = value;
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                }
            }
        }

        private Visibility _addButtonVisibility;
        public Visibility AddButtonVisibility {
            get {
                return _addButtonVisibility;
            }
            set {
                if (_addButtonVisibility != value) {
                    _addButtonVisibility = value;
                    OnPropertyChanged(nameof(AddButtonVisibility));
                }
            }
        }

        private MpApp _app;
        public MpApp App {
            get {
                return _app;
            }
            set {
                if (_app != value) {
                    _app = value;
                    OnPropertyChanged(nameof(App));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpAppViewModel(MpApp app) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(App):
                        //app is null for add row
                        if(App == null) {
                            //AppName = "Add Application";
                            DeleteButtonVisibility = Visibility.Collapsed;
                            AddButtonVisibility = Visibility.Visible;
                        } else {
                            AppId = App.AppId;
                            IconId = App.IconId;
                            AppPath = App.AppPath;
                            AppName = App.AppName;
                            IsAppRejected = App.IsAppRejected;
                            Icon = App.Icon;
                            DeleteButtonVisibility = Visibility.Visible;
                            AddButtonVisibility = Visibility.Collapsed;
                        }
                        break;
                    case nameof(AppId):
                        App.AppId = AppId;
                        break;
                    case nameof(IconId):
                        App.IconId = IconId;
                        break;
                    case nameof(AppName):
                        App.AppName = AppName;
                        break;
                    case nameof(AppPath):
                        App.AppPath = AppPath;
                        break;
                    case nameof(IsAppRejected):
                        App.IsAppRejected = IsAppRejected;
                        break;
                    case nameof(Icon):
                        App.Icon = Icon;
                        break;
                }
            };

            App = app;

            //this check since App will not change for empty row
            if(App == null) {
                OnPropertyChanged(nameof(App));
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
