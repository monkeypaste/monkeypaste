using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls.Shapes;
using MonkeyPaste.Common;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTitleView : MpAvUserControl<MpAvClipTileViewModel> {
        private List<Shape> _swirlShapes = new List<Shape>();
        private double _dy = 0;
        private DispatcherTimer _timer;
        private DateTime _animateStartTime;

        private bool _isStopping = false;

        private Dictionary<PathSegment, Point[]> _basePointLookup = new Dictionary<PathSegment, Point[]>();

        public MpAvClipTileTitleView() {
            InitializeComponent();
            var sc = this.FindControl<Canvas>("SwirlCanvas");
            sc.AttachedToVisualTree += Sc_AttachedToVisualTree;
            this.DataContextChanged += MpAvClipTileTitleView_DataContextChanged;
        }

        private void MpAvClipTileTitleView_DataContextChanged(object sender, System.EventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(BindingContext.IsHovering):
                    if(BindingContext.IsHovering) {
                        //StartSwirlAnimation();
                    } else {
                        //StopSwirlAnimation();
                    }
                    break;
            }
        }

        private void StartSwirlAnimation() {
            if(_timer == null) {
                _timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(20)
                };
                _timer.Tick += _timer_Tick;
            }

            _isStopping = false;
            _dy = 0;
            _animateStartTime = DateTime.Now;
            _timer.Start();
        }
        
        private void StopSwirlAnimation() {
            if(_timer == null || _isStopping) {
                return;
            }
            _isStopping = true;
        }

        private void _timer_Tick(object sender, EventArgs e) {
            if(_isStopping) {
                if(_dy > 0) {
                    _dy -= 0.01;
                } else if(_dy < 0) {
                    _dy += 0.01;
                }
                if(Math.Abs(_dy) < 0.05) {
                    _dy = 0;
                }
            } else {
                _dy = Math.Sin(DateTime.Now.Ticks * 0.01) * 10;
            }
            
            AnimateSwirl(_dy);

            if(_isStopping && _dy == 0) {
                _isStopping = false;
                _timer.Stop();
            }
        }

        private void AnimateSwirl(double dy) {
            double cur_dy = dy;
            foreach (var shape in _swirlShapes) {
                if (shape is Path path) {
                    if (path.Data is PathGeometry pg) {
                        var segs = pg.Figures.First().Segments;
                        foreach (var seg in segs) {
                            cur_dy = segs.IndexOf(seg) % 2 == 0 ? dy : -dy;
                            if (seg is BezierSegment bs) {
                                if (_basePointLookup.TryGetValue(bs, out var points)) {
                                    bs.Point1 = new Point(points[0].X, points[0].Y + cur_dy);
                                    bs.Point2 = new Point(points[1].X, points[1].Y + cur_dy);
                                    bs.Point3 = new Point(points[2].X, points[2].Y + cur_dy);
                                }

                            } else if (seg is QuadraticBezierSegment qbs) {
                                if (_basePointLookup.TryGetValue(qbs, out var points)) {
                                    qbs.Point1 = new Point(points[0].X, points[0].Y + cur_dy);
                                    qbs.Point2 = new Point(points[1].X, points[1].Y + cur_dy);
                                }
                            }
                        }
                    }
                }
                cur_dy = dy;
            }
        }
        private void Sc_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is Canvas swirlCanvas) {
                CreateSwirl();     
            }
        }

        private void CreateSwirl() {
            var swirlCanvas = this.FindControl<Canvas>("SwirlCanvas");
            swirlCanvas.Children.Clear();

            var layer0 = new Path() {
                Name = "layer0",
                StrokeThickness = 0,
                Fill = Brushes.Red,
                Data = new PathGeometry() {
                    Figures = new PathFigures() {
                            new PathFigure() {
                                IsClosed = true,
                                StartPoint = new Point(0,22),
                                Segments = new PathSegments() {
                                    new BezierSegment() {
                                        Point1 = new Point(150,-10),
                                        Point2 = new Point(160,85),
                                        Point3 = new Point(270,40)
                                    },
                                    new LineSegment() {
                                        Point = new Point(270,0)
                                    },
                                    new LineSegment() {
                                        Point = new Point(0,0)
                                    }
                                }
                            }
                        }
                }
            };
            swirlCanvas.Children.Add(layer0);

            var layer1 = new Path() {
                Name = "layer1",
                StrokeThickness = 0,
                Fill = Brushes.Green,
                Data = new PathGeometry() {
                    Figures = new PathFigures() {
                            new PathFigure() {
                                IsClosed = true,
                                StartPoint = new Point(0,35),
                                Segments = new PathSegments() {
                                    new BezierSegment() {
                                        Point1 = new Point(40,45),
                                        Point2 = new Point(75,20),
                                        Point3 = new Point(100,20)
                                    },
                                    new BezierSegment() {
                                        Point1 = new Point(185,20),
                                        Point2 = new Point(139,65),
                                        Point3 = new Point(270,40)
                                    },
                                    new LineSegment() {
                                        Point = new Point(270,0)
                                    },
                                    new LineSegment() {
                                        Point = new Point(0,0)
                                    }
                                }
                            }
                        }
                }
            };
            swirlCanvas.Children.Add(layer1);

            var layer2 = new Ellipse() {
                Name = "layer2",
                StrokeThickness = 0,
                Fill = MpColorHelpers.GetRandomHexColor().ToAvBrush(),
                Width = 87,
                Height = 28
            };
            swirlCanvas.Children.Add(layer2);
            Canvas.SetLeft(layer2, 165);
            Canvas.SetTop(layer2, 24);

            var layer3 = new Path() {
                Name = "layer3",
                StrokeThickness = 0,
                Fill = Brushes.Blue,
                Data = new PathGeometry() {
                    Figures = new PathFigures() {
                            new PathFigure() {
                                IsClosed = true,
                                StartPoint = new Point(0,55),
                                Segments = new PathSegments() {
                                    new QuadraticBezierSegment() {
                                        Point1 = new Point(30,10),
                                        Point2 = new Point(270,0)
                                    },
                                    new LineSegment() {
                                        Point = new Point(0,0)
                                    }
                                }
                            }
                        }
                }
            };
            swirlCanvas.Children.Add(layer3);

            _swirlShapes = swirlCanvas.GetVisualDescendants<Shape>().ToList();


            foreach (var shape in _swirlShapes) {
                if (shape is Path path && path.Name != "layer3") {
                    if (path.Data is PathGeometry pg) {
                        foreach (var seg in pg.Figures.First().Segments) {
                            if (seg is BezierSegment bs) {
                                _basePointLookup.AddOrReplace(bs, new Point[] { bs.Point1, bs.Point2, bs.Point3 });
                            } else if (seg is QuadraticBezierSegment qbs) {
                                _basePointLookup.AddOrReplace(qbs, new Point[] { qbs.Point1, qbs.Point2 });
                            }
                        }
                    }
                }
            }

            ColorizeSwirl().FireAndForgetSafeAsync(BindingContext);
        }

        private async Task ColorizeSwirl() {
            BindingContext.IsBusy = true;
            bool HasUserDefinedColor = !string.IsNullOrEmpty(BindingContext.CopyItem.ItemColor);
            int layerCount = _swirlShapes.Count;

            List<string> hexColors = new List<string>();
            List<double> opacities = new List<double>();

            if (BindingContext.IconId > 0 && !HasUserDefinedColor) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == BindingContext.IconId);
                if (ivm == null) {
                    var icon = await MpDb.GetItemAsync<MpIcon>(BindingContext.IconId);
                    hexColors = icon.HexColors;
                } else {
                    hexColors = ivm.PrimaryIconColorList.ToList();
                }
            } else if (HasUserDefinedColor) {
                hexColors = Enumerable.Repeat(BindingContext.CopyItemHexColor, layerCount).ToList();
            } else {
                var tagColors = await MpDataModelProvider.GetTagColorsForCopyItemAsync(BindingContext.CopyItemId);
                tagColors.ForEach(x => hexColors.Insert(0, x));
            }

            if (hexColors.Count == 0) {
                hexColors = Enumerable.Repeat(MpColorHelpers.GetRandomHexColor(), layerCount).ToList();
            }
            hexColors = hexColors.Take(layerCount).ToList();
            opacities = Enumerable.Repeat((double)MpRandom.Rand.Next(40, 120) / 255, layerCount).ToList();

            hexColors.ForEach((x, i) => _swirlShapes[i].Fill = x.AdjustAlpha(opacities[i]).ToAvBrush());

            BindingContext.IsBusy = false;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void SourceIconGrid_PointerPressed(object sender, PointerPressedEventArgs e) {
            var ctv = this.GetVisualAncestor<MpAvClipTileView>();
            var wv = ctv.GetVisualDescendant<WebViewControl.WebView>();
            if(wv != null) {
                wv.ShowDeveloperTools();
                return;
            }
            var cwv = ctv.GetVisualDescendant<MpAvCefNetWebView>();
            if(cwv != null) {
                cwv.ShowDevTools();
            }
        }
    }
}
