using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
        private MpTag _tag;
        public MpTag Tag {
            get {
                return _tag;
            }
            set {
                if(_tag != value) {
                    _tag = value;
                    OnPropertyChanged("Tag");
                }
            }
        }
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
        public void ToggleSelected() {
            IsSelected = !IsSelected;
        }
        public MpTagTileViewModel(MpTag tag) {
            DisplayName = "MpTagTileViewModel";
            _tag = tag;
        }

        public string TagName {
            get {
                return _tag.TagName;
            }
        }

        public Brush TagColor {
            get {
                return new SolidColorBrush(_tag.TagColor.Color);
            }
        }
        public double TagHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight * 0.85;

            }
        }
        public double TagFontSize {
            get {
                return TagHeight / 1.5;
            }
        }
    }
}
