using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public class MpIncrementalContentProvider {
        #region Private Variables

        private static IList<MpCopyItem> _lastResult;

        #endregion

        #region Properties
        public static List<MpIQueryInfo> QueryInfos { get; private set; } = new List<MpIQueryInfo>();

        public static MpIQueryInfo QueryInfo {
            get {
                //if(QueryInfos.Count > 0) {
                //    return QueryInfos.OrderBy(x => x.SortOrderIdx).ToList()[0];
                //}
                //return null;
                return MpPlatformWrapper.Services.QueryInfo;
            }
        }

        public static ObservableCollection<int> AllFetchedAndSortedCopyItemIds { get; private set; } = new ObservableCollection<int>();

        public static ObservableCollection<double> AllFetchedAndSortedTileOffsets { get; private set; } = new ObservableCollection<double>();

        public static int TotalTilesInQuery => AllFetchedAndSortedCopyItemIds.Count;

        public static double TotalTileWidth_WithoutMargins { get; set; }

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public static void Init() {
            QueryInfos.Clear();
            QueryInfos.Add(MpPlatformWrapper.Services.QueryInfo);
            //ResetQuery();
        }

        #endregion
    }
}
