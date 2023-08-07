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

        public bool IsPinPlaceholder { get; set; }
        public bool IsSubSelectable { get; set; }
        public bool IsTitleReadOnly { get; set; } = true;
        public bool IsContentReadOnly { get; set; } = true;
        public bool IsTransactionPaneOpen { get; set; }
        public string SelectedTransNodeGuid { get; set; }

        public double? UniqueWidth { get; set; }
        public double? UniqueHeight { get; set; }

        public MpQuillEditorSelectionStateMessage SubSelectionState { get; set; }

        public bool HasAnyUniqueProps =>
            //CopyItemId <= 0 ?
            //false :
            //QueryOffsetIdx < 0 ||
            IsTransactionPaneOpen ||
            IsSubSelectable ||
            !string.IsNullOrEmpty(SelectedTransNodeGuid) ||
            SubSelectionState != null ||
            IsTileDragging ||
            IsPinPlaceholder ||
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

        #region Public Methods

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

        #region SubSelection State
        public static void AddPersistentSubSelectionState(int ciid, int idx, MpQuillEditorSelectionStateMessage selState) {
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.SubSelectionState = selState;
            }
        }

        public static bool TryGetPersistentSubSelectionState(int ciid, int idx, out MpQuillEditorSelectionStateMessage selState) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.SubSelectionState != null) {
                selState = pp.SubSelectionState;
                return true;
            }
            selState = null;
            return false;
        }
        public static void RemovePersistentSubSelectionState(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.SubSelectionState = null;
                CleanupProps();
            }
        }
        public static void ClearPersistentSubSelectionState() {
            _props.ForEach(x => x.Value.SubSelectionState = null);
            CleanupProps();
        }

        #endregion

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
        public static bool HasPersistentSelection() {
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

        #region IsSubSelectable
        public static void AddPersistentIsSubSelectableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsSubSelectable = true;
            }
        }
        public static void RemovePersistentIsSubSelectableTile_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.IsSubSelectable = false;
                CleanupProps();
            }
        }

        public static bool IsPersistentIsSubSelectable_ById(int ciid, int idx) {
            return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && pp.IsSubSelectable;
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

        public static MpSize DefQuerySize { get; set; } = MpSize.Empty;
        public static IEnumerable<(int, double)> UniqueQueryWidths =>
            _props
            .Where(x => x.Value.UniqueWidth.HasValue && x.Value.QueryOffsetIdx >= 0)
            .Select(x => (x.Value.QueryOffsetIdx, x.Value.UniqueWidth.Value));

        public static IEnumerable<(int, double)> UniqueQueryHeights =>
            _props
            .Where(x => x.Value.UniqueHeight.HasValue && x.Value.QueryOffsetIdx >= 0)
            .Select(x => (x.Value.QueryOffsetIdx, x.Value.UniqueHeight.Value));

        public static IEnumerable<(int, MpSize)> UniqueQuerySizes =>
            _props
            .Where(x => (x.Value.UniqueWidth.HasValue || x.Value.UniqueHeight.HasValue) && x.Value.QueryOffsetIdx >= 0)
            .Select(x =>
                (x.Key,
                new MpSize(
                    x.Value.UniqueWidth.HasValue ? x.Value.UniqueWidth.Value : DefQuerySize.Width,
                    x.Value.UniqueHeight.HasValue ? x.Value.UniqueHeight.Value : DefQuerySize.Height)));

        public static MpSize GetTotalUniqueSizeBeforeIdx(int idx, MpSize defSize) {
            MpSize total = new MpSize();
            if (idx == 0) {
                return total;
            }

            for (int i = 0; i < _props.Count; i++) {
                if (i >= idx && idx >= 0) {
                    break;
                }
                total.Width += _props[i].UniqueWidth.HasValue ? (_props[i].UniqueWidth.Value - defSize.Width) : 0;
                total.Height += _props[i].UniqueHeight.HasValue ? (_props[i].UniqueHeight.Value - defSize.Height) : 0;
            }
            return total;
        }
        public static void AddOrReplaceUniqueWidth_ById(int ciid, double uniqueWidth, int idx) {
            GetProps(ciid, true, idx).UniqueWidth = uniqueWidth;
        }
        public static void AddOrReplaceUniqueHeight_ById(int ciid, double uniqueHeight, int idx) {
            GetProps(ciid, true, idx).UniqueHeight = uniqueHeight;
        }

        public static void RemoveUniqueSize_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.UniqueWidth = null;
                pp.UniqueHeight = null;
                CleanupProps();
            }
        }

        public static void RemoveUniqueWidth_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.UniqueWidth = null;
                CleanupProps();
            }
        }
        public static void RemoveUniqueHeight_ById(int ciid, int idx) {
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
                pp.UniqueHeight = null;
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

        public static bool TryGetUniqueHeight_ByOffsetIdx(int idx, out double uheight) {
            if (_props.FirstOrDefault(x => x.Value.QueryOffsetIdx == idx) is KeyValuePair<int, MpAvPersistentClipTileProperties> kvp &&
                kvp.Value is MpAvPersistentClipTileProperties pctp &&
                pctp.UniqueHeight.HasValue) {
                uheight = pctp.UniqueHeight.Value;
                return true;
            }
            uheight = 0;
            return false;
        }

        public static bool TryGetUniqueWidth_ById(int ciid, int idx, out double uniqueWidth) {
            uniqueWidth = 0;
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.UniqueWidth.HasValue) {
                uniqueWidth = pp.UniqueWidth.Value;
                return true;
            }
            return false;
        }

        public static bool TryGetUniqueHeight_ById(int ciid, int idx, out double uniqueHeight) {
            uniqueHeight = 0;
            if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.UniqueHeight.HasValue) {
                uniqueHeight = pp.UniqueHeight.Value;
                return true;
            }
            return false;
        }

        public static bool HasUniqueSize(int ciid, int idx) {
            return IsTileHaveUniqueHeight(ciid, idx) || IsTileHaveUniqueWidth(ciid, idx);
        }

        public static bool IsTileHaveUniqueWidth(int ciid, int idx) {
            return
                GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.UniqueWidth.HasValue;
        }
        public static bool IsTileHaveUniqueHeight(int ciid, int idx) {
            return
                GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp &&
                pp.UniqueHeight.HasValue;
        }

        public static void ClearPersistentQuerySizes() {
            ClearPersistentQueryWidths();
            ClearPersistentQueryHeights();
        }

        public static void ClearPersistentQueryWidths() {
            _props
                .Where(x => x.Value.QueryOffsetIdx >= 0)
                .ForEach(x => x.Value.UniqueWidth = null);
            CleanupProps();
        }

        public static void ClearPersistentQueryHeights() {
            _props
                .Where(x => x.Value.QueryOffsetIdx >= 0)
                .ForEach(x => x.Value.UniqueHeight = null);
            CleanupProps();
        }
        #endregion

        //#region IsTransactionPaneOpen

        //public static void AddPersistentIsTransactionPaneOpenTile_ById(int ciid, int idx) {
        //    if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
        //        pp.IsTransactionPaneOpen = false;
        //    }
        //}
        //public static void RemovePersistentIsTransactionPaneOpenTile_ById(int ciid, int idx) {
        //    if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
        //        pp.IsTransactionPaneOpen = true;
        //        CleanupProps();
        //    }
        //}

        //public static bool IsPersistentTileTransactionPaneOpen_ById(int ciid, int idx) {
        //    return GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp && !pp.IsTransactionPaneOpen;
        //}

        //#endregion

        //#region SelectedTransNodeGuid

        //public static void AddPersistentSelectedTransNodeGuidTile_ById(int ciid, int idx, string node_guid) {
        //    if (GetProps(ciid, true, idx) is MpAvPersistentClipTileProperties pp) {
        //        pp.SelectedTransNodeGuid = node_guid;
        //    }
        //}
        //public static void RemovePersistentSelectedTransNodeGuidTile_ById(int ciid, int idx) {
        //    if (GetProps(ciid, false, idx) is MpAvPersistentClipTileProperties pp) {
        //        pp.SelectedTransNodeGuid = null;
        //        CleanupProps();
        //    }
        //}

        //public static string GetPersistentSelectedTransNodeGuid_ById(int ciid, int idx) {
        //    if (GetProps(ciid, false, idx) is not MpAvPersistentClipTileProperties pp) {
        //        return null;
        //    }
        //    return pp.SelectedTransNodeGuid;
        //}

        //#endregion

        #endregion

        #region Private Methods

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
        #endregion
    }
}
