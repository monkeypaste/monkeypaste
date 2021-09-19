using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpFileListItemViewModel : MpContentItemViewModel {
        #region Properties

        #region View Models
        #endregion

        #region Controls
        public WebBrowser FileWebBrowser;
        #endregion

        private Uri _itemUri = null;
        public Uri ItemUri {
            get {
                return _itemUri;
            }
            set {
                if (_itemUri != value) {
                    _itemUri = value;
                    OnPropertyChanged(nameof(ItemUri));
                }
            }
        }

        private string _itemPath = string.Empty;
        public string ItemPath {
            get {
                return _itemPath;
            }
            set {
                if (_itemPath != value) {
                    _itemPath = value;
                    OnPropertyChanged(nameof(ItemPath));
                }
            }
        }

        private string _itemName = string.Empty;
        public string ItemName {
            get {
                return _itemName;
            }
            set {
                if (_itemName != value) {
                    _itemName = value;
                    OnPropertyChanged(nameof(ItemName));
                }
            }
        }

        private BitmapSource _icon = null;
        public BitmapSource Icon {
            get {
                if(_icon == null) {
                    return new BitmapImage();
                }
                return _icon;
            }
            set {
                if (_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public double ItemBitSize {
            get {
                return (double)MpHelpers.Instance.FileListSize(new string[] { ItemUri.LocalPath });
            }
        }

        public bool IsItemDirectory {
            get {
                return MpHelpers.Instance.IsPathDirectory(ItemUri.LocalPath);
            }
        }

        private bool _isSubSelected = false;
        public bool IsSelected {
            get {
                return _isSubSelected;
            }
            set {
                if (_isSubSelected != value) {
                    _isSubSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        #endregion

        #region Public Methods

        public MpFileListItemViewModel(MpFileListItemCollectionViewModel parent,string path) : base(null,null) {
            ItemPath = path;
            ItemUri = new Uri(ItemPath,UriKind.Absolute);
            ItemName = Path.GetFileName(ItemUri.LocalPath);
            Icon = (BitmapSource)MpHelpers.Instance.GetIconImage(ItemUri.LocalPath);
        }

        public void FileListItemTextBlock_Loaded(object sender, RoutedEventArgs e) {
            var tb = (TextBlock)sender;
            tb.Inlines.Clear();
            //tb.Inlines.Add(ItemName);
            Hyperlink hyperLink = new Hyperlink() {
                NavigateUri = ItemUri
            };
            hyperLink.Inlines.Add(ItemName);
            hyperLink.SetBinding(Hyperlink.IsEnabledProperty, "ClipTileViewModel.IsSelected");
            hyperLink.RequestNavigate += (s, e1) => {
                MpHelpers.Instance.OpenUrl(e1.Uri.ToString());
            };
            tb.Inlines.Add(hyperLink);
        }

        public void FileItemWebBrowser_Loaded(object sender, RoutedEventArgs e) {
            FileWebBrowser = (WebBrowser)sender;
            //FileWebBrowser.OpenFile(ItemPath);
        }

        #endregion
    }
}
