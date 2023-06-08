using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvMovableGridSplitter : GridSplitter {


        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Overrides
        protected override Type StyleKeyOverride => typeof(GridSplitter);
        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public void ApplyDelta(Vector delta) {
            OnDragStarted(new VectorEventArgs());
            OnDragDelta(new VectorEventArgs() { Vector = delta });
            OnDragCompleted(new VectorEventArgs());
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
