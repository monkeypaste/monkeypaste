using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpTitlePolygonShape : Shape {
        private Rect _rect;

        protected override Geometry DefiningGeometry {
            get {
                if(_rect.IsEmpty) {
                    return Geometry.Empty;
                }
                return new RectangleGeometry(_rect);
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }
    }
}
