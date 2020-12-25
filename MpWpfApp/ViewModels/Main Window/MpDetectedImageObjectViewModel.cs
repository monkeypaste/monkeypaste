using GongSolutions.Wpf.DragDrop.Utilities;
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
        private Point _mouseEnterPosition, _lastMousePosition;
        private bool _isMouseDown = false;
        private bool _isDragging = false;
        #endregion

        #region Properties
        private MpDetectedImageObject _detectedImageObject = null;
        public MpDetectedImageObject DetectedImageObject {
            get {
                return _detectedImageObject;
            }
            set {
                if(_detectedImageObject != value) {
                    _detectedImageObject = value;
                    
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
            DetectedImageObject = dio;
        }

        public void ClipTileImageDetectedObjectCanvas_Loaded(object sender, RoutedEventArgs args) {
            var b = (Border)sender;
            Console.WriteLine("Border render height: " + b.RenderSize.Height + " actual height: " + Height);
            var canvas = b.GetVisualAncestor<Canvas>();
            var canvasWidth = canvas.ActualWidth;
            var canvasHeight = canvas.ActualHeight;

            
            double maxCornerDistance = 3;
            double maxResizeDistance = 15;

            bool isFromLeft=false,isFromTop=false,isFromRight=false,isFromBottom=false;
            bool isFromTopLeft=false,isFromTopRight=false,isFromBottomRight=false,isFromBottomLeft=false;

            b.MouseEnter += (s, e2) => {
                if(_isMouseDown) {
                    return;
                }
                IsHovering = true;
                var mp = e2.GetPosition(b);
                _mouseEnterPosition = mp;                
            };
            canvas.MouseMove += (s, e2) => {
                //if(!IsHovering) {
                //    return;
                //}

                var mp = e2.GetPosition(b);
                var c = new Point(Width / 2, Height / 2);
                var tl = new Point(0, 0);
                var tr = new Point(Width, 0);
                var br = new Point(Width, Height);
                var bl = new Point(0, Height);

                if (!_isMouseDown) {
                    isFromLeft = MpHelpers.IsPointInTriangle(mp, c, bl, tl);
                    isFromTop = MpHelpers.IsPointInTriangle(mp, c, tl, tr);
                    isFromRight = MpHelpers.IsPointInTriangle(mp, c, tr, br);
                    isFromBottom = MpHelpers.IsPointInTriangle(mp, c, br, bl);

                    isFromTopLeft = MpHelpers.DistanceBetweenPoints(mp, tl) <= maxCornerDistance;
                    isFromTopRight = MpHelpers.DistanceBetweenPoints(mp, tr) <= maxCornerDistance;
                    isFromBottomRight = MpHelpers.DistanceBetweenPoints(mp, br) <= maxCornerDistance;
                    isFromBottomLeft = MpHelpers.DistanceBetweenPoints(mp, bl) <= maxCornerDistance;
                    if (MpHelpers.DistanceBetweenPoints(mp, _mouseEnterPosition) <= maxResizeDistance) {
                        if (isFromTopLeft || isFromBottomRight) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
                        } else if (isFromTopRight || isFromBottomRight) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNESW;
                        } else if (isFromLeft || isFromRight) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeWE;
                        } else if (isFromTop || isFromBottom) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNS;
                        } 
                        //else {
                        //    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                        //}
                    } else {
                        Application.Current.MainWindow.Cursor = Cursors.SizeAll;
                    }
                }
                if (_isMouseDown) {
                    _isDragging = true;
                    //mp = e2.GetPosition(canvas);
                    var diff = new Point(mp.X - _lastMousePosition.X, mp.Y - _lastMousePosition.Y);
                    
                    switch(Application.Current.MainWindow.Cursor.ToString()) {
                        //case "SizeNWSE":
                        //    Y += diff.Y;
                        //    Width += diff.X;
                        //    Height += diff.Y;
                        //    break;
                        //case "SizeNESW":
                        //    X += diff.X;
                        //    Width += diff.X;
                        //    Height += diff.Y;
                        //    break;
                        case "SizeWE":
                            if (isFromRight) {
                                Width -= diff.X;
                            } else {
                                X -= diff.X;
                                Width += diff.X;
                            }
                            break;
                        case "SizeNS":
                            if(isFromTop) {
                                Y += diff.Y;
                                Height -= diff.Y;
                            } else {
                                //Y -= diff.Y;
                                Height += diff.Y;
                            }
                            
                            break;
                        case "SizeAll":
                            X += diff.X;
                            Y += diff.Y;
                            break;
                    }

                    //X = Math.Max(X, 0);
                    //Y = Math.Max(Y, 0);
                    //Width = Math.Min(Width, canvasWidth);
                    //Height = Math.Min(Height, canvasHeight);
                }

                //_lastMousePosition = mp;
                //canvas.UpdateLayout(); 
            };
            b.MouseLeave += (s, e2) => {
                //if(!IsSelected) {
                //    IsHovering = false;

                //    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                //}
            };
            canvas.MouseLeftButtonDown += (s, e3) => {
                //IsSelected = true;
                _isMouseDown = true;
                _isDragging = false;
                _lastMousePosition = e3.GetPosition(b);

            };
            canvas.MouseLeftButtonUp += (s, e3) => {
                //IsSelected = false;
                _isMouseDown = false;
                _isDragging = false;
                isFromLeft = false;
                isFromTop = false;
                isFromRight = false;
                isFromBottom = false;
                isFromTopLeft = false; 
                isFromTopRight = false; 
                isFromBottomRight = false; 
                isFromBottomLeft = false;
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
                //DetectedImageObject.WriteToDatabase();
            };
        }
        #endregion

        #region Commands

        #endregion
    }
}
