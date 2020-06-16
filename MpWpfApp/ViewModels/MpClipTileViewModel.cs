using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
   public class MpClipTileViewModel  : MpViewModelBase {
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if(_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility {
            get {
                return _visibility;
            }
            set {
                if(_visibility != value) {
                    _visibility = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }
        private MpClip _copyItem;
        public MpClip CopyItem {
            get {
                return _copyItem;
            }
            set {
                if(_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged("CopyItem");
                }
            }
        }

        public MpClipTileViewModel(MpClip ci) {
            CopyItem = ci;
        }

        public void ToggleSelected() {
            IsSelected = !IsSelected;
        }
        private double _tileSize = MpMeasurements.Instance.TileSize;
        public double TileSize {
            get {
                return _tileSize;
            }
            set {
                if(_tileSize != value) {
                    _tileSize = value;
                    OnPropertyChanged("TileSize");
                }
            }
        }

        private double _tileTitleHeight = MpMeasurements.Instance.TileTitleHeight;
        public double TileTitleHeight {
            get {
                return _tileTitleHeight;
            }
            set {
                if(_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged("TileTitleHeight");
                }
            }
        }

        private double _tileContentHeight = MpMeasurements.Instance.TileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if(_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged("TileTitleHeight");
                }
            }
        }

        public string Title {
            get {
                return CopyItem.Title;
            } 
            set {
                if(CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        public Brush TitleColor {
            get {
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
        }
        public string Text {
            get {
                return CopyItem.Text;
            }
        }
        public ImageSource Icon {
            get {
                return CopyItem.App.Icon.IconImage;
            }
        }

        public override string ToString() {
            return CopyItem.ToString();
        }
    }
}
