using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace MpWpfApp {
    public class MpClipTileContentViewModel : MpViewModelBase {
        #region Properties
        public bool IsLoading { get; set; } = false;

        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        private object _contentData = null;
        public object ContentData {
            get {
                return _contentData;
            }
            set {
                if (_contentData != value) {
                    _contentData = value;
                    OnPropertyChanged(nameof(ContentData));
                }
            }
        }

        private ObservableCollection<MpFileListItemViewModel> _fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                return _fileListViewModels;
            }
            set {
                if (_fileListViewModels != value) {
                    _fileListViewModels = value;
                    OnPropertyChanged(nameof(FileListViewModels));
                }
            }
        }
        private double _contentHeight = 0;
        public double ContentHeight {
            get {
                return _contentHeight;
            }
            set {
                if (_contentHeight != value) {
                    _contentHeight = value;
                    OnPropertyChanged(nameof(ContentHeight));
                }
            }
        }

        private double _contentWidth = 0;
        public double ContentWidth {
            get {
                return _contentWidth;
            }
            set {
                if (_contentWidth != value) {
                    _contentWidth = value;
                    OnPropertyChanged(nameof(ContentWidth));
                }
            }
        }

        private Visibility _imgVisibility = Visibility.Visible;
        public Visibility ImgVisibility {
            get {
                return _imgVisibility;
            }
            set {
                if (_imgVisibility != value) {
                    _imgVisibility = value;
                    OnPropertyChanged(nameof(ImgVisibility));
                }
            }
        }

        private Visibility _fileListVisibility = Visibility.Visible;
        public Visibility FileListVisibility {
            get {
                return _fileListVisibility;
            }
            set {
                if (_fileListVisibility != value) {
                    _fileListVisibility = value;
                    OnPropertyChanged(nameof(FileListVisibility));
                }
            }
        }

        private Visibility _rtbVisibility = Visibility.Visible;
        public Visibility RtbVisibility {
            get {
                return _rtbVisibility;
            }
            set {
                if (_rtbVisibility != value) {
                    _rtbVisibility = value;
                    OnPropertyChanged(nameof(RtbVisibility));
                }
            }
        }

        //for drag/drop,export and paste not view
        private string _plainText = string.Empty;
        public string PlainText {
            get {
                if (_plainText == string.Empty) {
                    _plainText = ClipTileViewModel.CopyItem.GetPlainText();
                }
                return _plainText;
            }
            set {
                if (_plainText != value) {
                    _plainText = value;
                    OnPropertyChanged(nameof(PlainText));
                }
            }
        }

        //for drag/drop,export and paste not view
        private string _richText = string.Empty;
        public string RichText {
            get {
                if (_richText == string.Empty) {
                    _richText = ClipTileViewModel.CopyItem.GetRichText();
                }
                return _richText;
            }
            set {
                if (_richText != value) {
                    _richText = value;
                    OnPropertyChanged(nameof(RichText));
                }
            }
        }

        private BitmapSource _bmp = null;
        public BitmapSource Bmp {
            get {
                if (_bmp == null) {
                    _bmp = ClipTileViewModel.CopyItem.GetBitmapSource();
                }
                return _bmp;
            }
            set {
                if (_bmp != value) {
                    _bmp = value;
                    OnPropertyChanged(nameof(Bmp));
                }
            }
        }

        public List<string> FileDropList {
            get {
                return ClipTileViewModel.CopyItem.GetFileList();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }
        //private List<MpFileListItemViewModel> _fileViewModelList = new List<MpFileListItemViewModel>();
        //public List<MpFileListItemViewModel> FileViewModelList {
        //    get {
        //        return _fileViewModelList;
        //    }
        //    set {
        //        if(_fileViewModelList != value) {
        //            _fileViewModelList = value;
        //            OnPropertyChanged(nameof(FileViewModelList));
        //        }
        //    }
        //}
        #endregion

        #region Public Methods
        public MpClipTileContentViewModel(MpCopyItem copyItem, MpClipTileViewModel parent) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SearchText):
                        break;
                }
            };
            IsLoading = true;
            ClipTileViewModel = parent;
            switch (copyItem.CopyItemType) {
                case MpCopyItemType.FileList:
                    FileListVisibility = Visibility.Visible;
                    ImgVisibility = Visibility.Collapsed;
                    RtbVisibility = Visibility.Collapsed;
                    break;
                case MpCopyItemType.Image:
                    FileListVisibility = Visibility.Collapsed;
                    ImgVisibility = Visibility.Visible;
                    RtbVisibility = Visibility.Collapsed;
                    break;
                case MpCopyItemType.RichText:
                    FileListVisibility = Visibility.Collapsed;
                    ImgVisibility = Visibility.Collapsed;
                    RtbVisibility = Visibility.Visible;
                    break;
            }
            // all other properties are handled by children
        }

        public void ContentCanvas_Loaded(object sender, RoutedEventArgs e) {
            switch (ClipTileViewModel.CopyItemType) {
                case MpCopyItemType.FileList:
                    var flb = (ListBox)((Canvas)sender)?.FindName("ClipTileFileListBox");
                    flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

                    foreach (var path in FileDropList) {
                        FileListViewModels.Add(new MpFileListItemViewModel(path));
                    }
                    ContentWidth = Bmp.Width;
                    ContentHeight = Bmp.Height;
                    break;
                case MpCopyItemType.Image:
                    var img = (Image)((Canvas)sender)?.FindName("ClipTileImage");
                    img.ContextMenu = (ContextMenu)img.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
                    //aspect ratio
                    double ar = Bmp.Width / Bmp.Height;
                    if (Bmp.Width >= Bmp.Height) {
                        ContentWidth = ClipTileViewModel.TileBorderSize;
                        ContentHeight = ContentWidth * ar;
                    } else {
                        ContentHeight = ClipTileViewModel.TileContentHeight;
                        ContentWidth = ContentHeight * ar;
                    }
                    MpHelpers.ResizeBitmapSource(Bmp, new Size((int)ContentWidth, (int)ContentHeight));
                    
                    //Canvas.SetLeft(img, (ClipTileViewModel.TileBorderSize / 2) - (ContentWidth / 2));
                    //Canvas.SetTop(img, (ClipTileViewModel.TileContentHeight / 2) - (ContentHeight / 2));
                    break;
                case MpCopyItemType.RichText:
                    var rtb = (MpTokenizedRichTextBox)((Canvas)sender)?.FindName("ClipTileRichTextBox");
                    rtb.ContextMenu = (ContextMenu)rtb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
                    ContentWidth = rtb.RenderSize.Width;
                    ContentHeight = rtb.RenderSize.Height;
                    rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
                    rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;

                    var sortedTokenList = ClipTileViewModel.CopyItem.SubTextTokenList.OrderBy(stt => stt.BlockIdx).ThenBy(stt => stt.StartIdx).ToList();
                    foreach (var sortedToken in sortedTokenList) {
                        rtb.AddSubTextToken(sortedToken);
                    }
                    rtb.SearchText = string.Empty;
                    break;
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}
