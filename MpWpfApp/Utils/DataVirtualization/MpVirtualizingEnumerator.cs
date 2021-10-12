using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpVirtualizingEnumerator<T> : IEnumerator<T> {
        int _currentIndex = -1;
        MpVirtualizingCollection<T> _collection;


        public MpVirtualizingEnumerator(MpVirtualizingCollection<T> collection) {
            //Trace.WriteLine("MpVirtualizingEnumerator:GetEnumerator");
            _collection = collection;
        }

        #region IEnumerator<T> Members

        public T Current {
            get {
                if (_currentIndex < 0)
                    return default(T);
                return _collection[_currentIndex];
            }
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get {
                return Current;
            }
        }

        public bool MoveNext() {
            if (_currentIndex < _collection.Count - 1) {
                _currentIndex++;
                return true;
            } else
                return false;
        }

        public void Reset() {
            _currentIndex = -1;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            _collection = null;
        }

        #endregion

    }
}
