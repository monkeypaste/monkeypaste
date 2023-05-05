using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvPersistentClipTileProperties {
        public int QueryOffsetIdx { get; set; } = -1;
        public bool IsTileDragging { get; set; }
        public bool IsSelected { get; set; }

        public bool IsTitleReadOnly { get; set; } = true;
        public bool IsContentReadOnly { get; set; } = true;

        public double? UniqueWidth { get; set; }
        public double? UniqueHeight { get; set; }

        public bool HasAnyUniqueProps =>
            //CopyItemId <= 0 ?
            //false :
            //QueryOffsetIdx < 0 ||
            IsTileDragging ||
            IsSelected ||
            !IsTitleReadOnly ||
            !IsContentReadOnly ||
            UniqueWidth.HasValue ||
            UniqueHeight.HasValue;
    }

    public static class MpAvPersistentClipTilePropertiesHelper {
        #region Private Variables

        private static Dictionary<int, MpAvPersistentClipTileProperties> _props = new Dictionary<int, MpAvPersistentClipTileProperties>();

        #endregion


        private static void _props_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

        }
        private static void CleanupProps() {
            var to_remove_ids = _props.Where(x => !x.Value.HasAnyUniqueProps).Select(x => x.Key).ToArray();
            for (int i = 0; i < to_remove_ids.Length; i++) {
                _props.Remove(to_remove_ids[i]);
            }
        }

        private static MpAvPersistentClipTileProperties GetProps(int ciid, bool autoAdd = false, int idx = -1) {
            if (_props.ContainsKey(ciid)) {
                _props[ciid].QueryOffsetIdx = idx;
                return _props[ciid];
            }
            if (!autoAdd) {
                return null;
            }
            var new_props = new MpAvPersistentClipTileProperties() {
                QueryOffsetIdx = idx
            };
            _props.Add(ciid, new_props);
            return new_props;
        }

        public static void RemoveProps(int ciid) {
            if (GetProps(ciid) is MpAvPersistentClipTileProperties pp) {
                _props.Remove(_props.FirstOrDefault(x => x.Value == pp).Key);
            }
        }

        public static void UpdateQueryOffsetIdx(int ciid, int idx) {
            if (GetProps(ciid) is MpAvPersistentClipTileProperties pp) {
                pp.QueryOffsetIdx = idx;
            }
        }

        #region IsSelected
        public static void SetPersistentSelectedItem(int ciid, int idx) {
            _props.ForEach(x => x.Value.IsSelected = false);
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsSelected = true;
            }
            CleanupProps();
        }
        public static int GetPersistentSelectedItemId() {
            if (_props.FirstOrDefault(x => x.Value.IsSelected) is KeyValuePair<int, MpAvPersistentClipTileProperties> kvp &&
                kvp.Value is MpAvPersistentClipTileProperties pp) {
                return kvp.Key;
            }
            return -1;
        }
        public static bool HasPersistenSelection() {
            return GetPersistentSelectedItemId() >= 0;
        }

        public static void ClearPersistentSelection() {
            _props.ForEach(x => x.Value.IsSelected = false);
            CleanupProps();
        }
        #endregion

        #region IsTileDragging
        public static void AddPersistentIsTileDraggingTile_ById(int ciid, int idx) {
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsTileDragging = true;
            }
        }
        public static void RemovePersistentIsTileDraggingTile_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsTileDragging = false;
                CleanupProps();
            }
        }
        public static void ClearPersistentIsTileDragging() {
            _props.ForEach(x => x.Value.IsTileDragging = false);
            CleanupProps();
        }

        public static bool IsPersistentTileDraggingEditable_ById(int ciid, int idx) {
            return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && pp.IsTileDragging;
        }

        #endregion

        #region IsContentEditable
        public static void AddPersistentIsContentEditableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsContentReadOnly = false;
            }
        }
        public static void RemovePersistentIsContentEditableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsContentReadOnly = true;
                CleanupProps();
            }
        }

        public static bool IsPersistentTileContentEditable_ById(int ciid, int idx) {
            return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && !pp.IsContentReadOnly;
        }
        #endregion

        #region IsTitleEditable
        public static void AddPersistentIsTitleEditableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsTitleReadOnly = false;
            }
        }
        public static void RemovePersistentIsTitleEditableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsTitleReadOnly = true;
                CleanupProps();
            }
        }

        public static bool IsPersistentTileTitleEditable_ById(int ciid, int idx) {
            return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && !pp.IsTitleReadOnly;
        }
        #endregion

        #region Unique Size
        public static void AddOrReplacePersistentSize_ById(int ciid, double uniqueSize, int idx) {
            GetProps(ciid, true, idx).UniqueWidth = uniqueSize;
        }

        public static void RemovePersistentSize_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.UniqueWidth = null;
                CleanupProps();
            }
        }
        public static bool TryGetUniqueWidth_ByOffsetIdx(int idx, out double uwidth) {
            if (_props.FirstOrDefault(x => x.Value.QueryOffsetIdx == idx) is KeyValuePair<int, MpAvPersistentClipTileProperties> kvp &&
                kvp.Value is MpAvPersistentClipTileProperties pctp &&
                pctp.UniqueWidth.HasValue) {
                uwidth = pctp.UniqueWidth.Value;
                return true;
            }
            uwidth = 0;
            return false;
        }

        public static bool TryGetPersistentWidth_ById(int ciid, int idx, out double uniqueSize) {
            uniqueSize = 0;
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.UniqueWidth.HasValue) {
                uniqueSize = pp.UniqueWidth.Value;
                return true;
            }
            return false;
        }

        public static bool IsTileHaveUniqueSize(int ciid, int idx) {
            return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && pp.UniqueWidth.HasValue;
        }
        public static void ClearPersistentWidths() {
            //_persistentUniqueTileSizeLookup_ByIdx.Clear();
            //_persistentUniqueTileSizeLookup_ById.Clear();
            _props.ForEach(x => x.Value.UniqueWidth = null);
            CleanupProps();
        }
        #endregion
    }
}
