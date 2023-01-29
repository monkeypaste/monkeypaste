using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using System.Diagnostics;
using Avalonia.Controls.Shapes;
using Avalonia;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvShapeExtensions {
        public static Shape ToAvShape(this MpShape shape) {
            if(shape == null) {
                return null;
            }
            if(shape is MpEllipse ellipse) {
                return new Ellipse() {
                    Width = ellipse.Size.Width,
                    Height = ellipse.Size.Height
                };
            } 
            var p = new Polygon();
            p.Points = shape.Points.Select(x => x.ToAvPoint()).ToList();
            return p;
        }
    }
}
