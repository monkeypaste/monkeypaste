using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpRichTextBoxPathOverlayViewModel : MpUndoableViewModelBase<MpRichTextBoxPathOverlayViewModel> {
        #region Private Variables
        private Geometry _pathData = null;
        private Canvas _rtbc = null;
        #endregion

        #region Properties

        #region View Models
        private MpClipTileRichTextBoxViewModel _clipTileRichTextBoxViewModel;
        public MpClipTileRichTextBoxViewModel ClipTileRichTextBoxViewModel {
            get {
                return _clipTileRichTextBoxViewModel;
            }
            set {
                if(_clipTileRichTextBoxViewModel != value) {
                    _clipTileRichTextBoxViewModel = value;
                    OnPropertyChanged(nameof(ClipTileRichTextBoxViewModel));
                }
            }
        }
        #endregion

        #region Layout
        private MpObservableCollection<Point> _points = new MpObservableCollection<Point>();
        public MpObservableCollection<Point> Points {
            get {
                return _points;
            }
            set {
                if(_points != value) {
                    _points = value;
                    OnPropertyChanged(nameof(Points));
                }
            }
        }

        private PathGeometry _pathGeometryData;
        public PathGeometry PathGeometryData {
            get {
                return _pathGeometryData;
            }
            set {
                if(_pathGeometryData != value) {
                    _pathGeometryData = value;
                    OnPropertyChanged(nameof(PathGeometryData));
                }
            }
        }

        private Point _startPoint = new Point();
        public Point StartPoint {
            get {
                return _startPoint;
            }
            set {
                if(_startPoint != value) {
                    _startPoint = value;
                    OnPropertyChanged(nameof(StartPoint));
                }
            }
        }
        #endregion

        #region State
        
        #endregion

        #region Brushes
        public Brush OverlayBackgroundBrush {
            get {
                //if(ClipTileRichTextBoxViewModel == null || 
                //   ClipTileRichTextBoxViewModel.ClipTileViewModel == null) {
                //    return Brushes.Transparent;
                //}
                //if(ClipTileRichTextBoxViewModel.IsSelected) {
                //    return Brushes.Red;
                //}
                //if(IsHovering) {
                //    return Brushes.BlanchedAlmond;
                //}
                return Brushes.Transparent;
            }
        }

        public Brush OverlayBorderBrush {
            get {
                if (ClipTileRichTextBoxViewModel == null ||
                   ClipTileRichTextBoxViewModel.ClipTileViewModel == null) {
                    return Brushes.Transparent;
                }
                if (ClipTileRichTextBoxViewModel.IsHovering) {
                    return Brushes.Blue;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpRichTextBoxPathOverlayViewModel() : this(null) { }

        public MpRichTextBoxPathOverlayViewModel(MpClipTileRichTextBoxViewModel rtbvm) : base() {            
            ClipTileRichTextBoxViewModel = rtbvm;
            ClipTileRichTextBoxViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileRichTextBoxViewModel.IsHovering):
                    case nameof(ClipTileRichTextBoxViewModel.IsDragging):
                    case nameof(ClipTileRichTextBoxViewModel.IsSelected):
                        OnPropertyChanged(nameof(OverlayBackgroundBrush));
                        OnPropertyChanged(nameof(OverlayBorderBrush));
                        break;
                }
            };
        }

        public void RichTextBoxPathOverlayPath_Loaded(object sender, RoutedEventArgs args) {
            var overlayPath = (Path)sender;
            _pathData = overlayPath.Data;
            _rtbc = overlayPath.GetVisualAncestor<Canvas>();

            #region Drag & Drop
            overlayPath.PreviewMouseDown += ClipTileRichTextBoxViewModel.RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_PreviewMouseDown;
            overlayPath.PreviewMouseMove += ClipTileRichTextBoxViewModel.RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_PreviewMouseMove;
            overlayPath.GiveFeedback += ClipTileRichTextBoxViewModel.RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_GiveFeedback;
            overlayPath.Drop += ClipTileRichTextBoxViewModel.RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_Drop;
            #endregion

            UpdatePoints(ClipTileRichTextBoxViewModel.Rtb);
        }

        public void UpdatePoints(RichTextBox rtb) {
            {
                /*            
                      TLST-------------------TRST(TR)    Note: TRST & TRSB = TR (left-to-right text)
                TL----TLSB                   TRSB              BRST & BRSB = BR (left-to-right text)
                |                               |              
                |                               |              TLST & TLSB = TL (right-to-left text)
                |                               |              BLET & BLEB = BL (right-to-left text)
                |                               |
                |                 BLET-------BRST(BR)
                BL----------------BLEB       BRSB
                */
                // ignore right-to-left language considerations for now
                // TODO Account for edge case of:
                // 1. Head of list (only has TL & TR)
                // 2. Tail of list (only haw BL & BR)
            }

            var contentStartRect = rtb.Document.ContentStart.GetCharacterRect(LogicalDirection.Forward);
            var contentEndRect = rtb.Document.ContentEnd.GetCharacterRect(LogicalDirection.Forward);
            bool isSingleLine = ClipTileRichTextBoxViewModel.CopyItem.LineCount == 1;

            #region Define Points
            var tlsb = contentStartRect.BottomLeft;
            var tlst = contentStartRect.TopLeft;
            var bleb = contentEndRect.BottomRight;
            var blet = contentEndRect.TopRight;
            var tr = new Point(Canvas.GetLeft(rtb) + rtb.ActualWidth, Canvas.GetTop(rtb));
            var br = new Point(Canvas.GetLeft(rtb) + rtb.ActualWidth, blet.Y);
            var bl = new Point(Canvas.GetLeft(rtb), bleb.Y);
            var tl = new Point(Canvas.GetLeft(rtb), tlsb.Y);
            StartPoint = tlst;
            Points = new MpObservableCollection<Point>();
            if(isSingleLine) {
                Points.Add(tlst);
                Points.Add(blet);
                Points.Add(bleb);
                Points.Add(tlsb);
                Points.Add(tlst);
            } else {
                Points.Add(tlst);
                Points.Add(tr);
                Points.Add(br);
                Points.Add(blet);
                Points.Add(bleb);
                Points.Add(bl);
                Points.Add(tl);
                Points.Add(tlsb);
                Points.Add(tlst);
            }

            //Points.Clear();
            //StartPoint = tlst;
            //Points.Add(tr);
            //Points.Add(new Point(tr.X,tlsb.Y));
            //Points.Add(tlsb);
            //Points.Add(tlst);
            OnPropertyChanged(nameof(Points));
            #endregion

            //#region Define Segments
            ////var pathFigure = new PathFigure();
            ////pathFigure.StartPoint = new Point(Canvas.GetLeft(rtb), Canvas.GetTop(rtb));

            //var tlsb = new LineSegment();
            //tlsb.Point = contentStartRect.BottomLeft;

            //var tlst = new LineSegment();
            //tlst.Point = contentStartRect.TopLeft;

            //var bleb = new LineSegment();
            //bleb.Point = contentEndRect.BottomRight;

            //var blet = new LineSegment();
            //blet.Point = contentEndRect.TopRight;

            //var tr = new LineSegment();
            //tr.Point = new Point(Canvas.GetRight(rtb), Canvas.GetTop(rtb));

            //var br = new LineSegment();
            //br.Point = new Point(Canvas.GetRight(rtb), blet.Point.Y);

            //var bl = new LineSegment();
            //bl.Point = new Point(Canvas.GetLeft(rtb), Canvas.GetBottom(rtb));

            //var tl = new LineSegment();
            //tl.Point = new Point(Canvas.GetLeft(rtb), tlsb.Point.Y);
            //#endregion

            //#region Segment Collection
            //var pathSegmentCollection = new PathSegmentCollection();
            //pathSegmentCollection.Add(tlst);
            //pathSegmentCollection.Add(tr);
            //pathSegmentCollection.Add(br);
            //pathSegmentCollection.Add(blet);
            //pathSegmentCollection.Add(bleb);
            //pathSegmentCollection.Add(bl);
            //pathSegmentCollection.Add(tl);
            //pathSegmentCollection.Add(tlsb);

            //Points.Clear();

            //for (int i = pathSegmentCollection.Count - 1; i >= 0; i--) {
            //    Points.Add(((LineSegment)pathSegmentCollection[i]).Point);
            //}
            //#endregion

            //pathFigure.Segments = pathSegmentCollection;

            //var pathFigureCollection = new PathFigureCollection();
            //pathFigureCollection.Add(pathFigure);

            //var pathGeometry = new PathGeometry();
            //pathGeometry.Figures = pathFigureCollection;

            //return pathGeometry;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands

        #endregion
    }
}
