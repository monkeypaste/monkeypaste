using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvDropHostAdorner : MpAvAdornerBase {
        #region Private Variables

        private MpShape[] _dropShapes;

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
            //IsTileOnScreen = _isAdornerVisible;
            this.InvalidateVisual();
        }
        #endregion

        #region Private Methods
        #endregion

        #region Overrides
        public override void Render(DrawingContext dc) {
            if (_dropShapes != null) {
                _dropShapes.ForEach(x => x.DrawShape(dc));
            }
            base.Render(dc);
        }


        #endregion
    }
}
