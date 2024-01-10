using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPresetParamHostViewModelBase<P, C> :
        MpAvTreeSelectorViewModelBase<P, C>,
        MpIParameterHostViewModel
        where P : class
        where C : MpAvViewModelBase, MpISelectableViewModel, MpITreeItemViewModel {
        #region Interfaces
        public abstract int IconId { get; }
        public virtual string PluginGuid { get; set; }
        public virtual MpRuntimePlugin PluginFormat {
            get {
                if (MpPluginLoader.PluginGuidLookup.TryGetValue(PluginGuid, out var plugin)) {
                    return plugin;
                }
                return null;
            }
        }
        public abstract MpPresetParamaterHostBase ComponentFormat { get; }
        public abstract MpPresetParamaterHostBase BackupComponentFormat { get; }
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
