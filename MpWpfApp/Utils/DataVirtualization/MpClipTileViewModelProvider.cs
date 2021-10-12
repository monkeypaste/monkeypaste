using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpClipTileViewModelProvider : MpIItemsProvider<MpClipTileViewModel> {
        private MpCopyItemProvider _modelProvider;
        private int _pageSize;


        public MpClipTileViewModelProvider(int pageSize) {
            _pageSize = pageSize;
            _modelProvider = MpCopyItemProvider.Instance;
        }

        public void SetQueryInfo(MpQueryInfo info) {
            _modelProvider.SetQueryInfo(info);
        }

        public int FetchCount() {
            return _modelProvider.FetchCount();
        }

        public IList<MpClipTileViewModel> FetchRange(int startIndex, int count) {
            var ml = _modelProvider.FetchRange(startIndex, count);
            return ml.AsParallel().Select(x=>MpClipTrayViewModel.Instance.CreateClipTileViewModel(x)).ToList();
        }
    }
}
