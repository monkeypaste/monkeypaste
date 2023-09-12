using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvPresetParamHostViewModelBase<P, C> :
        MpAvTreeSelectorViewModelBase<P, C>,
        MpIParameterHostViewModel
        where P : class
        where C : MpAvViewModelBase, MpISelectableViewModel, MpITreeItemViewModel {
        #region Interfaces
        public abstract int IconId { get; }
        public abstract MpPluginFormat PluginFormat { get; }
        public abstract MpParameterHostBaseFormat ComponentFormat { get; }
        public abstract MpParameterHostBaseFormat BackupComponentFormat { get; }
        public abstract string PluginGuid { get; }
        public abstract MpIPluginComponentBase PluginComponent { get; }
        #endregion

        #region Properties

        #endregion
        #region Constructors

        public MpAvPresetParamHostViewModelBase() : base(null) { }

        public MpAvPresetParamHostViewModelBase(P parent) : base(parent) { }
        #endregion
    }
}
