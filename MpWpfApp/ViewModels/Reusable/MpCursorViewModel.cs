using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public enum MpCursorType {
        None = 0,
        Default,
        OverDragItem,
        ContentMove,
        TileMove,
        ContentCopy,
        TileCopy,
        Invalid,
        Waiting,
        IBeam,
        ResizeNS,
        ResizeWE,
        Link
    }

    public class MpCursorViewModel : MpViewModelBase, MpISingletonViewModel<MpCursorViewModel> {
        #region Private Variables

        private Dictionary<MpCursorType, Cursor> _cursorLookup = 
            new Dictionary<MpCursorType, Cursor>() {
                {MpCursorType.None, Cursors.Arrow },
                {MpCursorType.Default, Cursors.Arrow },
                {MpCursorType.OverDragItem, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandOpenCursor"] as string))  },
                {MpCursorType.ContentMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                {MpCursorType.TileMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                {MpCursorType.ContentCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string)) },
                {MpCursorType.TileCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string))  },
                {MpCursorType.Invalid, Cursors.No },
                {MpCursorType.Waiting, Cursors.Wait },
                {MpCursorType.IBeam, Cursors.IBeam },
                {MpCursorType.ResizeNS, Cursors.SizeNS },
                {MpCursorType.ResizeWE, Cursors.SizeWE },
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

        private static MpCursorViewModel _instance;
        public static MpCursorViewModel Instance => _instance ?? (_instance = new MpCursorViewModel());

        public async Task Init() {
            await Task.Delay(1);
            CurrentCursor = MpCursorType.Default;
        }

        public MpCursorViewModel() : base(null) {
            PropertyChanged += MpMouseViewModel_PropertyChanged;
            OnBusyChanged += MpViewModelBase_OnBusyChanged;
        }


        private void MpViewModelBase_OnBusyChanged(object sender, bool e) {
            NotifyAppBusy(e);
        }


        #endregion

        #region Public Methods


        public MpCursorType GetCursorFromString(string text) {
            if (text.ToLower() == "wait") {
                return MpCursorType.Waiting;
            }
            if (text.ToLower() == "arrow" || text.ToLower() == "default") {
                return MpCursorType.Default;
            }
            if (text.ToLower() == "ibeam") {
                return MpCursorType.IBeam;
            }
            if (text.ToLower() == "invalid") {
                return MpCursorType.Invalid;
            }
            if (text.ToLower() == "hand") {
                return MpCursorType.Link;
            } else {
                throw new Exception("Unknown cursor string: " + text);
            }
        }

        public Cursor GetCurrentCursor() {
            if(CurrentCursor == MpCursorType.OverDragItem ||
               CurrentCursor == MpCursorType.Invalid) {
                //Debugger.Break();
            }
            return _cursorLookup[CurrentCursor];
        }

        //private Dictionary<string, int> _busyCountLookup = new Dictionary<string, int>();

        public void NotifyAppBusy(bool isAppBusy, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            //int delta = isAppBusy ? 1 : -1;
            //if(!_busyCountLookup.ContainsKey(callerName)) {
            //    if(!isAppBusy) {
            //        Debugger.Break();
            //    }
            //    _busyCountLookup.Add(callerName, 1);
            //} else {
            //    _busyCountLookup[callerName] += delta;
            //}
            //this keeps track of notifiers busy status in a list
            //so is busy is not negated when something else is still busy
            _isBusyCount += isAppBusy ? 1 : -1;
            MpConsole.WriteLine($"IsBusy: {_isBusyCount}");
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
            if(Application.Current.Dispatcher.CheckAccess()) {
                Cursor cursor = GetCurrentCursor();

                Mouse.OverrideCursor = cursor;
                Mouse.PrimaryDevice.OverrideCursor = cursor;

                if(Application.Current.MainWindow == null) {
                    // NOTE occurs on init
                    return;
                }
                Application.Current.MainWindow.ForceCursor = true;
                Application.Current.MainWindow.Cursor = cursor;
            } else {
                MpHelpers.RunOnMainThread(UpdateCursor);
            }
            
        }
        #endregion
    }
}
