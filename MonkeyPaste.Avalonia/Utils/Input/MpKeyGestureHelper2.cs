using Avalonia.Controls;
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

        private List<T> _downs = new List<T>();
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

        public IReadOnlyList<T> Downs =>
            _downs;

        public IReadOnlyList<IReadOnlyList<T>> PeekGesture =>
            _gesture.Union(new[] { _downs }.ToList()).ToList();
        public string GestureString //{ get; private set; } = string.Empty; 
        => Mp.Services.KeyConverter.ConvertKeySequenceToString(_gesture);

        public string PeekGestureString //{ get; private set; } = string.Empty;
        => Mp.Services.KeyConverter.ConvertKeySequenceToString(PeekGesture);

        public string DownString //{ get; private set; } = string.Empty;
        => Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { _downs });

        public int DownCount =>
            _downs.Count;

        public bool HasMatches { get; set; }
        public bool IsSuppressed { get; set; }

        #endregion

        #region Constructors
        public MpKeyGestureHelper2() : this("unknown gesture") { }
        public MpKeyGestureHelper2(string name) {
            _name = name;
        }
        #endregion

        #region Public Methods
        public bool Down(T key) {
            if (_downs.Contains(key)) {
                return false;
            }
            _downs.Add(key);
            return true;
        }
        public void Up(T key) {
            if (!_gesture.Any() ||
                !_gesture.Last().Contains(key)) {
                //prevents remaining releases from gesturing
                _gesture.Add(_downs.ToList());
            }
            _downs.Remove(key);
        }

        public bool HasGesture() {
            return _gesture.Any() || _downs.Any();
        }
        public void Reset() {
            _gesture.Clear();
            //_downs.Clear();
            HasMatches = false;
            IsSuppressed = false;
        }

        public void Clear() {
            Reset();
            _downs.Clear();
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
