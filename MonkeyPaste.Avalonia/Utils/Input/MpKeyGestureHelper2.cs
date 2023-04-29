using DynamicData;
using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpKeyGestureHelper2<T> {
        #region Private Variables
        private string _name;
        private List<List<T>> _gesture = new List<List<T>>();
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        public IReadOnlyList<IReadOnlyList<T>> Gesture =>
            _gesture;

        List<T> Downs =>
            _gesture.LastOrDefault<List<T>>();
        public string GestureString =>
            Mp.Services.KeyConverter.ConvertKeySequenceToString(_gesture);

        public int DownCount { get; private set; }
        #endregion

        #region Constructors
        public MpKeyGestureHelper2() : this("unknown gesture") { }
        public MpKeyGestureHelper2(string name) {
            _name = name;
        }
        #endregion

        #region Public Methods
        public void Down(T key) {
            if (Downs == null) {
                _gesture.Add(new List<T>());
            }

            if (!Downs.Contains(key)) {
                DownCount++;
                // holding key fires down repeatedly
                //MpConsole.WriteLine($"[{_name}] DOWN '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' DCOUNT: {DownCount}");

                Downs.Add(key);
            }

        }
        public void Up(T key) {
            int to_remove = Downs == null ? 0 : Downs.Where(x => x.Equals(key)).Count();
            DownCount -= to_remove;
            //MpConsole.WriteLine($"[{_name}] UP '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' DCOUNT: {DownCount}");
            if (Downs == null) {
                _gesture.Add(new List<T>());
            } else {
                _gesture.Add(new List<T>(Downs.Where(x => !x.Equals(key))));
            }
        }

        public void Reset() {
            _gesture.Clear();
            DownCount = 0;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
