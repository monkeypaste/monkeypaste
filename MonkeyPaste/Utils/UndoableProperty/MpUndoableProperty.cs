namespace MonkeyPaste {
    /// <summary>
    /// This class encapsulates a single undoable property.
    /// </summary>
    public class MpUndoableProperty : MpViewModelBase, MpIUndoRedo {
        #region Member
        private object _oldValue;
        private object _newValue;
        private string _property;
        private object _instance;
        #endregion

        /// <summary>
        /// Initialize a new instance of <see cref="MpUndoableProperty"/>.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="instance">The instance of the property.</param>
        /// <param name="oldValue">The pre-change property.</param>
        /// <param name="newValue">The post-change property.</param>
        public MpUndoableProperty(object instance, string property, object oldValue, object newValue)
            : this(instance, property, oldValue, newValue, property) {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="MpUndoableProperty"/>.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="instance">The instance of the property.</param>
        /// <param name="oldValue">The pre-change property.</param>
        /// <param name="newValue">The post-change property.</param>
        /// <param name="name">The name of the undo operation.</param>
        public MpUndoableProperty(object instance, string property, object oldValue, object newValue, string name)
            : base() {
            _instance = instance;
            _property = property;
            _oldValue = oldValue;
            _newValue = newValue;

            Name = name;

            // Notify the calling application that this should be added to the undo list.
            MpAvUndoManagerViewModel.Instance.Add(this);
        }

        /// <summary>
        /// The property name.
        /// </summary>

        /// <summary>
        /// Undo the property change.
        /// </summary>
        public void Undo() {
            _instance.GetType().GetProperty(_property).SetValue(_instance, _oldValue, null);
        }

        public void Redo() {
            _instance.GetType().GetProperty(_property).SetValue(_instance, _newValue, null);
        }

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public string Name { get; private set; }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
