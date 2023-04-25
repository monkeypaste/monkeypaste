using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginDependencyViewModel :
        MpViewModelBase<MpAvPluginItemViewModel>,
        MpILabelTextViewModel,
        MpITreeItemViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpILabelTextViewModel Implementation

        private string _label_text;
        public string LabelText {
            get {
                if (!string.IsNullOrEmpty(_label_text)) {
                    return _label_text;
                }
                return $"{DependencyName}{DependencyVersion}";
            }
            set {
                if (LabelText != value) {
                    _label_text = value;
                    OnPropertyChanged(nameof(LabelText));
                }
            }
        }
        #endregion

        #region MpITreeItemViewModel Implementation

        public MpITreeItemViewModel ParentTreeItem { get; }
        public IEnumerable<MpITreeItemViewModel> Children =>
            Items;
        public bool IsExpanded { get; set; }
        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvPluginDependencyViewModel> Items { get; set; } = new ObservableCollection<MpAvPluginDependencyViewModel>();
        #endregion

        #region Apppearance

        #endregion

        #region Model
        public MpPluginDependencyType DependencyType {
            get {
                if (PluginDependency == null) {
                    return MpPluginDependencyType.None;
                }
                return PluginDependency.type;
            }
        }
        public string DependencyName {
            get {
                if (PluginDependency == null) {
                    return string.Empty;
                }
                return PluginDependency.name;
            }
        }

        public string DependencyVersion {
            get {
                if (PluginDependency == null) {
                    return "1.0";
                }
                return PluginDependency.version;
            }
        }
        public MpPluginDependency PluginDependency { get; set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvPluginDependencyViewModel() : this(null) { }
        public MpAvPluginDependencyViewModel(MpAvPluginItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpPluginDependency pd) {
            IsBusy = true;
            await Task.Delay(1);
            PluginDependency = pd;

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
