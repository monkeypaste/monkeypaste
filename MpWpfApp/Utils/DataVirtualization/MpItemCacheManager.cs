using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using System.Timers;
using System.Collections;

namespace MpWpfApp {
    // EventArgs class for the CacheChanged event 
    public class MpCacheChangedEventArgs<T> : EventArgs {
        public T oldItem { get; set; }
        public T newItem { get; set; }
        public int itemIndex { get; set; }
    }

    // Implements a relatively simple cache for items based on a set of ranges
    public class MpItemCacheManager<T>  {
        // data structure to hold all the items that are in the ranges the cache manager is looking after
        private List<MpCacheEntryBlock<T>> _cacheBlocks;
        // List of ranges for items that are not present in the cache
        internal MpItemIndexRangeList _requests;
        // list of ranges for items that are present in the cache
        private MpItemIndexRangeList _cachedResults;
        // Range of items that is currently being requested
        private ItemIndexRange _requestInProgress;
        // Used to be able to cancel outstanding requests
        private CancellationTokenSource _cancelTokenSource;
        // Callback that will be used to request data
        private FetchDataCallbackHandler _fetchDataCallback;
        // Maximum number of items that can be fetched in one batch
        private uint _maxBatchFetchSize;
        // Timer to optimize the the fetching of data so we throttle requests if the list is still changing
        private System.Timers.Timer _timer;

#if DEBUG
        // Name for trace messages, and when debugging so you know which instance of the cache manager you are dealing with
        string debugName = string.Empty;

        public object Current => throw new NotImplementedException();
#endif
        public MpItemCacheManager(FetchDataCallbackHandler callback, uint batchsize, string debugName = "ItemCacheManager") {
            _cacheBlocks = new List<MpCacheEntryBlock<T>>();
            _requests = new MpItemIndexRangeList();
            _cachedResults = new MpItemIndexRangeList();
            _fetchDataCallback = callback;
            _maxBatchFetchSize = batchsize;
            //set up a timer that is used to delay fetching data so that we can catch up if the list is scrolling fast
            _timer = new System.Timers.Timer();
            _timer.Elapsed += (sender, args) => {
                FetchData();
            };
            _timer.Interval = new TimeSpan(20 * 10000).TotalMilliseconds;
#if DEBUG
            debugName = debugName;
#endif
#if TRACE_DATASOURCE
                        Debug.WriteLine(debugName + "* Cache initialized/reset");
#endif
        }

        public event TypedEventHandler<object, MpCacheChangedEventArgs<T>> CacheChanged;

        /// <summary>
        /// Indexer for access to the item cache
        /// </summary>
        /// <param name="index">Item Index</param>
        /// <returns></returns>
        public T this[int index] {
            get {
                // iterates through the cache blocks to find the item
                foreach (MpCacheEntryBlock<T> block in _cacheBlocks) {
                    if (index >= block.FirstIndex && index <= block.LastIndex) {
                        return block.Items[index - block.FirstIndex];
                    }
                }
                return default(T);
            }
            set {
                // iterates through the cache blocks to find the right block
                for (int i = 0; i < _cacheBlocks.Count; i++) {
                    MpCacheEntryBlock<T> block = _cacheBlocks[i];
                    if (index >= block.FirstIndex && index <= block.LastIndex) {
                        block.Items[index - block.FirstIndex] = value;
                        //register that we have the result in the cache
                        if (value != null) { _cachedResults.Add((uint)index, 1); }
                        return;
                    }
                    // We have moved past the block where the item is supposed to live
                    if (block.FirstIndex > index) {
                        AddOrExtendBlock(index, value, i);
                        return;
                    }
                }
                // No blocks exist, so creating a new block
                AddOrExtendBlock(index, value, _cacheBlocks.Count);
            }
        }

        // Extends an existing block if the item fits at the end, or creates a new block
        private void AddOrExtendBlock(int index, T value, int insertBeforeBlock) {
            if (insertBeforeBlock > 0) {
                MpCacheEntryBlock<T> block = _cacheBlocks[insertBeforeBlock - 1];
                if (block.LastIndex == index - 1) {
                    T[] newItems = new T[block.Length + 1];
                    Array.Copy(block.Items, newItems, (int)block.Length);
                    newItems[block.Length] = value;
                    block.Length++;
                    block.Items = newItems;
                    return;
                }
            }
            MpCacheEntryBlock<T> newBlock = new MpCacheEntryBlock<T>() { FirstIndex = index, Length = 1, Items = new T[] { value } };
            _cacheBlocks.Insert(insertBeforeBlock, newBlock);
        }


