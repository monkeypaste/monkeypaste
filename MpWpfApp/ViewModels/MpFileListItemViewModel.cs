﻿using MpWinFormsClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpFileListItemViewModel : MpViewModelBase {
        private string _itemPath = string.Empty;
        public string ItemPath {
            get {
                return _itemPath;
            }
            set {
                if(_itemPath != value) {
                    _itemPath = value;
                    OnPropertyChanged(nameof(ItemPath));
                }
            }
        }

        private BitmapSource _icon = null;
        public BitmapSource Icon {
            get {
                return _icon;
            }
            set {
                if(_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public double ItemBitSize {
            get {
                return (double)MpHelperSingleton.Instance.FileListSize(new string[]{ ItemPath});
            }
        }

        public bool IsItemDirectory {
            get {
                return MpHelperSingleton.Instance.IsPathDirectory(ItemPath);
            }
        }

        public MpFileListItemViewModel(string path) {
            ItemPath = path;
            Icon = (BitmapSource)MpHelperSingleton.Instance.GetIconImage(ItemPath);
        }
    }
}