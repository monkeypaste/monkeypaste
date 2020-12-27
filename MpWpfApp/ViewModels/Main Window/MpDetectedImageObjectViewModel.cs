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
                return Math.Max(12, Height / 5);
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

        public double X {
            get {
                return Math.Min(_l, _r);
                //var x = Math.Max(0,Math.Min(_l, _r));
                //if(x + Width > _cw) {
                //    return _cw - Width;
                //}
                //return x;
            }
        }

        public double Y {
            get {
                return Math.Min(_t, _b);
                //var y = Math.Max(0,Math.Min(_t, _b));
                //if(y + Height > _ch) {
                //    return _ch - Height;
                //}
                //return y;
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
        #endregion

        #region Public Methods
        public MpDetectedImageObjectViewModel(
            MpDetectedImageObject dio) {
            DetectedImageObject = dio;
        }

        public void ClipTileImageDetectedObjectCanvas_Loaded(object sender, RoutedEventArgs args) {
            var b = (Border)sender;
            var canvas = b.GetVisualAncestor<Canvas>();
            var containerCanvas = canvas.GetVisualAncestor<Canvas>();
            var vb = (Viewbox)containerCanvas.FindName("ClipTileImageViewbox");
            var img = (Image)containerCanvas.FindName("ClipTileImage");
            var tb = (TextBox)b.Child;
            var ctvm = (MpClipTileViewModel)img.DataContext;

            ContainerVisual child = VisualTreeHelper.GetChild(vb, 0) as ContainerVisual;
            ScaleTransform scale = child.Transform as ScaleTransform;
            _xr = scale.ScaleX;
            _yr = scale.ScaleY;
            Console.WriteLine(string.Format(@"XR:{0} YR:{1}", _xr, _yr));

            _l = DetectedImageObject.X;
            _t = DetectedImageObject.Y;
            _r = DetectedImageObject.X + DetectedImageObject.Width;
            _b = DetectedImageObject.Y + DetectedImageObject.Height;

            //trigger properties from constructor
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));

            Point mouseEnterPosition = new Point(), mouseDownPosition = new Point(), lastMousePosition = new Point();            

            double maxCornerDistance = 10;
            double maxResizeDistance = 15;

            bool isFromLeft=false,isFromTop=false,isFromRight=false,isFromBottom=false;
            bool isFromTopLeft=false,isFromTopRight=false,isFromBottomRight=false,isFromBottomLeft=false;

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
                ctvm.DetectedImageObjectViewModels.Remove(this);
                canvas.Children.Remove(b);                
            };
            b.ContextMenu = new ContextMenu();
            b.ContextMenu.Items.Add(renameMenuItem);
            b.ContextMenu.Items.Add(deleteMenuItem);

            tb.PreviewMouseLeftButtonDown += (s, e) => {
                if(tb.IsReadOnly) {
                    MainWindowViewModel.IsShowingDialog = true;
                    tb.IsReadOnly = false;
                    tb.Focus();
                    tb.SelectAll();
                    tb.Cursor = Cursors.IBeam;
                    tb.Background = Brushes.White;
                    tb.Foreground = Brushes.Black;
                    e.Handled = true;
                }
            };

            tb.PreviewKeyDown += (s, e) => {
                if(e.Key == Key.Enter || e.Key == Key.Escape) {
                    MainWindowViewModel.IsShowingDialog = false;
                    tb.IsReadOnly = true;
                    tb.Cursor = Cursors.Arrow;
                    tb.Background = Brushes.Black;
                    tb.Foreground = Brushes.White;
                    e.Handled = true;
                }
            };

            tb.MouseEnter += (s, e) => {
                if (tb.IsReadOnly) {
                    tb.Foreground = Brushes.Red;
                }
            };

            tb.MouseLeave += (s, e) => {
                if (tb.IsReadOnly) {
                    tb.Foreground = Brushes.White;
                }
            };

            b.MouseEnter += (s, e2) => {
                if (_isMouseDown) {
                    return;
                }
                IsHovering = true;
                mouseEnterPosition = e2.GetPosition(canvas);
            };

            b.MouseLeave += (s, e2) => {
                if (_isMouseDown) {
                    return;
                }
                IsHovering = false;
            };

            MainWindowViewModel.ApplicationHook.MouseDown += (s, e) => {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                    _isMouseDown = true;
                    mouseDownPosition = Mouse.GetPosition(canvas);
                    var hit = VisualTreeHelper.HitTest(canvas, mouseDownPosition)?.VisualHit;
                    _isDragging = hit == b;
                    if(hit != tb) {
                        MainWindowViewModel.IsShowingDialog = false;
                        tb.IsReadOnly = true;
                        tb.Cursor = Cursors.Arrow;
                        tb.Background = Brushes.Black;
                        tb.Foreground = Brushes.White;
                    }
                }
            };

            MainWindowViewModel.ApplicationHook.MouseUp += (s, e) => {
                _isMouseDown = false;
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

            MainWindowViewModel.ApplicationHook.MouseMove += (s, e) => {                
                var p = Mouse.GetPosition(canvas);

                var c = new Point(X + (Width / 2), Y + (Height / 2));
                var tl = new Point(X, Y);
                var tr = new Point(X + Width, Y);
                var br = new Point(X + Width, Y + Height);
                var bl = new Point(X, Y + Height);

                
                if (!_isMouseDown) {
                    isFromLeft = MpHelpers.IsPointInTriangle(p, c, bl, tl);
                    isFromTop = MpHelpers.IsPointInTriangle(p, c, tl, tr);
                    isFromRight = MpHelpers.IsPointInTriangle(p, c, tr, br);
                    isFromBottom = MpHelpers.IsPointInTriangle(p, c, br, bl);

                    isFromTopLeft = MpHelpers.DistanceBetweenPoints(p, tl) <= maxCornerDistance;
                    isFromTopRight = MpHelpers.DistanceBetweenPoints(p, tr) <= maxCornerDistance;
                    isFromBottomRight = MpHelpers.DistanceBetweenPoints(p, br) <= maxCornerDistance;
                    isFromBottomLeft = MpHelpers.DistanceBetweenPoints(p, bl) <= maxCornerDistance;

                    bool isMouseOver = VisualTreeHelper.HitTest(canvas, p)?.VisualHit == b;
                    if (isMouseOver && MpHelpers.DistanceBetweenPoints(p, mouseEnterPosition) <= maxResizeDistance) {
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
                    } else if(isMouseOver) {
                        Application.Current.MainWindow.Cursor = Cursors.SizeAll;
                    } else {
                        Application.Current.MainWindow.Cursor = Cursors.Arrow;
                    }
                } else if(!_isDragging) { 
                    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                } else {
                    switch (Application.Current.MainWindow.Cursor.ToString()) {
                        case "SizeNWSE":
                            if(isFromTopLeft) {
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
                            double k = 1;
                            _l += diff.X * k;
                            _r += diff.X * k;
                            _t += diff.Y * k;
                            _b += diff.Y * k;
                            break;
                    }
                    if(Application.Current.MainWindow.Cursor != Cursors.SizeAll) {
                        _t = Math.Max(_t, 0);
                        _l = Math.Max(_l, 0);
                        _b = Math.Min(_b, canvas.Height);
                        _r = Math.Min(_r, canvas.Width);
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
