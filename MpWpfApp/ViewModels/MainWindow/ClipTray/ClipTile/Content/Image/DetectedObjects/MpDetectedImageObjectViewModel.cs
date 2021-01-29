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
        private double _l = 0, _r = 0, _t = 0, _b = 0;
        private double _xr = 1, _yr = 1;
        private bool _isMouseDown = false;
        private bool _isDragging = false;
        private bool _isDrawing = false;
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
        
        public double FontSize {
            get {
                return Math.Max(8,Math.Min(Width,Height) / 5);
            }
        }

        public double X {
            get {
                return Math.Min(_l, _r);
            }
        }

        public double Y {
            get {
                return Math.Min(_t, _b);
            }
        }

        public double Width {
            get {
                double al = Math.Abs(_l);
                double ar = Math.Abs(_r);
                return Math.Max(ar, al) - Math.Min(ar, al);
            }
        }

        public double Height {
            get {
                double at = Math.Abs(_t);
                double ab = Math.Abs(_b);
                return Math.Max(at, ab) - Math.Min(at, ab);
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
                return DetectedImageObject.ObjectTypeName;
            }
            set {
                if (ObjectTypeName != value) {
                    DetectedImageObject.ObjectTypeName = value;
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
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Blue;
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

        private bool _isNameReadOnly = true;
        public bool IsNameReadOnly {
            get {
                return _isNameReadOnly;
            }
            set {
                if (_isNameReadOnly != value) {
                    _isNameReadOnly = value;
                    OnPropertyChanged(nameof(IsNameReadOnly));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpDetectedImageObjectViewModel(MpDetectedImageObject dio, bool isDrawing = false) {
            _isDrawing = isDrawing;
            DetectedImageObject = dio;
        }
        
        public void ClipTileImageDetectedObjectCanvas_Loaded(object sender, RoutedEventArgs args) {
            var b = (Border)sender;
            var itemsControlCanvas = b.GetVisualAncestor<Canvas>();
            var itemsControl = b.GetVisualAncestor<ItemsControl>();
            if(itemsControlCanvas == null) {
                // this is a strange bug where this dio is loading for richtext clips for some reason
                return;
            }
            var containerCanvas = itemsControlCanvas.GetVisualAncestor<Canvas>();
            var vb = itemsControlCanvas.GetVisualAncestor<Viewbox>();
            var vbGrid = vb.GetVisualAncestor<Grid>();
            var img = (Image)containerCanvas.FindName("ClipTileImage");
            var tb = (TextBox)b.Child;
            var ctvm = (MpClipTileViewModel)vbGrid.DataContext;

            _l = DetectedImageObject.X;
            _t = DetectedImageObject.Y;
            _r = DetectedImageObject.X + DetectedImageObject.Width;
            _b = DetectedImageObject.Y + DetectedImageObject.Height;

            //trigger properties from constructor
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(FontSize));

            Canvas.SetZIndex(b, 1);
            
            Point mouseEnterPosition = new Point(), mouseDownPosition = new Point(), lastMousePosition = new Point();  
            
            double maxCornerDistance = 10;
            double maxResizeDistance = 15;

            bool isMouseOverBorder = false;
            bool isFromLeft=false,isFromTop=false,isFromRight=false,isFromBottom=false;
            bool isFromTopLeft=false,isFromTopRight=false,isFromBottomRight=false,isFromBottomLeft=false;

            if (_isDrawing) {
                mouseEnterPosition = lastMousePosition = mouseDownPosition = Mouse.GetPosition(containerCanvas);
                _isMouseDown = true;
                _isDragging = true;
                isMouseOverBorder = true;
                isFromTopLeft = true;
                Mouse.Capture(vbGrid);
                Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
            }

            vbGrid.MouseMove += (s, e) => {
                if (!ctvm.IsSelected/* && !ctvm.IsHovering*/) {
                    //Mouse.Capture(null);
                    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                    return;
                }

                var p = Mouse.GetPosition(itemsControlCanvas);

                var c = new Point(X + (Width / 2), Y + (Height / 2));
                var tl = new Point(X, Y);
                var tr = new Point(X + Width, Y);
                var br = new Point(X + Width, Y + Height);
                var bl = new Point(X, Y + Height);

                isMouseOverBorder = VisualTreeHelper.HitTest(itemsControlCanvas, p)?.VisualHit == b;

                if (!_isMouseDown) {
                    isFromLeft = MpHelpers.Instance.IsPointInTriangle(p, c, bl, tl);
                    isFromTop = MpHelpers.Instance.IsPointInTriangle(p, c, tl, tr);
                    isFromRight = MpHelpers.Instance.IsPointInTriangle(p, c, tr, br);
                    isFromBottom = MpHelpers.Instance.IsPointInTriangle(p, c, br, bl);

                    isFromTopLeft = MpHelpers.Instance.DistanceBetweenPoints(p, tl) <= maxCornerDistance;
                    isFromTopRight = MpHelpers.Instance.DistanceBetweenPoints(p, tr) <= maxCornerDistance;
                    isFromBottomRight = MpHelpers.Instance.DistanceBetweenPoints(p, br) <= maxCornerDistance;
                    isFromBottomLeft = MpHelpers.Instance.DistanceBetweenPoints(p, bl) <= maxCornerDistance;

                    if (isMouseOverBorder && MpHelpers.Instance.DistanceBetweenPoints(p, mouseEnterPosition) <= maxResizeDistance) {
                        if (isFromTopLeft || isFromBottomRight) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNWSE;
                        } else if (isFromTopRight || isFromBottomLeft) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNESW;
                        } else if (isFromLeft || isFromRight) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeWE;
                        } else if (isFromTop || isFromBottom) {
                            Application.Current.MainWindow.Cursor = Cursors.SizeNS;
                        } else {
                            Application.Current.MainWindow.Cursor = Cursors.Arrow;
                        }
                    } else if (isMouseOverBorder) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeAll;
                    } else {
                        //Application.Current.MainWindow.Cursor = Cursors.Arrow;
                    }
                } else if (!_isDragging) {
                    //Application.Current.MainWindow.Cursor = Cursors.Arrow;
                } else {
                    switch (Application.Current.MainWindow.Cursor.ToString()) {
                        case "SizeNWSE":
                            if (isFromTopLeft) {
                                _l = p.X;
                                _t = p.Y;
                            } else {
                                _r = p.X;
                                _b = p.Y;
                            }
                            break;
                        case "SizeNESW":
                            if (isFromTopRight) {
                                _r = p.X;
                                _t = p.Y;
                            } else {
                                _l = p.X;
                                _b = p.Y;
                            }
                            break;
                        case "SizeWE":
                            if (isFromLeft) {
                                _l = p.X;
                            } else {
                                _r = p.X;
                            }
                            break;
                        case "SizeNS":
                            if (isFromTop) {
                                _t = p.Y;
                            } else {
                                _b = p.Y;
                            }
                            break;
                        case "SizeAll":
                            var diff = new Point(p.X - lastMousePosition.X, p.Y - lastMousePosition.Y);
                            _l += diff.X;
                            _r += diff.X;
                            _t += diff.Y;
                            _b += diff.Y;
                            break;
                    }
                    if(_t < 0) {
                        _b -= _t;
                        _t = 0;
                    }
                    if(_b > itemsControlCanvas.Height) {
                        _t -= _b - itemsControlCanvas.Height; 
                        _b = itemsControlCanvas.Height;
                    }
                    if(_l < 0) {
                        _r -= _l;
                        _l = 0;
                    }
                    if(_r > itemsControlCanvas.Width) {
                        _l -= _r - itemsControlCanvas.Width;
                        _r = itemsControlCanvas.Width;
                    }

                    IsHovering = true;

                    OnPropertyChanged(nameof(X));
                    OnPropertyChanged(nameof(Y));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(FontSize));

                    OnPropertyChanged(nameof(BorderBrush));
                }

                lastMousePosition = p;
            };
            
            vbGrid.MouseLeftButtonDown += (s, e) => {
                if(!ctvm.IsSelected || !IsHovering) {
                    return;
                }
                _isMouseDown = true;
                mouseDownPosition = Mouse.GetPosition(itemsControlCanvas);
                var hit = VisualTreeHelper.HitTest(itemsControlCanvas, mouseDownPosition)?.VisualHit;
                _isDragging = hit == b;
                Mouse.Capture(vbGrid);
                if (hit != tb) {
                    MainWindowViewModel.IsShowingDialog = false;
                    tb.IsReadOnly = true;
                    tb.Cursor = Cursors.Arrow;
                    tb.Background = Brushes.Black;
                    tb.Foreground = Brushes.White;
                }
            };

            vbGrid.MouseLeftButtonUp += (s, e) => {
                if (!ctvm.IsSelected) {
                    return;
                }
                _isMouseDown = false;
                isFromLeft = false;
                isFromTop = false;
                isFromRight = false;
                isFromBottom = false;
                isFromTopLeft = false; 
                isFromTopRight = false; 
                isFromBottomRight = false; 
                isFromBottomLeft = false;

                Mouse.Capture(null);

                Application.Current.MainWindow.Cursor = Cursors.Arrow;

                WriteModelToDatabase();
            };

            var renameMenuItem = new MenuItem();
            renameMenuItem.Header = "Rename";
            renameMenuItem.PreviewMouseDown += (s4, e4) => {
                e4.Handled = true;
                MainWindowViewModel.IsShowingDialog = true;
                tb.IsReadOnly = false;
                tb.Focus();
                tb.SelectAll();
                tb.Cursor = Cursors.IBeam;
                tb.Background = Brushes.White;
                tb.Foreground = Brushes.Black;
            };
            var deleteMenuItem = new MenuItem();
            deleteMenuItem.Header = "Delete";
            deleteMenuItem.Click += (s4, e4) => {
                ctvm.DetectedImageObjectCollectionViewModel.Remove(this);
                DetectedImageObject.DeleteFromDatabase();
            };
            b.ContextMenu = new ContextMenu();
            b.ContextMenu.Items.Add(renameMenuItem);
            b.ContextMenu.Items.Add(deleteMenuItem);

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsNameReadOnly):
                        if(IsNameReadOnly) {
                            MainWindowViewModel.IsShowingDialog = false;
                            tb.IsReadOnly = true;
                            tb.Cursor = Cursors.Arrow;
                            tb.Background = Brushes.Black;
                            tb.Foreground = Brushes.White;
                            WriteModelToDatabase();
                        } else {
                            MainWindowViewModel.IsShowingDialog = true;
                            tb.IsReadOnly = false;
                            tb.Focus();
                            tb.SelectAll();
                            tb.Cursor = Cursors.IBeam;
                            tb.Background = Brushes.White;
                            tb.Foreground = Brushes.Black;                            
                        }
                        break;
                }
            };

            tb.PreviewMouseLeftButtonDown += (s, e) => {
                if (IsNameReadOnly && ctvm.IsSelected) {
                    IsNameReadOnly = false;
                    e.Handled = true;
                }
            };

            tb.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Enter || e.Key == Key.Escape) {
                    IsNameReadOnly = true;
                    e.Handled = true;
                }
            };

            tb.LostFocus += (s, e) => {
                IsNameReadOnly = true;
            };

            tb.MouseEnter += (s, e) => {
                if (tb.IsReadOnly && ctvm.IsSelected) {
                    tb.Foreground = Brushes.Red;
                }
            };

            tb.MouseLeave += (s, e) => {
                if (tb.IsReadOnly && ctvm.IsSelected) {
                    tb.Foreground = Brushes.White;
                }
            };

            b.MouseEnter += (s, e2) => {
                if (_isMouseDown) {
                    return;
                }
                IsHovering = true;
                mouseEnterPosition = e2.GetPosition(itemsControlCanvas);
            };

            b.MouseLeave += (s, e2) => {
                if (_isMouseDown) {
                    return;
                }
                IsHovering = false;
            };
        }
        #endregion

        #region Private Methods
        private void WriteModelToDatabase() {
            DetectedImageObject.X = X;
            DetectedImageObject.Y = Y;
            DetectedImageObject.Width = Width;
            DetectedImageObject.Height = Height;
            DetectedImageObject.ObjectTypeName = ObjectTypeName;

            DetectedImageObject.WriteToDatabase();
        }
        #endregion

        #region Commands

        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                "X:{0} Y:{1} Width:{2} Height:{3} L,T,R,B:{4},{5},{6},{7} Mouse Down?{8} Dragging?{9}",
                X,Y,Width,Height,_l,_t,_r,_b,_isMouseDown ? 1:0, _isDragging ? 1:0
                );
        }
        #endregion
    }
}
