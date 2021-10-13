using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpClipTileViewModelProvider : MpIItemsProvider<MpClipTileViewModel> {
        public MpCopyItemProvider ModelProvider { get; set; }

        private int _pageSize;


        public MpClipTileViewModelProvider(int pageSize) {
            _pageSize = pageSize;
            ModelProvider = MpCopyItemProvider.Instance;
        }

        public void SetQueryInfo(MpQueryInfo info) {
            ModelProvider.SetQueryInfo(info);
        }

        public int FetchCount() {
            if(ModelProvider == null) {
                return 0;
            }
            return ModelProvider.FetchCount();
        }

        public async Task<int> FetchCountAsync() {
            if (ModelProvider == null) {
                return 0;
            }
            int count = await ModelProvider.FetchCountAsync();
            return count;
        }

        public IList<MpClipTileViewModel> FetchRange(int startIndex, int count) {
            if (ModelProvider == null) {
                return new List<MpClipTileViewModel>();
            }
            var ml = ModelProvider.FetchRange(startIndex, count);
            var vml = new List<MpClipTileViewModel>();
            foreach(var m in ml) {
                vml.Add(MpClipTrayViewModel.Instance.CreateClipTileViewModel(m));
            }
            return vml;
           // return ml.AsParallel().Select(x=>MpClipTrayViewModel.Instance.CreateClipTileViewModel(x)).ToList();
        }

        public async Task<IList<MpClipTileViewModel>> FetchRangeAsync(int startIndex, int count) {
            if (ModelProvider == null) {
                return new List<MpClipTileViewModel>();
            }
            var ml = await ModelProvider.FetchRangeAsync(startIndex, count);
            var vml = new List<MpClipTileViewModel>();
            foreach (var m in ml) {
                vml.Add(MpClipTrayViewModel.Instance.CreateClipTileViewModel(m));
            }
            return vml;
            // return ml.AsParallel().Select(x=>MpClipTrayViewModel.Instance.CreateClipTileViewModel(x)).ToList();
        }
    }
}
