using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading;
using Xamarin.Forms;

namespace MonkeyPaste {
    /// <summary>
    /// This class provides the ability to bulk manage data in an observable collection without
    /// worrying about raising the notification changed event after every insert or delete. Change notifications
    /// are only raised when the bulk operation is complete.
    /// </summary>
    public class MpRangeObservableCollection<T> : ObservableCollection<T> {
        #region Members
        private bool _suppressNotifications = false;
        //private Dispatcher _dispatcher;
        #endregion

        #region Public methods
        /// <summary>
        /// Initialize a new instance of <see cref="MpRangeObservableCollection"/>.
        /// </summary>
        public MpRangeObservableCollection() : base() {
            //_dispatcher = Dispatcher.CurrentDispatcher;
        }
        /// <summary>
        /// Add multiple items into the collection.
        /// </summary>
        /// <param name="list">The list of items that need to be added in.</param>
        public void AddRange(IEnumerable<T> list) {
            if (list == null)
                throw new ArgumentNullException("list");
            _suppressNotifications = true;
            foreach (T item in list) {
                Add(item);
            }
            _suppressNotifications = false;
            if (list.Count() > 0) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Remove multiple items from the collection.
        /// </summary>
        /// <param name="list">The list of items that need to be added in.</param>
        public void Remove(IEnumerable<T> list) {
            if (list == null)
                throw new ArgumentNullException("list");
            _suppressNotifications = true;
            foreach (T item in list) {
                Remove(item);
            }
            _suppressNotifications = false;
            if (list.Count() > 0) {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Raises the <see cref="System.Collections.ObjectModel.OnCollectionChanged"/> event with the relevant parameters.
        /// </summary>
        /// <param name="e">The parameter to raise</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (!_suppressNotifications) {
                Device.BeginInvokeOnMainThread(() => {
                    base.OnCollectionChanged(e);
                });
                //if (_dispatcher != null && _dispatcher.CheckAccess()) {
                //    base.OnCollectionChanged(e);
                //} else {
                //    _dispatcher.Invoke(DispatcherPriority.DataBind, (SendOrPostCallback)delegate { base.OnCollectionChanged(e); }, e);
                //}
            }
        }
        #endregion
    }

}