        /// <summary>
        /// Updates the desired item range of the cache, discarding items that are not needed, 
        /// and figuring out which items need to be requested. It will then kick off a fetch if required.
        /// </summary>
        /// <param name="ranges">New set of ranges the cache should hold</param>
        public void UpdateRanges(ItemIndexRange[] ranges) {
            //Normalize ranges to get a unique set of discontinuous ranges
            ranges = NormalizeRanges(ranges);

            // Fail fast if the ranges haven't changed
            if (!HasRangesChanged(ranges)) { 
                return; 
            }

            //To make the cache update easier, we'll create a new set of CacheEntryBlocks
            List<MpCacheEntryBlock<T>> newCacheBlocks = new List<MpCacheEntryBlock<T>>();
            foreach (ItemIndexRange range in ranges) {
                MpCacheEntryBlock<T> newBlock = new MpCacheEntryBlock<T>() { 
                    FirstIndex = range.FirstIndex, 
                    Length = range.Length, 
                    Items = new T[range.Length] };

                newCacheBlocks.Add(newBlock);
            }

#if TRACE_DATASOURCE
                        string s = "┌ " + debugName + ".UpdateRanges: ";
                        foreach (ItemIndexRange range in ranges)
                        {
                            s += range.FirstIndex + "->" + range.LastIndex + " ";
                        }
                        Debug.WriteLine(s);
#endif
            //Copy over data to the new cache blocks from the old ones where there is overlap
            int lastTransferred = 0;
            for (int i = 0; i < ranges.Length; i++) {
                MpCacheEntryBlock<T> newBlock = newCacheBlocks[i];
                ItemIndexRange range = ranges[i];
                int j = lastTransferred;
                while (j < _cacheBlocks.Count && _cacheBlocks[j].FirstIndex <= ranges[i].LastIndex) {
                    ItemIndexRange overlap, oldEntryRange;
                    ItemIndexRange[] added, removed;
                    MpCacheEntryBlock<T> oldBlock = _cacheBlocks[j];
                    oldEntryRange = new ItemIndexRange(oldBlock.FirstIndex, oldBlock.Length);
                    bool hasOverlap = oldEntryRange.DiffRanges(range, out overlap, out removed, out added);
                    if (hasOverlap) {
                        Array.Copy(
                            oldBlock.Items, 
                            overlap.FirstIndex - oldBlock.FirstIndex, 
                            newBlock.Items, 
                            overlap.FirstIndex - range.FirstIndex, 
                            (int)overlap.Length);
#if TRACE_DATASOURCE
                                                Debug.WriteLine("│ Transfering cache items " + overlap.FirstIndex + "->" + overlap.LastIndex);
#endif
                    }
                    j++;
                    if (ranges.Length > i + 1 && oldBlock.LastIndex < ranges[i + 1].FirstIndex) { 
                        lastTransferred = j; 
                    }
                }
            }
            //swap over to the new cache
            _cacheBlocks = newCacheBlocks;

            //figure out what items need to be fetched because we don't have them in the cache
            _requests = new MpItemIndexRangeList(ranges);
            MpItemIndexRangeList newCachedResults = new MpItemIndexRangeList();

            // Use the previous knowlege of what we have cached to form the new list
            foreach (ItemIndexRange range in ranges) {
                foreach (ItemIndexRange cached in _cachedResults) {
                    ItemIndexRange overlap;
                    ItemIndexRange[] added, removed;
                    bool hasOverlap = cached.DiffRanges(range, out overlap, out removed, out added);
                    if (hasOverlap) { 
                        
                        newCachedResults.Add(overlap); 
                    }
                }
            }
            // remove the data we know we have cached from the results
            foreach (ItemIndexRange range in newCachedResults) {
                _requests.Subtract(range);
            }
            _cachedResults = newCachedResults;

            StartFetchData();

#if TRACE_DATASOURCE
                        s = "└ Pending requests: ";
                        foreach (ItemIndexRange range in _requests)
                        {
                            s += range.FirstIndex + "->" + range.LastIndex + " ";
                        }
                        Debug.WriteLine(s);
#endif
        }

        // Compares the new ranges against the previous ones to see if they have changed
        private bool HasRangesChanged(ItemIndexRange[] ranges) {
            if (ranges.Length != _cacheBlocks.Count) {
                return true;
            }
            for (int i = 0; i < ranges.Length; i++) {
                ItemIndexRange r = ranges[i];
                MpCacheEntryBlock<T> block = _cacheBlocks[i];
                if (r.FirstIndex != block.FirstIndex || r.LastIndex != block.LastIndex) {
                    return true;
                }
            }
            return false;
        }

        // Gets the first block of items that we don't have values for
        public ItemIndexRange GetFirstRequestBlock() {
            if (_requests.Count > 0) {
                ItemIndexRange range = _requests[0];
                if (range.Length > _maxBatchFetchSize) {
                    range = new ItemIndexRange(range.FirstIndex, _maxBatchFetchSize);
                }
                return range;
            }
            return null;
        }


