using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSideBarTreeCollectionViewModel : MpSingletonViewModel<MpSideBarTreeCollectionViewModel>, MpITreeItemViewModel {
        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; set; }

        public ObservableCollection<MpITreeItemViewModel> Children { get; set; } = new ObservableCollection<MpITreeItemViewModel>();

        public MpSideBarTreeCollectionViewModel() :base() { }

        public async Task Init() {
            await Task.Delay(1);

            Children.Clear();

            Children.Add(MpTagTrayViewModel.Instance);
            Children.Add(MpAnalyticItemCollectionViewModel.Instance);
        }
    }
}
