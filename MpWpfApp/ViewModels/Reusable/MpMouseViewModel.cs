using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public enum MpCursorType {
        None = 0,
        Default,
        Move,
        Copy,
        Invalid,
        Waiting,
        Input
    }

    public class MpMouseViewModel : MpSingletonViewModel<MpMouseViewModel,object> {
        #region Private Variables

        private Cursor _defaultCursor = Cursors.Arrow;
        private Cursor _moveCursor = Cursors.Hand;
        private Cursor _copyCursor = Cursors.Cross;
        private Cursor _invalidCursor = Cursors.No;
        private Cursor _waitingCursor = Cursors.Wait;
        private Cursor _inputCursor = Cursors.IBeam;

        #endregion

        #region Properties

        #region State

        public bool IsAppBusy { get; set; } = false;

        public MpCursorType CurrentCursor { get; set; } = MpCursorType.Default;

        #endregion

        #endregion

        #region Constructors

        public MpMouseViewModel() : base() { }

        #endregion

        #region Public Methods

        public override async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                PropertyChanged += MpMouseViewModel_PropertyChanged;
                CurrentCursor = MpCursorType.Default;
            });
        }

        public MpCursorType GetCursorFromString(string text) {
            if (text.ToLower() == "wait") {
                return MpCursorType.Waiting;
            }
            if (text.ToLower() == "arrow") {
                return MpCursorType.Default;
            }
            if (text.ToLower() == "ibeam") {
                return MpCursorType.Input;
            }
            if (text.ToLower() == "hand") {
                return MpCursorType.Move;
            } else {
                throw new Exception("Unknown cursor string: " + text);
            }
        }

        public Cursor GetCurrentCursor() {
            Cursor cursor;
            switch (CurrentCursor) {
                case MpCursorType.Default:
                    cursor = _defaultCursor;
                    break;
                case MpCursorType.Input:
                    cursor = _inputCursor;
                    break;
                case MpCursorType.Invalid:
                    cursor = _invalidCursor;
                    break;
                case MpCursorType.Move:
                    cursor = _moveCursor;
                    break;
                case MpCursorType.Copy:
                    cursor = _copyCursor;
                    break;
                case MpCursorType.Waiting:
                    cursor = _waitingCursor;
                    break;
                default:
                    cursor = _defaultCursor;
                    break;
            }
            return cursor;
        }

        #endregion

        #region Private Methods

        private void MpMouseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsAppBusy):
                    CurrentCursor = IsAppBusy ? MpCursorType.Waiting : MpCursorType.Default;
                    break;
                case nameof(CurrentCursor):
                    UpdateCursor();
                    break;
            }
        }

        private void UpdateCursor() {
            MpHelpers.Instance.RunOnMainThread(() => {
                Cursor cursor = GetCurrentCursor();
                
                Mouse.OverrideCursor = cursor;
                Mouse.PrimaryDevice.OverrideCursor = cursor;

                Application.Current.MainWindow.ForceCursor = true;
                Application.Current.MainWindow.Cursor = cursor;
            });
        }
        #endregion
    }
}
