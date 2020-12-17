using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDetectedImageObjectViewModel : MpViewModelBase {
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

        public double X {
            get {
                return DetectedImageObject.X;
            }
            set {
                if (X != value) {
                    DetectedImageObject.X = value;
                    OnPropertyChanged(nameof(X));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Y {
            get {
                return DetectedImageObject.Y;
            }
            set {
                if (Y != value) {
                    DetectedImageObject.Y = value;
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
                return DetectedImageObject.Width;
            }
            set {
                if (Width != value) {
                    DetectedImageObject.Width = value;
                    OnPropertyChanged(nameof(Width));

                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public double Height {
            get {
                return DetectedImageObject.Height;
            }
            set {
                if (Height != value) {
                    DetectedImageObject.Height = value;
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
        public MpDetectedImageObjectViewModel(MpDetectedImageObject dio) {
            DetectedImageObject = dio;
        }
        #endregion

        #region Commands

        #endregion
    }
}
