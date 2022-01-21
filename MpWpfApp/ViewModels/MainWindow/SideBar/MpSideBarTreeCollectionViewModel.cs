using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSideBarTreeCollectionViewModel : MpViewModelBase, MpISingletonViewModel<MpSideBarTreeCollectionViewModel>, MpITreeItemViewModel {
        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem { get; set; }

        public ObservableCollection<MpITreeItemViewModel> Children { get; set; } = new ObservableCollection<MpITreeItemViewModel>();

        private static MpSideBarTreeCollectionViewModel _instance;
        public static MpSideBarTreeCollectionViewModel Instance => _instance ?? (_instance = new MpSideBarTreeCollectionViewModel());


        public MpSideBarTreeCollectionViewModel() : base(null) { }

        public async Task Init() {
            await Task.Delay(1);

            Children.Clear();

            Children.Add(MpTagTrayViewModel.Instance);
            Children.Add(MpAnalyticItemCollectionViewModel.Instance);
        }
    }
}
