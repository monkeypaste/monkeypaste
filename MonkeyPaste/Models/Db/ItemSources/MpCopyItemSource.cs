using MonkeyPaste;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemSource {
        #region Singleton Definition
        private static readonly Lazy<MpCopyItemSource> _Lazy = new Lazy<MpCopyItemSource>(() => new MpCopyItemSource());
        public static MpCopyItemSource Instance { get { return _Lazy.Value; } }

        public void Init() {
            MpDb.Instance.OnItemAdded += Instance_OnItemAdded;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;

            Task.Run(async () => {
                string query = "select count(Id) from MpCopyItem";
                var result = await MpDb.Instance.QueryAsync("MpCopyItem", query);
                TotalItemCount = (int)result[0];
            });
        }

        #endregion

        #region Private Variables
        private Dictionary<int, MpCopyItem> _itemLookup = new Dictionary<int, MpCopyItem>();
        #endregion

        #region Properties
        public List<MpCopyItem> AllItems { 
            get {
                return _itemLookup.Select(x => x.Value).ToList();
            }
        }

        public int TotalItemCount { get; private set; }
        #endregion

        #region Public Methods

        public List<MpCopyItem> GetRecentItems() {
            string query = @"select * from MpCopyItem where pk_MpCopyItemId in (
                                select distinct pci.pk_MpCopyItemId from MpCopyItem aci
                                inner join MpCopyItem pci 
                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                order by aci.CopyDateTime desc limit ?)";
            var result = MpDb.Instance.Query<MpCopyItem>(query, MpPreferences.Instance.MaxRecentClipItems);

            return result;
        }

        public async Task<List<MpCopyItem>> GetRecentItemsAsync() {
            string query = @"select * from MpCopyItem where pk_MpCopyItemId in (
                                select distinct pci.pk_MpCopyItemId from MpCopyItem aci
                                inner join MpCopyItem pci 
                                ON pci.pk_MpCopyItemId = aci.fk_ParentCopyItemId or aci.fk_ParentCopyItemId = 0
                                order by aci.CopyDateTime desc limit ?)";
            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(query, MpPreferences.Instance.MaxRecentClipItems);

            return result;
        }

        public async Task<List<int>> QueryForIds(
            int tagId,
            MpClipTileSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {

            var citm = MpDb.Instance.GetTableMapping("MpCopyItem");
            string sortColumn = citm.FindColumnWithPropertyName(Enum.GetName(typeof(MpClipTileSortType), sortType)).Name;
            string isDescStr = isDescending ? "DESC" : "ASC";
            string query = string.Empty;

            switch (tagId) {
                case MpTag.RecentTagId:
                    int maxRecentCount = MpPreferences.Instance.MaxRecentClipItems;
                    query = $"select Id from MpCopyItem where fk_ParentCopyItemId = 0 orderby ModifiedDateTime {isDescStr} limit ?";
                    break;
                case MpTag.AllTagId:
                    query = $"select Id from MpCopyItem where fk_ParentCopyItemId = 0 orderby {sortColumn} {isDescStr}";
                    break;
                default:
                    
                    break;
            }

            var result = await MpDb.Instance.QueryAsync("MpCopyItem",query);
            return result.Cast<int>().ToList();
        }

        public async Task<List<MpCopyItem>> GetPageAsync(
            int tagId,
            int start,
            int count,
            MpClipTileSortType sortType,
            bool isDescending,
            Dictionary<int, int> manualSortOrderLookup = null) {
            MpCopyItem dummyCi = new MpCopyItem();
            List<MpCopyItem> result = await MpDb.Instance.GetItemsAsync<MpCopyItem>();

            switch (tagId) {
                case MpTag.RecentTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0)
                                 .OrderByDynamic(isDescending, x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    return result;
                case MpTag.AllTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0).ToList();
                    break;
                default:
                    var citl = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
                    if (isDescending) {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx descending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    } else {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx ascending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    }
                    break;
            }
            switch (sortType) {
                case MpClipTileSortType.CopyDateTime:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.CopyDateTime)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                case MpClipTileSortType.ItemType:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.ItemType)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                // TODO add rest of sort types
                case MpClipTileSortType.Manual:
                    if (manualSortOrderLookup == null) {
                        result = result.Take(count).Skip(start).ToList();
                    } else {
                        int missingCount = 0;
                        var missingItems = new List<MpCopyItem>();
                        foreach (var ci in result) {
                            if (manualSortOrderLookup.ContainsKey(ci.Id)) {
                                ci.ManualSortIdx = manualSortOrderLookup[ci.Id];
                            } else {
                                missingCount++;
                                if (isDescending) {
                                    ci.ManualSortIdx = manualSortOrderLookup.Min(x => x.Value) - missingCount;
                                } else {
                                    ci.ManualSortIdx = manualSortOrderLookup.Max(x => x.Value) + missingCount;
                                }

                            }
                        }
                        result = result.OrderByDynamic(isDescending, x => x.ManualSortIdx).Take(count).Skip(start).ToList();
                    }
                    break;
            }
            return result;
        }

        #endregion

        #region Private Methods

        #region Db Event Handlers

        private void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpCopyItem) {
                TotalItemCount++;
            }
            //if (e is MpCopyItem ci) {
            //    if (_itemLookup.ContainsKey(ci.Id)) {
            //        _itemLookup[ci.Id] = ci;
            //    } else {
            //        _itemLookup.Add(ci.Id, ci);
            //    }
            //    return;
            //}
            //int ciid = 0;
            //if (e is MpCopyItemTag cit) {
            //    ciid = cit.CopyItemId;
            //} else if (e is MpCopyItemTemplate template) {
            //    ciid = template.CopyItemId;
            //} else if (e is MpShortcut s) {
            //    ciid = s.CopyItemId;
            //}

            //if (ciid > 0) {
            //    Task.Run(async () => {
            //        MpCopyItem ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(ciid);
            //        if (_itemLookup.ContainsKey(ciid)) {
            //            _itemLookup[ciid] = ci;
            //        } else {
            //            _itemLookup.Add(ciid, ci);
            //        }
            //    });
            //}
        }

        private void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (_itemLookup.ContainsKey(ci.Id)) {
                    _itemLookup[ci.Id] = ci;
                } else {
                    _itemLookup.Add(ci.Id, ci);
                }
                return;
            }
            int ciid = 0;
            if (e is MpCopyItemTag cit) {
                ciid = cit.CopyItemId;
            } else if (e is MpCopyItemTemplate template) {
                ciid = template.CopyItemId;
            } else if (e is MpShortcut s) {
                ciid = s.CopyItemId;
            }

            if (ciid > 0) {
                Task.Run(async () => {
                    MpCopyItem ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(ciid);
                    if (_itemLookup.ContainsKey(ciid)) {
                        _itemLookup[ciid] = ci;
                    } else {
                        _itemLookup.Add(ciid, ci);
                    }
                });
            }
        }

        private void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpCopyItem) {
                TotalItemCount--;
            }
            //if (e is MpCopyItem ci) {
            //    if (_itemLookup.ContainsKey(ci.Id)) {
            //        _itemLookup.Remove(ci.Id);
            //    }
            //    return;
            //}
            //int ciid = 0;
            //if (e is MpCopyItemTag cit) {
            //    ciid = cit.CopyItemId;
            //} else if (e is MpCopyItemTemplate template) {
            //    ciid = template.CopyItemId;
            //} else if (e is MpShortcut s) {
            //    ciid = s.CopyItemId;
            //}

            //if (ciid > 0) {
            //    Task.Run(async () => {
            //        MpCopyItem ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(ciid);
            //        if (_itemLookup.ContainsKey(ciid)) {
            //            _itemLookup[ciid] = ci;
            //        } else {
            //            _itemLookup.Add(ciid, ci);
            //        }
            //    });
            //}
        }

        #endregion

        #endregion
    }
}
