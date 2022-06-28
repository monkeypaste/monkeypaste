using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;

namespace MpWpfApp {
    public abstract class MpPluginItemViewModelBase : 
        MpViewModelBase,
        MpITreeItemViewModel {

        #region Statics


        #endregion
        #region Properties

        #region View Models
        public MpAnalyticItemCollectionViewModel AnalyzerCollectionViewModel => MpAnalyticItemCollectionViewModel.Instance;

        #endregion


        #region MpISingletonViewModel Implementation

        private static MpPluginItemViewModelBase _instance;
        public static MpPluginItemViewModelBase Instance => _instance ?? (_instance = new MpPluginItemViewModelBase());

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

        public MpPluginItemViewModelBase() : base() { }

        #endregion


        #region Public Methods

        public abstract Task InitAsync();


        #endregion
    }
}
