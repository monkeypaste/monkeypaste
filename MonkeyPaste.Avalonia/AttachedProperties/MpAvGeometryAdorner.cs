using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvGeometryAdorner : MpAvAdornerBase {
        private IEnumerable<(IBrush, Geometry)> _gl;
        public MpAvGeometryAdorner(Control adornedControl) : base(adornedControl) { }

        public void Clear() {
            IsVisible = false;
            this.Redraw();
        }
        public void DrawGeometry(IEnumerable<(IBrush, Geometry)> gl) {
            _gl = gl;
            IsVisible = _gl != null && _gl.Any();
            this.Redraw();
        }
        public override void Render(DrawingContext context) {
            if (IsVisible) {
                if (AdornedControl is HtmlPanel hp) {
                    //using (context.PushTransform(Matrix.CreateTranslation(-hp.ScrollOffset.X, -hp.ScrollOffset.Y))) {
                    foreach (var g in _gl.Where(x => x.Item2 != null)) {
                        context.DrawGeometry(g.Item1, null, g.Item2);
                    }
                    // }
                } else {
                    _gl.Where(x => x.Item2 != null).ForEach(x => context.DrawGeometry(x.Item1, null, x.Item2));
                }
            }
            base.Render(context);
        }
    }
}
