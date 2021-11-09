using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        IBeam,
        SizeNS
    }

    public class MpMouseViewModel : MpSingletonViewModel<MpMouseViewModel, object> {
        #region Private Variables

        private readonly Cursor _defaultCursor = Cursors.Arrow;
        private readonly Cursor _moveCursor = Cursors.Hand;
        private readonly Cursor _copyCursor = Cursors.Cross;
        private readonly Cursor _invalidCursor = Cursors.No;
        private readonly Cursor _waitingCursor = Cursors.Wait;
        private readonly Cursor _inputCursor = Cursors.IBeam;
        private readonly Cursor _sizeNSCursor = Cursors.SizeNS;

        private int _isBusyCount = 0;

        #endregion

        #region Properties

        #region State

        public bool IsAppBusy => _isBusyCount > 0;

        public MpCursorType CurrentCursor { get; set; } = MpCursorType.Default;

        #endregion

        #endregion

        #region Constructors

        public MpMouseViewModel() : base() { }

        #endregion

        #region Public Methods

        public async Task Init() {
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
                return MpCursorType.IBeam;
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
                case MpCursorType.IBeam:
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
                case MpCursorType.SizeNS:
                    cursor = _sizeNSCursor;
                    break;
                default:
                    cursor = _defaultCursor;
                    break;
            }
            return cursor;
        }

        public void NotifyAppBusy(bool isAppBusy) {
            //this keeps track of notifiers busy status in a list
            //so is busy is not negated when something else is still busy
            _isBusyCount += isAppBusy ? 1 : -1;
            OnPropertyChanged(nameof(IsAppBusy));
        }
        #endregion

        #region Private Methods

        private void MpMouseViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
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

                // Application.Current.MainWindow.ForceCursor = true;
                // Application.Current.MainWindow.Cursor = cursor;
            });
        }
        #endregion
    }
}
