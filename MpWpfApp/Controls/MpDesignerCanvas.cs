using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace MpWpfApp {

    public class MpDesignerCanvas : Canvas {
        private DispatcherTimer _timer;
        #region Properties

        public Brush EmptyLineBrush { get; set; } = Brushes.DimGray;
        public double EmptyLineThickness { get; set; } = 2;
        public DashStyle EmptyLineDaskStyle { get; set; } = DashStyles.Dash;

        public Brush TransitionLineBorderBrush { get; set; } = Brushes.White;
        public Brush TransitionLineFillBrush { get; set; } = Brushes.Red;
        public double TransitionLineThickness { get; set; } = 1;

        public double TipWidth { get; set; } = 10;

        public double TipLength { get; set; } = 20;

        public double TailWidth { get; set; } = 5;

        #region Bg Grid

        public Brush GridLineBrush { get; set; } = Brushes.LightBlue;
        public double GridLineThickness { get; set; } = 1;

        public Brush OriginBrush { get; set; } = Brushes.Cyan;
        public double OriginThickness { get; set; } = 3;

        public int GridLineSpacing { get; set; } = 35;

        public bool ShowGrid { get; set; } = true;

        #endregion


        #region Voronoi

        public double MinDist { get; set; } = 1;
        public int SiteCount { get; set; } = 30;

        public Brush VoronoiLineColor { get; set; } = Brushes.Black;
        public double VoronoiLineThickness { get; set; } = 2;
        public Brush VoronoiFillColor1 { get; set; } = Brushes.DarkGray;
        public Brush VoronoiFillColor2 { get; set; } = Brushes.Silver;
        public Brush VoronoiFillColor3 { get; set; } = Brushes.Gray;
        public Brush VoronoiFillColor4 { get; set; } = Brushes.DimGray;

        public bool ShowVoronoi { get; set; } = true;

        #endregion

        #endregion

        public override void EndInit() {
            base.EndInit();

            this.SizeChanged += MpDesignerCanvas_SizeChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += _timer_Tick;

            _timer.Start();
        }

        private void MpDesignerCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
            voroObj = null;
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if(DataContext == null) {
                return;
            }

            var tavm = DataContext as MpTriggerActionViewModelBase;

            if(tavm == null) {
                return;
            }

            if(ShowGrid) {
                DrawGrid(dc);
            }
            if(ShowVoronoi) {
                if(voroObj == null) {
                    VoroniTest(dc);
                }
            }

            var avmc = tavm.FindAllChildren().ToList();

            if(avmc == null) {
                return;
            }
            avmc.Insert(0, tavm);
            foreach (var avm in avmc) {
                Point tail = new Point(avm.X + (avm.Width / 2), avm.Y + (avm.Height / 2));
                Point head;

                var pavm = avm.ParentActionViewModel;
                if (pavm == null || (avm is MpEmptyActionViewModel eavm && !eavm.IsVisible)) {
                    continue;
                } else {
                    head = new Point(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));
                }

                if(avm is MpEmptyActionViewModel) {
                    DrawEmptyLine(dc, head, tail, avm.Width / 2);
                } else {
                    DrawArrow(dc, head, tail, avm.Width / 2);
                }
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void DrawGrid(DrawingContext dc) {
            var zc = this.GetVisualAncestor<ZoomAndPan.ZoomAndPanControl>();
            Point offset = new Point(zc.ContentOffsetX, zc.ContentOffsetY);
            offset.X = MonkeyPaste.MpMathHelpers.WrapValue(offset.X, -GridLineSpacing, GridLineSpacing);
            offset.Y = MonkeyPaste.MpMathHelpers.WrapValue(offset.Y, -GridLineSpacing, GridLineSpacing);

            int HorizontalGridLineCount = (int)(RenderSize.Width / GridLineSpacing);

            double xStep = RenderSize.Width / HorizontalGridLineCount;
            double curX = 0;
            for (int x = 0; x < HorizontalGridLineCount; x++) {
                Point p1 = new Point(curX, 0);
                p1 = (Point)(p1 - offset);
                Point p2 = new Point(curX, RenderSize.Height);
                p2 = (Point)(p2 - offset);

                bool isOrigin = x == (int)(HorizontalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curX += xStep;
            }

            int VerticalGridLineCount = (int)(RenderSize.Height / GridLineSpacing);
            double yStep = RenderSize.Height / VerticalGridLineCount;
            double curY = 0;
            for (int y = 0; y < VerticalGridLineCount; y++) {
                Point p1 = new Point(0, curY);
                p1 = (Point)(p1 - offset);
                Point p2 = new Point(RenderSize.Width, curY);
                p2 = (Point)(p2 - offset);

                bool isOrigin = y == (int)(VerticalGridLineCount / 2);
                if (isOrigin) {
                    dc.DrawLine(new Pen(OriginBrush, OriginThickness), p1, p2);
                } else {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), p1, p2);
                }

                curY += yStep;
            }
        }

        private void DrawEmptyLine(DrawingContext dc, Point startPoint, Point endPoint, double dw) {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * (dw / 2);

            var emptyPen = new Pen(EmptyLineBrush, EmptyLineThickness) { DashStyle = EmptyLineDaskStyle };

            dc.DrawLine(emptyPen, startPoint, endPoint);
        }

        private void DrawArrow(DrawingContext dc, Point startPoint, Point endPoint, double dw) {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            startPoint += normalizedDirection * dw;
            endPoint -= normalizedDirection * dw;

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * TailWidth;

            // Adjust arrow thickness for very thick lines
            Vector arrowWidthVector = normalizedlineWidenVector * TipWidth;

            var pc = new PointCollection(6);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * TipLength);

            //pc.Add(endPoint); // Start with tip of the arrow
            pc.Add(endArrowCenterPosition + arrowWidthVector);
            pc.Add(endArrowCenterPosition + lineWidenVector);
            pc.Add(startPoint + lineWidenVector);
            pc.Add(startPoint - lineWidenVector);
            pc.Add(endArrowCenterPosition - lineWidenVector);
            pc.Add(endArrowCenterPosition - arrowWidthVector);


            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                geometryContext.BeginFigure(endPoint, true, true);
                geometryContext.PolyLineTo(pc, true, true);
            }
            streamGeometry.Freeze();
            dc.DrawGeometry(TransitionLineFillBrush, new Pen(TransitionLineBorderBrush, TransitionLineThickness), streamGeometry);

            //for (int i = 0; i < pc.Count; i++) {
            //    var p1 = pc[i];
            //    var p2 = i == pc.Count - 1 ? pc[0] : pc[i + 1];
            //    dc.DrawLine(new Pen(TransitionLineBorderBrush, TransitionLineThickness), p1,p2);
            //}
        }


        #region Voroni Test
        private Voronoi2.Voronoi voroObj;
        List<Voronoi2.GraphEdge> ge;

        List<Point> sites = new List<Point>();
        private void VoroniTest(DrawingContext dc) {
            if(voroObj == null) {
                voroObj = new Voronoi2.Voronoi(MinDist);

                var rand = MpHelpers.Rand;
                int seed = rand.Next();
                for (int i = 0; i < SiteCount-4; i++) {
                    sites.Add(new Point(rand.NextDouble() * this.Width, rand.NextDouble() * this.Height));
                }
                var thisRect = new Rect(0, 0, this.Width, this.Height);
                sites.Add(thisRect.TopLeft);
                sites.Add(thisRect.TopRight);
                sites.Add(thisRect.BottomRight);
                sites.Add(thisRect.BottomLeft);

                ge = MakeVoronoiGraph(sites, (int)this.Width, (int)this.Height);
            }
            spreadPoints(dc);
        }

        void spreadPoints(DrawingContext dc) {
            for (int i = 0; i < sites.Count; i++) {
                //g.FillEllipse(Brushes.Blue, sites[i].X - 1.5f, sites[i].Y - 1.5f, 3, 3);
                Point center = new Point(sites[i].X - 1.5, sites[i].Y - 1.5);
                dc.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, 1), center, 3, 3);
            }


            var polygons = new List<List<MpPoint>>();
            for (int i = 0; i < sites.Count; i++) {
                var polygon = new List<MpPoint>();
                foreach(var e in ge.Where(x=>x.site1 == i || x.site2 == i)) {
                    polygon.Add(new MpPoint(e.x1, e.y1));
                    polygon.Add(new MpPoint(e.x2, e.y2));
                }
                polygon = polygon.Distinct().ToList();
                polygons.Add(MpGeometryHelpers.GetConvexHull(polygon));
            }

            double minArea = polygons.Min(x => MpGeometryHelpers.GetArea(x));
            double maxArea = polygons.Min(x => MpGeometryHelpers.GetArea(x));
            double areaDiff = maxArea - minArea;

            var fills = new Brush[] {
                VoronoiFillColor1,
                VoronoiFillColor2,
                VoronoiFillColor3,
                VoronoiFillColor4
            };

            foreach(var p in polygons) {
                if(p.Count < 2) {
                    continue;
                }
                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext geometryContext = streamGeometry.Open()) {
                    var pc = new PointCollection(p.Skip(1).Select(x => x.ToPoint()));
                    geometryContext.BeginFigure(p[0].ToPoint(), true, true);
                    geometryContext.PolyLineTo(pc, true, true);
                }
                streamGeometry.Freeze();

                double area = MpGeometryHelpers.GetArea(p);
                int bIdx = GetAreaGroupInterval(area, minArea, fills.Length);
                bIdx = Math.Max(0,Math.Min(bIdx, fills.Length - 1));
                bIdx = MpHelpers.Rand.Next(0, fills.Length - 1);
                dc.DrawGeometry(fills[bIdx], new Pen(VoronoiLineColor, VoronoiLineThickness), streamGeometry);
            }

            for (int i = 0; i < ge.Count; i++) {
                Point p1 = new Point(ge[i].x1, ge[i].y1);
                Point p2 = new Point(ge[i].x2, ge[i].y2);
                dc.DrawLine(new Pen(VoronoiLineColor, VoronoiLineThickness), p1, p2);
            }
        }

        private int GetAreaGroupInterval(double area, double minArea, int intervalSize) {
            return (int)((area - minArea) / intervalSize);
            //var group = (area - minArea) / intervalSize;
            //var startAge = group * intervalSize + minArea;
            //var endAge = startAge + intervalSize - 1;
            //return String.Format("{0}-{1}", startAge, endAge);
        }

        List<Voronoi2.GraphEdge> MakeVoronoiGraph(List<Point> sites, int width, int height) {
            double[] xVal = new double[sites.Count];
            double[] yVal = new double[sites.Count];
            for (int i = 0; i < sites.Count; i++) {
                xVal[i] = sites[i].X;
                yVal[i] = sites[i].Y;
            }
            return voroObj.generateVoronoi(xVal, yVal, 0, width, 0, height);
        }

        #endregion
    }
}
