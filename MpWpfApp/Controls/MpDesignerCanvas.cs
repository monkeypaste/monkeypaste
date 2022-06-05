
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
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
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MpWpfApp {

    public class MpDesignerCanvas : Canvas {
        #region Private Variables

        private DispatcherTimer _timer;

        #endregion

        #region Properties

        #region Appearance

        public Brush TransitionLineDefaultBorderBrush { get; set; } = Brushes.White;
        public Brush TransitionLineHoverBorderBrush { get; set; } = Brushes.Yellow;
        public Brush TransitionLineDisabledFillBrush { get; set; } = Brushes.Red;
        public Brush TransitionLineEnabledFillBrush { get; set; } = Brushes.Lime;

        #endregion

        #region Layout
        public double TransitionLineThickness { get; set; } = 1;
        public double TipWidth { get; set; } = 10;
        public double TipLength { get; set; } = 20;
        public double TailWidth { get; set; } = 5;
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

            var acvm = DataContext as MpActionCollectionViewModel;
            if(acvm == null) {
                return;
            }
            var tavm = acvm.SelectedItem;
            if(tavm == null) {
                return;
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


                var pavm = avm.ParentActionViewModel;
                if (pavm == null) {
                    continue;
                }

                var borderBrush = pavm.IsHovering ? TransitionLineHoverBorderBrush : TransitionLineDefaultBorderBrush;
                var fillBrush = avm.IsEnabled.HasValue && avm.IsEnabled.Value ? //&&
                                //(pavm.ParentActionViewModel == null || (pavm.ParentActionViewModel.IsEnabled.HasValue && pavm.ParentActionViewModel.IsEnabled.Value)) ?
                    TransitionLineEnabledFillBrush : TransitionLineDisabledFillBrush;

                Point head = new Point(pavm.X + (pavm.Width / 2), pavm.Y + (pavm.Height / 2));


                if(pavm is MpMacroActionViewModel) {
                    int i = 0;
                }
                
                DrawArrow(dc, head, tail, avm.Width / 2, borderBrush, fillBrush);
            }
        }

        private void _timer_Tick(object sender, EventArgs e) {
            InvalidateVisual();
        }

        private void DrawArrow(DrawingContext dc, Point startPoint, Point endPoint, double dw, Brush borderBrush, Brush fillBrush) {
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

            // Start with tip of the arrow
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
            dc.DrawGeometry(
                fillBrush,
                new Pen(borderBrush, TransitionLineThickness),
                streamGeometry);
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
