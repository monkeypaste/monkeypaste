using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpFileListItemViewModel : MpViewModelBase {
        #region View Models

        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        #endregion

        #region Properties
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
                return (double)MpHelpers.FileListSize(new string[] { ItemUri.LocalPath });
            }
        }

        public bool IsItemDirectory {
            get {
                return MpHelpers.IsPathDirectory(ItemUri.LocalPath);
            }
        }

        #endregion

        #region Public Methods

        public MpFileListItemViewModel(MpClipTileViewModel parent, string path) {
            ClipTileViewModel = parent;
            ItemPath = path;
            ItemUri = new Uri(ItemPath,UriKind.Absolute);
            ItemName = Path.GetFileName(ItemUri.LocalPath);
            Icon = (BitmapSource)MpHelpers.GetIconImage(ItemUri.LocalPath);
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
                System.Diagnostics.Process.Start(e1.Uri.ToString());
            };
            tb.Inlines.Add(hyperLink);
        }

        #endregion
    }
}
