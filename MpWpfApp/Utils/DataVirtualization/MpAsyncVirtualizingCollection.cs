using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Derived VirtualizatingCollection, performing loading asychronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class MpAsyncVirtualizingCollection<T> : MpVirtualizingCollection<T>, INotifyCollectionChanged {
        #region Constructors

        public MpAsyncVirtualizingCollection() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MpAsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        public MpAsyncVirtualizingCollection(MpIItemsProvider<T> itemsProvider)
            : base(itemsProvider) {
            _synchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpAsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        public MpAsyncVirtualizingCollection(MpIItemsProvider<T> itemsProvider, int pageSize)
            : base(itemsProvider, pageSize) {
            _synchronizationContext = SynchronizationContext.Current; 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MpAsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeout">The page timeout.</param>
        public MpAsyncVirtualizingCollection(MpIItemsProvider<T> itemsProvider, int pageSize, int pageTimeout)
            : base(itemsProvider, pageSize, pageTimeout) {
            _synchronizationContext = SynchronizationContext.Current;
        }

        #endregion

        #region SynchronizationContext

        private readonly SynchronizationContext _synchronizationContext;

        /// <summary>
        /// Gets the synchronization context used for UI-related operations. This is obtained as
        /// the current SynchronizationContext when the AsyncVirtualizingCollection is created.
        /// </summary>
        /// <value>The synchronization context.</value>
        protected SynchronizationContext SynchronizationContext {
            get { return _synchronizationContext; }
        }

        #endregion

        

        #region IsLoading

        private bool _isLoading;
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private bool _isLoadingCount;
        public bool IsLoadingCount {
            get {
                return _isLoadingCount;
            }
            set {
                if(_isLoadingCount != value) {
                    _isLoadingCount = value;
                    OnPropertyChanged(nameof(IsLoadingCount));
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        private bool _isLoadingData = true;
        public bool IsLoadingData {
            get {
                return _isLoadingData;
            }
            set {
                if (_isLoadingData != value) {
                    _isLoadingData = value;
                    OnPropertyChanged(nameof(IsLoadingData));
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        #endregion

        #region Load overrides

        /// <summary>
        /// Asynchronously loads the count of items.
        /// </summary>
        protected override void LoadCount() {
            Count = 0;
            IsLoading = true;
            IsLoadingCount = true;
            ThreadPool.QueueUserWorkItem(LoadCountWork);
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">None required.</param>
        private void LoadCountWork(object args) {
            int count = FetchCount();
            SynchronizationContext.Send(LoadCountCompleted, count);
        }

        /// <summary>
        /// Performed on UI-thread after LoadCountWork.
        /// </summary>
        /// <param name="args">Number of items returned.</param>
        private void LoadCountCompleted(object args) {
            Count = (int)args;
            IsLoadingCount = false;
            IsLoading = false;
            FireCollectionReset();
        }

        /// <summary>
        /// Asynchronously loads the page.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void LoadPage(int index) {
            IsLoadingData = true;
            IsLoading = true;
            ThreadPool.QueueUserWorkItem(LoadPageWork, index);
        }

        /// <summary>
        /// Performed on background thread.
        /// </summary>
        /// <param name="args">Index of the page to load.</param>
        private void LoadPageWork(object args) {
            int pageIndex = (int)args;
            IList<T> page = FetchPage(pageIndex);
            SynchronizationContext.Send(LoadPageCompleted, new object[] { pageIndex, page });
        }

        /// <summary>
        /// Performed on UI-thread after LoadPageWork.
        /// </summary>
        /// <param name="args">object[] { int pageIndex, IList(T) page }</param>
        private void LoadPageCompleted(object args) {
            int pageIndex = (int)((object[])args)[0];
            IList<T> page = (IList<T>)((object[])args)[1];

            PopulatePage(pageIndex, page);
            IsLoading = false;
            IsLoadingData = false;
            FireCollectionReset();
        }

        #endregion
    }
}
