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

        public int AppId {
            get {
                return App.AppId;
            }
        }

        public string AppPath {
            get {
                return App.AppPath;
            }
        }

        public string AppName {
            get {
                return App.AppName;
            }
        }

        public bool IsAppRejected {
            get {
                return App.IsAppRejected;
            }
            set {
                if(App.IsAppRejected != value) {
                    App.IsAppRejected = value;
                    App.WriteToDatabase();
                    OnPropertyChanged(nameof(IsAppRejected));
                    OnPropertyChanged(nameof(App));
                }
            }
        }

        public bool IsNew {
            get {
                return App != null && AppId == 0;
            }
        }

        public BitmapSource IconImage {
            get {
                return App.IconImage;
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
                    OnPropertyChanged(nameof(AppId));
                    OnPropertyChanged(nameof(AppPath));
                    OnPropertyChanged(nameof(AppName));
                    OnPropertyChanged(nameof(IsAppRejected));
                    OnPropertyChanged(nameof(IconImage));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpAppViewModel(MpApp app) : base() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(App):
                        //app is null for add row
                        if(App == null) {
                            //AppName = "Add Application";
                            DeleteButtonVisibility = Visibility.Collapsed;
                            AddButtonVisibility = Visibility.Visible;
                        } else {
                            DeleteButtonVisibility = Visibility.Visible;
                            AddButtonVisibility = Visibility.Collapsed;
                        }
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
