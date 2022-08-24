using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPersistentClipTilePropertiesHelper {
        #region Properties
        public static List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();

        private static List<int> _persistentEditableTiles_ById { get; set; } = new List<int>();
        private static Dictionary<int, double> _persistentUniqueTileSizeLookup_ById { get; set; } = new Dictionary<int, double>();
        private static Dictionary<int, double> _persistentUniqueTileSizeLookup_ByIdx { get; set; } = new Dictionary<int, double>();


        #endregion

        #region Public Methods

        public static void AddPersistentEditableTile_ById(int ciid) {
            if (_persistentEditableTiles_ById.Contains(ciid)) {
                return;
            }
            _persistentEditableTiles_ById.Add(ciid);
        }
        public static void RemovePersistentEditableTile_ById(int ciid) {
            if (_persistentEditableTiles_ById.Contains(ciid)) {
                _persistentEditableTiles_ById.Remove(ciid);
            }
        }

        public static bool IsPersistentTileEditable_ById(int ciid) {
            return _persistentEditableTiles_ById.Contains(ciid);
        }

        public static void AddOrReplacePersistentSize_ById(int ciid, double uniqueSize) {
            _persistentUniqueTileSizeLookup_ById.AddOrReplace(ciid, uniqueSize);

            int queryOffset = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(ciid);
            if (queryOffset < 0) {
                return;
            }
            _persistentUniqueTileSizeLookup_ByIdx.AddOrReplace(queryOffset, uniqueSize);
        }

        public static void RemovePersistentSize_ById(int ciid) {
            if (_persistentUniqueTileSizeLookup_ById.ContainsKey(ciid)) {
                _persistentUniqueTileSizeLookup_ById.Remove(ciid);
            }
            int queryOffset = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(ciid);
            if (queryOffset < 0) {
                return;
            }
            _persistentUniqueTileSizeLookup_ByIdx.Remove(queryOffset);
        }

        public static void ShiftPersistentSize(int ciid, int oldOffsetIdx, int newOffsetIdx) {
            // called when tile is removed and query offset lookup needs to be adjusted
            if (TryGetByPersistentSize_ById(ciid, out double uniqueSize)) {
                _persistentUniqueTileSizeLookup_ByIdx.Remove(oldOffsetIdx);
                _persistentUniqueTileSizeLookup_ByIdx.Add(newOffsetIdx, uniqueSize);
            }
        }

        public static bool TryGetByPersistentSize_ById(int ciid, out double uniqueSize) {
            return _persistentUniqueTileSizeLookup_ById.TryGetValue(ciid, out uniqueSize);
        }

        public static bool IsTileHaveUniqueSize(int ciid) {
            return _persistentUniqueTileSizeLookup_ById.TryGetValue(ciid, out double uniqueSize);
        }
        public static void ClearPersistentWidths() {
            _persistentUniqueTileSizeLookup_ByIdx.Clear();
            _persistentUniqueTileSizeLookup_ById.Clear();
        }
        #endregion
    }
}
