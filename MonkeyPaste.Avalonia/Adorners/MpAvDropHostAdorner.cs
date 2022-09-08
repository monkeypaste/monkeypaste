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
            //IsVisible = _isAdornerVisible;
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
