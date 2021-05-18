using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    /// <summary>
    /// This class is responsible for coordinating the 
    /// undo/redo messages from the various view models
    /// in the application. By having a central repository, 
    /// the undo/redo state is managed without the
    /// need for the VMs having to subscribe to any complex hierarchy.
    /// </summary>
    public static class UndoManager {
        #region Properties
        private static MpRangeObservableCollection<MpIUndoRedo> _undoList;
        public static MpRangeObservableCollection<MpIUndoRedo> UndoList {
            get {
                if (_undoList == null)
                    _undoList = new MpRangeObservableCollection<MpIUndoRedo>();
                return _undoList;
            }
            private set {
                _undoList = value;
            }
        }

        private static MpRangeObservableCollection<MpIUndoRedo> _redoList;
        public static MpRangeObservableCollection<MpIUndoRedo> RedoList {
            get {
                if (_redoList == null)
                    _redoList = new MpRangeObservableCollection<MpIUndoRedo>();
                return _redoList;
            }
            private set {
                _redoList = value;
            }
        }

        private static int? _maxLimit;
        public static int? MaximumUndoLimit {
            get {
                return _maxLimit;
            }
            set {
                if (value.HasValue && value.Value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _maxLimit = value;
                TrimUndoList();
            }
        }
        #endregion

        /// <summary>
        /// Add an undoable instance into the Undo list.
        /// </summary>
        /// <typeparam name="T">The type of instance this is.</typeparam>
        /// <param name="instance">The instance this undo item applies to.</param>
        public static void Add<T>(T instance) where T : MpIUndoRedo {
            if (instance == null)
                throw new ArgumentNullException("instance");

            UndoList.Add(instance);
            RedoList.Clear();

            // Ensure that the undo list does not exceed the maximum size.
            TrimUndoList();
        }


        /// <summary>
        /// Ensure that the undo list does not get too big by
        /// checking the size of the collection against the
        /// <see cref="MaximumUndoLimit"/>
        /// </summary>
        private static void TrimUndoList() {
            if (_maxLimit.HasValue && _maxLimit.Value > 0) {
                while (_maxLimit.Value < UndoList.Count) {
                    UndoList.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Actions can only be undone if there are items in the <see cref="UndoList"/>.
        /// </summary>
        public static bool CanUndo {
            get {
                return UndoList.Count > 0;
            }
        }

        /// <summary>
        /// Actions can only be redone if there are items in the <see cref="RedoList"/>.
        /// </summary>
        public static bool CanRedo {
            get {
                return RedoList.Count > 0;
            }
        }

        /// <summary>
        /// Clear all items from the list.
        /// </summary>
        public static void ClearAll() {
            UndoList.Clear();
            RedoList.Clear();
        }

        /// <summary>
        /// Undo the last VM change.
        /// </summary>
        public static void Undo() {
            if (UndoList.Count > 0) {
                // Extract the item from the undo list.
                MpIUndoRedo item = UndoList.Last();
                UndoList.RemoveAt(UndoList.Count - 1);
                List<MpIUndoRedo> copyRedoList = RedoList.ToList();
                copyRedoList.Add(item);
                // We need to copy the undo list here.
                List<MpIUndoRedo> copyUndoList = UndoList.ToList();
                item.Undo();
                // Now repopulate the undo and redo lists.
                UpdateRedoList(copyRedoList);
                UndoList.Clear();
                UndoList.AddRange(copyUndoList);
            }
        }

        /// <summary>
        /// Redo the last undone VM change.
        /// </summary>
        /// <remarks>
        /// Unlike the undo operation, we don't need to copy the undo list out
        /// because we want the item we're redoing being added back to the redo
        /// list.
        /// </remarks>
        public static void Redo() {
            if (RedoList.Count > 0) {
                // Extract the item from the redo list.
                MpIUndoRedo item = RedoList.Last();
                // Now, remove it from the list.
                RedoList.RemoveAt(RedoList.Count - 1);
                // Here we need to copy the redo list out because
                // we will clear the list when the Add is called and
                // the Redo is cleared there.
                List<MpIUndoRedo> redoList = RedoList.ToList();
                // Redo the last operation.
                item.Redo();
                // Now reset the redo list.
                UpdateRedoList(redoList);
            }
        }

        private static void UpdateRedoList(List<MpIUndoRedo> redoList) {
            RedoList.Clear();
            RedoList.AddRange(redoList);
        }
    }
}
