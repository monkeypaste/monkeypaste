using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    /// <summary>
    /// The interface describing the Undo/Redo operation.
    /// </summary>
    public interface MpIUndoRedo {
        /// <summary>
        /// The optional name for the Undo/Redo property.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Code to perform the Undo operation.
        /// </summary>
        void Undo();
        /// <summary>
        /// Code to perform the Redo operation.
        /// </summary>
        void Redo();
    }
}
