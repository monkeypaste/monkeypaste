using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia;
using System.Threading;
using Avalonia.Rendering;

namespace MonkeyPaste.Avalonia {
    public class MpAvDropHostAdorner : MpAvAdornerBase {
        #region Private Variables

        private  MpShape[] _dropShapes;

        #endregion

        #region Statics
        #endregion

        #region Properties

        private bool _isAdornerVisible => _dropShapes != null && _dropShapes.Length > 0;
        #endregion
        #region Constructors

        public MpAvDropHostAdorner(Control uie) : base(uie) {
            IsVisible = false;
        }
        #endregion

        #region Public Methods

        public void DrawDropAdorner(MpShape[] dropShapes) {
            _dropShapes = dropShapes;
            IsVisible = _isAdornerVisible;
            this.InvalidateVisual();
        }
        #endregion

        #region Private Methods
        private void DrawShape(DrawingContext dc, MpShape shape) {
            var fe = AdornedControl as Control;
            if (fe == null) {
                return;
            }
            IBrush brush = shape.FillOctColor.ToAvBrush();
            IPen pen = shape.StrokeOctColor.ToAvPen(shape.StrokeThickness);

            if (shape is MpLine dl) {
                dc.DrawLine(
                    pen,
                    dl.P1.ToAvPoint(),
                    dl.P2.ToAvPoint());
            } else if (shape is MpEllipse de) {
                dc.DrawEllipse(
                    brush,
                    pen,
                    de.Center.ToAvPoint(),
                    de.Size.Width / 2,
                    de.Size.Height / 2);
            } else if (shape is MpRect dr) {
                dc.DrawRectangle(
                    brush,
                    pen,
                    dr.ToAvRect());
            }
        }

        #endregion

        #region Overrides
        public override void Render(DrawingContext dc) {
            //if(_dropShapes == null || _dropShapes.Length == 0) {
            //    IsVisible = false;
            //    return;
            //}
            //IsVisible = true;
            _dropShapes.ForEach(x => DrawShape(dc, x));
            base.Render(dc);
        }

       
        #endregion
    }
}
