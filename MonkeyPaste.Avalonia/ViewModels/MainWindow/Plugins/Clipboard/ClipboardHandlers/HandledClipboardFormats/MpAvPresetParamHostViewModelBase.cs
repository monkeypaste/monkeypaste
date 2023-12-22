using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPresetParamHostViewModelBase<P, C> :
        MpAvTreeSelectorViewModelBase<P, C>,
        MpIParameterHostViewModel
        where P : class
        where C : MpAvViewModelBase, MpISelectableViewModel, MpITreeItemViewModel {
        #region Interfaces
        public abstract int IconId { get; }
        public virtual string PluginGuid { get; set; }
        public virtual MpPluginWrapper PluginFormat {
            get {
                var kvp = MpPluginLoader.Plugins.FirstOrDefault(x => x.Value.guid == PluginGuid);
                if (kvp.IsDefault()) {
                    return null;
                }
                return kvp.Value;
            }
        }
        public abstract MpParameterHostBaseFormat ComponentFormat { get; }
        public abstract MpParameterHostBaseFormat BackupComponentFormat { get; }
        //public abstract MpIPluginComponentBase PluginComponent { get; }
        #endregion

        #region Properties

        #endregion
        #region Constructors

        public MpAvPresetParamHostViewModelBase() : base(null) { }

        public MpAvPresetParamHostViewModelBase(P parent) : base(parent) { }
        #endregion
    }
}
