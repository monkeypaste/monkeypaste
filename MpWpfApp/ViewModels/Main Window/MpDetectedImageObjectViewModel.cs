using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDetectedImageObjectViewModel : MpViewModelBase {
        #region Private Variables

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

        private double _iw = 0;
        public double Iw {
            get {
                return _iw;
            }
            set {
                if(_iw != value) {
                    _iw = value;
                    OnPropertyChanged(nameof(Iw));
                    OnPropertyChanged(nameof(WidthRatio));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private double _ih = 0;
        public double Ih {
            get {
                return _ih;
            }
            set {
                if (_ih != value) {
                    _ih = value;
                    OnPropertyChanged(nameof(Ih));
                    OnPropertyChanged(nameof(HeightRatio));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        private double _cw = 0;
        public double Cw {
            get {
                return _cw;
            }
            set {
                if (_cw != value) {
                    _cw = value;
                    OnPropertyChanged(nameof(Cw));
                    OnPropertyChanged(nameof(WidthRatio));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private double _ch = 0;
        public double Ch {
            get {
                return _ch;
            }
            set {
                if (_ch != value) {
                    _ch = value;
                    OnPropertyChanged(nameof(Ch));
                    OnPropertyChanged(nameof(HeightRatio));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        //private double _tw = 0;
        //public double Tw {
        //    get {
        //        return _tw;
        //    }
        //    set {
        //        if (_tw != value) {
        //            _tw = value;
        //            OnPropertyChanged(nameof(Tw));
        //            OnPropertyChanged(nameof(WidthRatio));
        //            OnPropertyChanged(nameof(Width));
        //            OnPropertyChanged(nameof(X));
        //        }
        //    }
        //}

        //private double _th = 0;
        //public double Th {
        //    get {
        //        return _th;
        //    }
        //    set {
        //        if (_th != value) {
        //            _th = value;
        //            OnPropertyChanged(nameof(Th));
        //            OnPropertyChanged(nameof(HeightRatio));
        //            OnPropertyChanged(nameof(Height));
        //            OnPropertyChanged(nameof(Y));
        //        }
        //    }
        //}

        private double _ox = 0;
        public double Ox {
            get {
                return _ox;
            }
            set {
                if (_ox != value) {
                    _ox = value;
                    OnPropertyChanged(nameof(Ox));
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private double _oy = 0;
        public double Oy {
            get {
                return _oy;
            }
            set {
                if (_oy != value) {
                    _oy = value;
                    OnPropertyChanged(nameof(Oy));
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double WidthRatio {
            get {
                return Cw / Iw;
            }
        }

        public double HeightRatio {
            get {
                return Ch / Ih;
            }
        }

        public double X {
            get {
                return (DetectedImageObject.X * WidthRatio) + Ox;
            }
            set {
                if (X != value) {
                    DetectedImageObject.X = (value / WidthRatio) - Ox;
                    OnPropertyChanged(nameof(X));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Y {
            get {
                return (DetectedImageObject.Y * HeightRatio) + Oy;
            }
            set {
                if (Y != value) {
                    DetectedImageObject.Y = (value / HeightRatio) - Oy;
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
            MpDetectedImageObject dio, 
            double iw, double ih, 
            double cw, double ch,
            double ox, double oy) {
            //_iw = iw;
            //_ih = ih;
            //_tw = tw;
            //_th = th;
            //_xratio = tw / iw;
            //_yratio = th / ih;
            //if (_xratio > 1) {
            //    _xratio = 1 / _xratio;
            //}
            //if (_yratio > 1) {
            //    _yratio = 1 / _yratio;
            //}
            //_xoffset = tw - iw;
            //_yoffset = th - ih;
            Iw = iw; 
            Ih = ih;
            Cw = cw;
            Ch = ch;
            Ox = ox;
            Oy = oy;
            DetectedImageObject = dio;
        }
        #endregion

        #region Commands

        #endregion
    }
}
