using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;

namespace MpWpfApp {
    public class MpPluginComponentCollectionViewModel : 
        MpViewModelBase,
        MpISingletonViewModel<MpPluginComponentCollectionViewModel>,
        MpISidebarItemViewModel,
        MpITreeItemViewModel {

        #region Properties

        #region View Models
        public MpAnalyticItemCollectionViewModel AnalyzerCollectionViewModel => MpAnalyticItemCollectionViewModel.Instance;

        #endregion


        #region MpISingletonViewModel Implementation

        private static MpPluginComponentCollectionViewModel _instance;
        public static MpPluginComponentCollectionViewModel Instance => _instance ?? (_instance = new MpPluginComponentCollectionViewModel());

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultTagTreePanelWidth;
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultTagTreePanelWidth;
        public bool IsSidebarVisible { get; set; }
        public MpISidebarItemViewModel NextSidebarItem { get; }
        public MpISidebarItemViewModel PreviousSidebarItem { get; }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public ObservableCollection<MpITreeItemViewModel> Children { get; }

        #endregion

        #endregion

        #region Constructors

        public MpPluginComponentCollectionViewModel() : base() { }

        #endregion


        #region Public Methods

        public async Task Init() {
            await Task.Delay(1);

        }



        #endregion
    }
}
