using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpUndoableObservableCollectionViewModel<T,I> : MpObservableCollectionViewModel<I> where I : class {
        private List<MpUndoableProperty<T>> _undoables;

        /// <summary>
        /// Initializes a new instance of <see cref="UndoableViewModelBase"/>
        /// </summary>
        //public MpUndoableObservableCollectionViewModel(int maxItems) : base(maxItems) { }

        
        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="instance">The instance to add the undoable item against.</param>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original value.</param>
        /// <param name="newValue">The updated value.</param>
        protected void AddUndo(T instance, string property, object oldValue, object newValue) {
            AddUndo(instance, property, oldValue, newValue, property);
        }

        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="instance">The instance to add the undoable item against.</param>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original value.</param>
        /// <param name="newValue">The updated value.</param>
        /// <param name="name">The name of the undo operation.</param>
        protected void AddUndo(T instance, string property, object oldValue, object newValue, string name) {
            if (!IsInDesignMode) {
                // Only add an undoable item if we aren't in design time. We don't want to
                // use up valuable resources, and this is a handy little optimisation.
                Undoable.Add(new MpUndoableProperty<T>(property, instance, oldValue, newValue, name));
            }
        }

        /// <summary>
        /// Get the list of undoable/redoable entries for this VM.
        /// </summary>
        protected List<MpUndoableProperty<T>> Undoable {
            get {
                if (_undoables == null)
                    _undoables = new List<MpUndoableProperty<T>>();
                return _undoables;
            }
        }
    }
}
