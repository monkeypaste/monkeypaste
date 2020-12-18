using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpDetectedImageObjectViewModel : MpViewModelBase {
        #region Private Variables
        private double _xo, _yo, _xc, _yc;
        #endregion

        #region Properties
        private MpDetectedImageObject _detectedImageObject = null;
        public MpDetectedImageObject DetectedImageObject {
            get {
                return _detectedImageObject;
            }
            set {
                //if(_detectedImageObject != value) 
                {
                    _detectedImageObject = value;
                    DetectedImageObject.WriteToDatabase();
                    OnPropertyChanged(nameof(DetectedImageObject));

                    OnPropertyChanged(nameof(X));
                    OnPropertyChanged(nameof(Y));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Confidence));
                    OnPropertyChanged(nameof(ObjectTypeName));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private double _widthRatio = 1;
        public double WidthRatio {
            get {
                return _widthRatio;
            }
            set {
                if(_widthRatio != value) {
                    _widthRatio = value;
                    OnPropertyChanged(nameof(WidthRatio));
                }
            }
        }

        private double _heightRatio = 1;
        public double HeightRatio {
            get {
                return _heightRatio;
            }
            set {
                if (_heightRatio != value) {
                    _heightRatio = value;
                    OnPropertyChanged(nameof(HeightRatio));
                }
            }
        }

        public double X {
            get {
                return (DetectedImageObject.X * WidthRatio) + _xo;
            }
            set {
                if (X != value) {
                    DetectedImageObject.X = (value / WidthRatio) - _xo;
                    OnPropertyChanged(nameof(X));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Y {
            get {
                return (DetectedImageObject.Y * HeightRatio) + _yo;
            }
            set {
                if (Y != value) {
                    DetectedImageObject.Y = (value / HeightRatio) - _yo;
                    OnPropertyChanged(nameof(Y));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double FontSize {
            get {
                return 12;
            }
        }

        public double LabelX {
            get {
                return Width / 2;
            }
        }

        public double LabelY {
            get {
                return Height / 2;
            }
        }

        public double Width {
            get {
                return DetectedImageObject.Width * WidthRatio;
            }
            set {
                if (Width != value) {
                    DetectedImageObject.Width = value / WidthRatio;
                    OnPropertyChanged(nameof(Width));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Height {
            get {
                return DetectedImageObject.Height * HeightRatio;
            }
            set {
                if (Height != value) {
                    DetectedImageObject.Height = value / HeightRatio;
                    OnPropertyChanged(nameof(Height));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Confidence {
            get {
                return DetectedImageObject.Confidence;
            }
            set {
                if (Confidence != value) {
                    DetectedImageObject.Confidence = value;
                    OnPropertyChanged(nameof(Confidence));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public string ObjectTypeName {
            get {
                return DetectedImageObject.TypeCsv;
            }
            set {
                if (ObjectTypeName != value) {
                    DetectedImageObject.TypeCsv = value;
                    OnPropertyChanged(nameof(ObjectTypeName));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public int CopyItemId {
            get {
                return DetectedImageObject.CopyItemId;
            }
            set {
                if (CopyItemId != value) {
                    DetectedImageObject.CopyItemId = value;
                    OnPropertyChanged(nameof(CopyItemId));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpDetectedImageObjectViewModel(
            MpDetectedImageObject dio, double xc, double yc) {
            _xc = xc;
            _yc = yc;
            _xo = 0;
            _yo = 0;
            WidthRatio = 1;
            HeightRatio = 1;
            DetectedImageObject = dio;
        }

        public void ClipTileImageDetectedObjectCanvas_Loaded(object sender, RoutedEventArgs args) {
            var bc = (Canvas)sender;
            var dob = (Border)bc.FindName("DetectedObjectBorder");

            Canvas.SetLeft(dob, _xc);
            Canvas.SetTop(dob, _yc);
        }
        #endregion

        #region Commands

        #endregion
    }
}