        // Throttling function for fetching data. Forces a wait of 20ms before making the request.
        // If another fetch is requested in that time, it will reset the timer, so we don't fetch data if the view is actively scrolling
        public void StartFetchData() {
            // Verify if an active request is still needed
            if (_requestInProgress != null) {
                if (_requests.Intersects(_requestInProgress)) {
                    return;
                } else {
                    //cancel the existing request
#if TRACE_DATASOURCE
                                        Debug.WriteLine("> " + debugName + " Cancelling request: " + _requestInProgress.FirstIndex + "->" + _requestInProgress.LastIndex);
#endif
                    _cancelTokenSource.Cancel();
                }
            }

            //Using a timer to delay fetching data by 20ms, if another range comes in that time, then the timer is reset.
            _timer.Stop();
            _timer.Start();
        }

        public delegate Task<T[]> FetchDataCallbackHandler(ItemIndexRange range, CancellationToken ct);

        // Called by the timer to make a request for data
        public async void FetchData() {
            //Stop the timer so we don't get fired again unless data is requested
            _timer.Stop();
            if (_requestInProgress != null) {
                // Verify if an active request is still needed
                if (_requests.Intersects(_requestInProgress)) {
                    return;
                } else {
                    // Cancel the existing request
#if TRACE_DATASOURCE
                                        Debug.WriteLine(">" + debugName + " Cancelling request: " + _requestInProgress.FirstIndex + "->" + _requestInProgress.LastIndex);
#endif
                    _cancelTokenSource.Cancel();
                }
            }

            ItemIndexRange nextRequest = GetFirstRequestBlock();
            if (nextRequest != null) {
                _cancelTokenSource = new CancellationTokenSource();
                CancellationToken ct = _cancelTokenSource.Token;
                _requestInProgress = nextRequest;
                T[] data = null;
                try {
#if TRACE_DATASOURCE
                                        Debug.WriteLine(">" + debugName + " Fetching items " + nextRequest.FirstIndex + "->" + nextRequest.LastIndex);
#endif
                    // Use the callback to get the data, passing in a cancellation token
                    data = await _fetchDataCallback(nextRequest, ct);

                    if (!ct.IsCancellationRequested) {
#if TRACE_DATASOURCE
                                                Debug.WriteLine(">" + debugName + " Inserting items into cache at: " + nextRequest.FirstIndex + "->" + (nextRequest.FirstIndex + data.Length - 1));
#endif
                        for (int i = 0; i < data.Length; i++) {
                            int cacheIndex = (int)(nextRequest.FirstIndex + i);

                            T oldItem = this[cacheIndex];
                            T newItem = data[i];

                            if (!newItem.Equals(oldItem)) {
                                this[cacheIndex] = newItem;

                                // Fire CacheChanged so that the datasource can fire its INCC event, and do other work based on the item having data
                                if (CacheChanged != null) {
                                    CacheChanged(this, new MpCacheChangedEventArgs<T>() { oldItem = oldItem, newItem = newItem, itemIndex = cacheIndex });
                                }
                            }
                        }
                        _requests.Subtract(new ItemIndexRange(nextRequest.FirstIndex, (uint)data.Length));
                    }
                }
                // Try/Catch is needed as cancellation is via an exception
                catch (OperationCanceledException) { } finally {
                    _requestInProgress = null;
                    // Start another request if required
                    FetchData();
                }
            }
        }


        /// <summary>
        /// Merges a set of ranges to form a new set of non-contiguous ranges
        /// </summary>
        /// <param name="ranges">The list of ranges to merge</param>
        /// <returns>A smaller set of merged ranges</returns>
        private ItemIndexRange[] NormalizeRanges(ItemIndexRange[] ranges) {
            List<ItemIndexRange> results = new List<ItemIndexRange>();
            foreach (ItemIndexRange range in ranges) {
                bool handled = false;
                for (int i = 0; i < results.Count; i++) {
                    ItemIndexRange existing = results[i];
                    if (range.ContiguousOrOverlaps(existing)) {
                        results[i] = existing.Combine(range);
                        handled = true;
                        break;
                    } else if (range.FirstIndex < existing.FirstIndex) {
                        results.Insert(i, range);
                        handled = true;
                        break;
                    }
                }
                if (!handled) { 
                    results.Add(range); 
                }
            }
            return results.ToArray();
        }


        // Sees if the value is in our cache if so it returns the index
        public int IndexOf(T value) {
            foreach (MpCacheEntryBlock<T> entry in _cacheBlocks) {
                int index = Array.IndexOf<T>(entry.Items, value);
                if (index != -1) {
                    return index + entry.FirstIndex;
                }
            }
            return -1;
        }

        // Type for the cache blocks
        public class MpCacheEntryBlock<ITEMTYPE> {
            public int FirstIndex;
            public uint Length;
            public ITEMTYPE[] Items;

            public int LastIndex { 
                get { 
                    return FirstIndex + (int)Length - 1; 
                } 
            }
        }
    }
}
