using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvCommonExtensions {
        #region Shape Rendering

        public static IPen GetPen(this MpShape shape) {
            IPen pen = new Pen(
                shape.StrokeOctColor.ToAvBrush(),
                shape.StrokeThickness,
                new DashStyle(shape.StrokeDashStyle, shape.StrokeDashOffset),
                shape.StrokeLineCap.ToEnum<PenLineCap>(),
                shape.StrokeLineJoin.ToEnum<PenLineJoin>(),
                shape.StrokeMiterLimit);
            return pen;
        }

        public static void DrawRect(this MpRect rect,DrawingContext dc) {
            IBrush brush = rect.FillOctColor.ToAvBrush();
            IPen pen = rect.GetPen();
            BoxShadows bs = string.IsNullOrEmpty(rect.BoxShadows) ? default : BoxShadows.Parse(rect.BoxShadows);
            dc.DrawRectangle(
                    brush,
                    pen,
                    rect.ToAvRect(),
                    rect.RadiusX,
                    rect.RadiusY,
                    bs);
        }

        public static void DrawLine(this MpLine line, DrawingContext dc) {
            IPen pen = line.GetPen();
            dc.DrawLine(
                    pen,
                    line.P1.ToAvPoint(),
                    line.P2.ToAvPoint());
        }

        public static void DrawEllipse(this MpEllipse ellipse, DrawingContext dc) {
            IBrush brush = ellipse.FillOctColor.ToAvBrush();
            IPen pen = ellipse.GetPen();

            dc.DrawEllipse(
                   brush,
                   pen,
                   ellipse.Center.ToAvPoint(),
                   ellipse.Size.Width / 2,
                   ellipse.Size.Height / 2);
        }

        public static void DrawShape(this MpShape shape, DrawingContext dc) {
            if (shape is MpLine dl) {
                dl.DrawLine(dc);
            } else if (shape is MpEllipse de) {
                de.DrawEllipse(dc);
            } else if (shape is MpRect dr) {
                dr.DrawRect(dc);
            }
        }

        #endregion
    }
}
