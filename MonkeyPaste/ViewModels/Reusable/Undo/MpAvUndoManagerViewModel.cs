using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpAvUndoManagerViewModel : MpViewModelBase {
        #region Statics

        private static MpAvUndoManagerViewModel _instance;
        public static MpAvUndoManagerViewModel Instance => _instance ?? (_instance = new MpAvUndoManagerViewModel());
        #endregion

        #region Properties

        #region View Models
        public static ObservableCollection<MpIUndoRedo> UndoList { get; set; } = new ObservableCollection<MpIUndoRedo>();

        public static ObservableCollection<MpIUndoRedo> RedoList { get; set; } = new ObservableCollection<MpIUndoRedo>();

        #endregion

        #region State

        public int MaximumUndoLimit {
            get {
                return MpPrefViewModel.Instance.MaxUndoLimit; ;
            }
            set {
                if (MaximumUndoLimit != value) {
                    MpPrefViewModel.Instance.MaxUndoLimit = Math.Max(0, value);
                    OnPropertyChanged(nameof(MaximumUndoLimit));
                    TrimUndoList();
                }
            }
        }

        public bool CanUndo {
            get {
                return !IsUndoRedoSuppressed && UndoList.Count > 0;
            }
        }
        public bool CanRedo {
            get {
                return !IsUndoRedoSuppressed && RedoList.Count > 0;
            }
        }

        public bool IsUndoRedoSuppressed {
            get {
                return Mp.Services.FocusMonitor.IsSelfManagedHistoryControlFocused;
            }
        }
        #endregion

        #endregion

        #region Constructors

        private MpAvUndoManagerViewModel() : base(null) { }

        #endregion

        #region Public Methods
        public void Add<T>(T instance) where T : MpIUndoRedo {
            if (instance == null)
                throw new ArgumentNullException("instance");

            UndoList.Add(instance);
            RedoList.Clear();

            // Ensure that the undo list does not exceed the maximum size.
            TrimUndoList();
        }

        public void ClearAll() {
            UndoList.Clear();
            RedoList.Clear();
        }

        public void Undo() {
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
                foreach (var ui in copyUndoList) {
                    UndoList.Add(ui);
                }
            }
        }
        public void Redo() {
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

        #endregion

        #region Private Methods

        private void TrimUndoList() {
            while (MaximumUndoLimit < UndoList.Count) {
                UndoList.RemoveAt(0);
            }
        }
        private void UpdateRedoList(List<MpIUndoRedo> redoList) {
            RedoList.Clear();
            foreach (var ri in redoList) {
                RedoList.Add(ri);
            }
        }
        #endregion

        #region Commands

        public ICommand UndoCommand => new MpCommand(
            () => {
                Undo();
            },
            () => {
                return CanUndo;
            });

        public ICommand RedoCommand => new MpCommand(
            () => {
                Redo();
            },
            () => {
                return CanRedo;
            });
        #endregion

    }
}
