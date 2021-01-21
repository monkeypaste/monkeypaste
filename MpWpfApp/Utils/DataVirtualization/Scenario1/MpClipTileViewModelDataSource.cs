using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace MpWpfApp {
    //********************************************************************************************
    //*
    //* Note: This sample uses a custom compiler constant to enable tracing. If you add
    //* TRACE_DATASOURCE to the Conditional compilation symbols of the Build tab of the
    //* Project Properties window, then the application will spit out trace data to the
    //* Output window while debugging.
    //*
    //********************************************************************************************


    /// <summary>
    /// A custom datasource over the file system that supports data virtualization
    /// </summary>
    public class MpClipTileViewModelDataSource : INotifyCollectionChanged, System.Collections.IList, IItemsRangeInfo {
        public MpCopyItemDataProvider CopyItemDataProvider { get; set; }

        // Folder that we are browsing
        //private StorageFolder _folder;

        private uint _pageSize = 15;

        private int _tagId;
        // Query object that will tell us if the folder content changed
        //private StorageFileQueryResult _queryResult;
        // Dispatcher so we can marshal calls back to the UI thread
        private Dispatcher _dispatcher;
        // Cache for the file data that is currently being used
        private MpItemCacheManager<MpClipTileViewModel> _itemCache;

        // Total number of files available
        private int _count = 1;


        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private MpClipTileViewModelDataSource() {
            //Setup the dispatcher for the UI thread
            _dispatcher = Application.Current.MainWindow.Dispatcher;
            // The ItemCacheManager does most of the heavy lifting. We pass it a callback that it will use to actually fetch data, and the max size of a request
            _itemCache = new MpItemCacheManager<MpClipTileViewModel>(FetchDataCallback, _pageSize);
            _itemCache.CacheChanged += ItemCache_CacheChanged;
        }

        // Factory method to create the datasource
        // Requires async work which is why it needs a factory rather than being part of the constructor
        public static async Task<MpClipTileViewModelDataSource> GetDataSoure(int tagId) {
            MpClipTileViewModelDataSource ds = new MpClipTileViewModelDataSource();
            await ds.SetTag(tagId);
            return ds;
        }

        public async Task SetTag(int tagId) {
            if (_tagId == tagId) {
                return;
            }
            if (CopyItemDataProvider != null) {
                CopyItemDataProvider.CopyItemChanged -= CopyItemDataProvider_CopyItemChanged;
            }
            _tagId = tagId;
            CopyItemDataProvider = new MpCopyItemDataProvider(_tagId);
            CopyItemDataProvider.CopyItemChanged += CopyItemDataProvider_CopyItemChanged;
            await UpdateCount();
        }

        private void CopyItemDataProvider_CopyItemChanged(object sender, object args) {
            //This callback can occur on a different thread so we need to marshal it back to the UI thread
            if (!_dispatcher.CheckAccess()) {
                //var t = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, ResetCollection);
                _dispatcher.BeginInvoke(new Action(ResetCollection));
            } else {
                ResetCollection();
            }
        }

        private void AlterCollection(MpCopyItemChangeType changeType, object args) {
            switch(changeType) {
                case MpCopyItemChangeType.Reset:
                    // Unhook the old change notification
                    if (_itemCache != null) {
                        _itemCache.CacheChanged -= ItemCache_CacheChanged;
                    }

                    // Create a new instance of the cache manager
                    _itemCache = new MpItemCacheManager<MpClipTileViewModel>(FetchDataCallback, _pageSize);
                    _itemCache.CacheChanged += ItemCache_CacheChanged;
                    if (CollectionChanged != null) {
                        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    break;
                case MpCopyItemChangeType.Add:
                    //_itemCache
                    break;
            }
        }
        // Handles a change notification for the list of files from the OS
        private void ResetCollection() {
            // Unhook the old change notification
            if (_itemCache != null) {
                _itemCache.CacheChanged -= ItemCache_CacheChanged;
            }

            // Create a new instance of the cache manager
            _itemCache = new MpItemCacheManager<MpClipTileViewModel>(FetchDataCallback, _pageSize);
            _itemCache.CacheChanged += ItemCache_CacheChanged;
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        async Task UpdateCount() {
            _count = await CopyItemDataProvider.GetCopyItemsByTagIdCountAsync();
            if (CollectionChanged != null) {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #region IList Implementation

        public bool Contains(object value) {
            return IndexOf(value) != -1;
        }

        public int IndexOf(object value) {
            return (value != null) ? _itemCache.IndexOf((MpClipTileViewModel)value) : -1;
        }

        public object this[int index] {
            get {
                // The cache will return null if it doesn't have the item. 
                // Once the item is fetched it will fire a changed event so that we can inform the list control
                return _itemCache[index];
            }
            set {
                throw new NotImplementedException();
            }
        }
        public int Count {
            get { return _count; }
        }

        #endregion

        //Required for the IItemsRangeInfo interface
        public void Dispose() {
            _itemCache = null;
        }

        /// <summary>
        /// Primary method for IItemsRangeInfo interface
        /// Is called when the list control's view is changed
        /// </summary>
        /// <param name="visibleRange">The range of items that are actually visible</param>
        /// <param name="trackedItems">Additional set of ranges that the list is using, for example the buffer regions and focussed element</param>
        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems) {
#if TRACE_DATASOURCE
                        string s = string.Format("* RangesChanged fired: Visible {0}->{1}", visibleRange.FirstIndex, visibleRange.LastIndex);
                        foreach (ItemIndexRange r in trackedItems) { s += string.Format(" {0}->{1}", r.FirstIndex, r.LastIndex); }
                        Debug.WriteLine(s);
#endif
            // We know that the visible range is included in the broader range so don't need to hand it to the UpdateRanges call
            // Update the cache of items based on the new set of ranges. It will callback for additional data if required
            _itemCache.UpdateRanges(trackedItems.ToArray());
        }

        // Callback from itemcache that it needs items to be retrieved
        // Using this callback model abstracts the details of this specific datasource from the cache implementation
        private async Task<MpClipTileViewModel[]> FetchDataCallback(ItemIndexRange batch, CancellationToken ct) {
            // Fetch file objects from filesystem
            var results = await CopyItemDataProvider.GetCopyItemsByTagIdAsync((uint)batch.FirstIndex, Math.Max(batch.Length, 20)).AsTask(ct);
            //.GetFilesAsync((uint)batch.FirstIndex, Math.Max(batch.Length, 20)).AsTask(ct);
            var clipTileViewModelList = new List<MpClipTileViewModel>();
            if (results != null) {
                for (int i = 0; i < results.Count; i++) {
                    // Check if request has been cancelled, if so abort getting additional data
                    ct.ThrowIfCancellationRequested();
                    // Create our MpClipTileViewModel object with the file data and thumbnail 
                    var newItem = await MpClipTileViewModel.LoadClipTileViewModel(results[i], ct);
                    clipTileViewModelList.Add(newItem);
                }
            }
            return clipTileViewModelList.ToArray();
        }

        // Event fired when items are inserted in the cache
        // Used to fire our collection changed event
        private void ItemCache_CacheChanged(object sender, MpCacheChangedEventArgs<MpClipTileViewModel> args) {
            if (CollectionChanged != null) {
                CollectionChanged(
                    this, 
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, 
                        args.oldItem, 
                        args.newItem, 
                        args.itemIndex));
            }
        }

        #region Parts of IList Not Implemented

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize {
            get { 
                return false; 
            }
        }

        public bool IsReadOnly {
            get { 
                return false; 
            }
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }
        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public bool IsSynchronized {
            get { 
                throw new NotImplementedException(); 
            }
        }

        public object SyncRoot {
            get { 
                throw new NotImplementedException(); 
            }
        }

        public System.Collections.IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        #endregion
    }


}
