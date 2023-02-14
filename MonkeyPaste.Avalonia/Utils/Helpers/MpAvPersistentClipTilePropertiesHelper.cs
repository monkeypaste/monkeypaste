using MonkeyPaste.Common;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPersistentClipTilePropertiesHelper {
        #region Properties


        #endregion
        #region IsSelected
        public static List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();
        #endregion

        #region IsTileDragging
        private static List<int> _persistentIsTileDraggingTiles_ById { get; set; } = new List<int>();
        public static void AddPersistentIsTileDraggingTile_ById(int ciid) {
            if (_persistentIsTileDraggingTiles_ById.Contains(ciid)) {
                return;
            }
            _persistentIsTileDraggingTiles_ById.Add(ciid);
        }
        public static void RemovePersistentIsTileDraggingTile_ById(int ciid) {
            if (_persistentIsTileDraggingTiles_ById.Contains(ciid)) {
                _persistentIsTileDraggingTiles_ById.Remove(ciid);
            }
        }
        public static void ClearPersistentIsTileDragging() {
            _persistentIsTileDraggingTiles_ById.Clear();
        }

        public static bool IsPersistentTileDraggingEditable_ById(int ciid) {
            return _persistentIsTileDraggingTiles_ById.Contains(ciid);
        }

        public static List<int> GetIsDraggingTiles() {
            return _persistentIsTileDraggingTiles_ById;
        }
        #endregion

        #region IsContentEditable
        private static List<int> _persistentIsContentEditableTiles_ById { get; set; } = new List<int>();
        public static void AddPersistentIsContentEditableTile_ById(int ciid) {
            if (_persistentIsContentEditableTiles_ById.Contains(ciid)) {
                return;
            }
            _persistentIsContentEditableTiles_ById.Add(ciid);
        }
        public static void RemovePersistentIsContentEditableTile_ById(int ciid) {
            if (_persistentIsContentEditableTiles_ById.Contains(ciid)) {
                _persistentIsContentEditableTiles_ById.Remove(ciid);
            }
        }

        public static bool IsPersistentTileContentEditable_ById(int ciid) {
            return _persistentIsContentEditableTiles_ById.Contains(ciid);
        }
        #endregion

        #region IsTitleEditable
        private static List<int> _persistentIsTitleEditableTiles_ById { get; set; } = new List<int>();
        public static void AddPersistentIsTitleEditableTile_ById(int ciid) {
            if (_persistentIsTitleEditableTiles_ById.Contains(ciid)) {
                return;
            }
            _persistentIsTitleEditableTiles_ById.Add(ciid);
        }
        public static void RemovePersistentIsTitleEditableTile_ById(int ciid) {
            if (_persistentIsTitleEditableTiles_ById.Contains(ciid)) {
                _persistentIsTitleEditableTiles_ById.Remove(ciid);
            }
        }

        public static bool IsPersistentTileTitleEditable_ById(int ciid) {
            return _persistentIsTitleEditableTiles_ById.Contains(ciid);
        }
        #endregion

        #region Unique Size

        private static Dictionary<int, double> _persistentUniqueTileSizeLookup_ById { get; set; } = new Dictionary<int, double>();
        //private static Dictionary<int, double> _persistentUniqueTileSizeLookup_ByIdx { get; set; } = new Dictionary<int, double>();
        public static void AddOrReplacePersistentSize_ById(int ciid, double uniqueSize) {
            _persistentUniqueTileSizeLookup_ById.AddOrReplace(ciid, uniqueSize);

            //int queryOffset = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(ciid);
            //if (queryOffset < 0) {
            //    return;
            //}
            //_persistentUniqueTileSizeLookup_ByIdx.AddOrReplace(queryOffset, uniqueSize);
        }

        public static void RemovePersistentSize_ById(int ciid) {
            if (_persistentUniqueTileSizeLookup_ById.ContainsKey(ciid)) {
                _persistentUniqueTileSizeLookup_ById.Remove(ciid);
            }
            //int queryOffset = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(ciid);
            //if (queryOffset < 0) {
            //    return;
            //}
            //_persistentUniqueTileSizeLookup_ByIdx.Remove(queryOffset);
        }

        public static bool TryGetByPersistentSize_ById(int ciid, out double uniqueSize) {
            return _persistentUniqueTileSizeLookup_ById.TryGetValue(ciid, out uniqueSize);
        }

        public static bool IsTileHaveUniqueSize(int ciid) {
            return _persistentUniqueTileSizeLookup_ById.TryGetValue(ciid, out double uniqueSize);
        }
        public static void ClearPersistentWidths() {
            //_persistentUniqueTileSizeLookup_ByIdx.Clear();
            _persistentUniqueTileSizeLookup_ById.Clear();
        }
        #endregion
    }
}
