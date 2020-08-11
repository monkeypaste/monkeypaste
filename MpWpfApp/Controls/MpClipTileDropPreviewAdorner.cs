using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Windows.Foundation;

namespace MpWpfApp {
    public class MpClipTileDropPreviewAdorner : Adorner {
        private Rectangle _child;
        private double _leftOffset, _topOffset;

        public MpClipTileDropPreviewAdorner(UIElement adornedElement) : base(adornedElement) {
            var brush = new VisualBrush(adornedElement);

            _child = new Rectangle();
            _child.Width = adornedElement.RenderSize.Width;
            _child.Height = adornedElement.RenderSize.Height;
            _child.Fill = brush;
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint) {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize) {
            _child.Arrange(new System.Windows.Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index) {
            return _child;
        }

        protected override int VisualChildrenCount => 1;

        public double LeftOffset {
            get {
                return _leftOffset;
            }
            set {
                _leftOffset = value;
                UpdatePosition();
            }
        }

        public double TopOffset {
            get {
                return _topOffset;
            }
            set {
                _topOffset = value;
                UpdatePosition();
            }
        }

        private void UpdatePosition() {
            var adornerLayer = (AdornerLayer)this.Parent;
            if (adornerLayer != null) {
                adornerLayer.Update(AdornedElement);
            }
        }

    }
}
