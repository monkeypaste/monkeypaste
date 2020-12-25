using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpDetectedImageObjectViewModel : MpViewModelBase {
        #region Private Variables
        private double _xo, _yo, _xc, _yc;
        private Point _mouseEnterPosition, _lastMousePosition;
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

        public Brush BorderBrush {
            get {
                if (IsSelected) {
                    return Brushes.Blue;
                }
                if (IsHovering) {
                    return Brushes.Red;
                }
                return Brushes.Yellow;
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpDetectedImageObjectViewModel(
            MpDetectedImageObject dio) {
            _xc = 0;
            _yc = 0;
            _xo = 0;
            _yo = 0;
            WidthRatio = 1;
            HeightRatio = 1;
            DetectedImageObject = dio;
        }

        public void ClipTileImageDetectedObjectCanvas_Loaded(object sender, RoutedEventArgs args) {
            var b = (Border)sender;

            var c =new Point(Width / 2, Height / 2);
            var tl = new Point(0, 0);
            var tr = new Point(Width, 0);
            var br = new Point(Width, Height);
            var bl = new Point(0, Height);
            double maxCornerDistance = 3;
            double maxResizeDistance = 15;

            b.MouseEnter += (s, e2) => {
                IsHovering = true;
                var mp = e2.GetPosition(b);
                _lastMousePosition = _mouseEnterPosition = mp;                
            };
            b.MouseMove += (s, e2) => {
                if(!IsHovering) {
                    return;
                }

                var mp = e2.GetPosition(b);
                if(MpHelpers.DistanceBetweenPoints(mp,_mouseEnterPosition) <= maxResizeDistance) {
                    //all CW from centroid
                    bool isFromLeft = MpHelpers.IsPointInTriangle(mp, c, bl, tl);
                    bool isFromTop = MpHelpers.IsPointInTriangle(mp, c, tl, tr);
                    bool isFromRight = MpHelpers.IsPointInTriangle(mp, c, tr, br);
                    bool isFromBottom = MpHelpers.IsPointInTriangle(mp, c, br, bl);

                    bool isFromTopLeft = MpHelpers.DistanceBetweenPoints(mp, tl) <= maxCornerDistance;
                    bool isFromTopRight = MpHelpers.DistanceBetweenPoints(mp, tr) <= maxCornerDistance;
                    bool isFromBottomRight = MpHelpers.DistanceBetweenPoints(mp, br) <= maxCornerDistance;
                    bool isFromBottomLeft = MpHelpers.DistanceBetweenPoints(mp, bl) <= maxCornerDistance;

                    if(isFromTopLeft || isFromBottomRight) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
                    } else if (isFromTopRight || isFromBottomRight) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeNESW;
                    } else if(isFromLeft || isFromRight) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeWE;
                    } else if (isFromTop || isFromBottom) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeNS;
                    } else {
                        Application.Current.MainWindow.Cursor = Cursors.Arrow;
                    }
                } else {
                    Application.Current.MainWindow.Cursor = Cursors.SizeAll;
                }
                
                if(IsSelected) {
                    var diff = new Point(mp.X - _lastMousePosition.X, mp.Y - _lastMousePosition.Y);

                    switch(Application.Current.MainWindow.Cursor.ToString()) {
                        case "SizeNWSE":
                            
                            break;
                        case "SizeNESW":

                            break;
                        case "SizeWE":

                            break;
                        case "SizeNS":

                            break;
                        case "SizeAll":

                            break;
                    }
                }
            };
            b.MouseLeave += (s, e2) => {
                IsHovering = false;

                Application.Current.MainWindow.Cursor = Cursors.Arrow;
            };
            b.MouseLeftButtonDown += (s, e3) => {
                IsSelected = true;
            };
            b.MouseLeftButtonUp += (s, e3) => {
                IsSelected = false;
            };
        }
        #endregion

        #region Commands

        #endregion
    }
}
