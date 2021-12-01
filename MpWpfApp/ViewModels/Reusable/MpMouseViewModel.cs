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
        ContentMove,
        TileMove,
        ContentCopy,
        TileCopy,
        Invalid,
        Waiting,
        IBeam,
        ResizeNS,
        Link
    }

    public class MpMouseViewModel : MpSingletonViewModel<MpMouseViewModel> {
        #region Private Variables

        private Dictionary<MpCursorType, Cursor> _cursorLookup = 
            new Dictionary<MpCursorType, Cursor>() {
                {MpCursorType.None, Cursors.Arrow },
                {MpCursorType.Default, Cursors.Arrow },
                {MpCursorType.ContentMove, Cursors.SizeNS },
                {MpCursorType.TileMove, Cursors.SizeWE },
                {MpCursorType.ContentCopy, Cursors.ScrollNS },
                {MpCursorType.TileCopy, Cursors.ScrollWE },
                {MpCursorType.Invalid, Cursors.No },
                {MpCursorType.Waiting, Cursors.Wait },
                {MpCursorType.IBeam, Cursors.IBeam },
                {MpCursorType.ResizeNS, Cursors.SizeNS },
                {MpCursorType.Link, Cursors.Hand },
            };

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
                return MpCursorType.Link;
            } else {
                throw new Exception("Unknown cursor string: " + text);
            }
        }

        public Cursor GetCurrentCursor() {
            return _cursorLookup[CurrentCursor];
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
            if(MpClipTrayViewModel.Instance.IsScrolling ||
               MpClipTrayViewModel.Instance.IsLoadingMore) {
                return;
            }
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
