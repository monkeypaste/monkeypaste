using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    /// <summary>
    /// This class encapsulates a single undoable property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MpUndoableProperty<T> : MpIUndoRedo {
        #region Member
        private object _oldValue;
        private object _newValue;
        private string _property;
        private T _instance;
        #endregion

        /// <summary>
        /// Initialize a new instance of <see cref="MpUndoableProperty"/>.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="instance">The instance of the property.</param>
        /// <param name="oldValue">The pre-change property.</param>
        /// <param name="newValue">The post-change property.</param>
        public MpUndoableProperty(string property, T instance, object oldValue, object newValue)
            : this(property, instance, oldValue, newValue, property) {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="MpUndoableProperty"/>.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="instance">The instance of the property.</param>
        /// <param name="oldValue">The pre-change property.</param>
        /// <param name="newValue">The post-change property.</param>
        /// <param name="name">The name of the undo operation.</param>
        public MpUndoableProperty(string property, T instance, object oldValue, object newValue, string name)
            : base() {
            _instance = instance;
            _property = property;
            _oldValue = oldValue;
            _newValue = newValue;

            Name = name;

            // Notify the calling application that this should be added to the undo list.
            UndoManager.Add(this);
        }

        /// <summary>
        /// The property name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Undo the property change.
        /// </summary>
        public void Undo() {
            _instance.GetType().GetProperty(_property).SetValue(_instance, _oldValue, null);
        }

        public void Redo() {
            _instance.GetType().GetProperty(_property).SetValue(_instance, _newValue, null);
        }
    }
}
