using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvPreferenceFrameViewModel :
        MpViewModelBase<MpAvPreferencesMenuViewModel>,
        MpIFilterMatch,
        MpILabelTextViewModel,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpILabelTextViewModel Implementation
        public string LabelText { get; set; }
        #endregion


        #region MpIFilterMatch Implementation

        bool MpIFilterMatch.IsMatch(string filter) {
            throw new System.NotImplementedException();
        }
        #endregion

        #region MpIParameterHostViewModel Implementation
        int MpIParameterHostViewModel.IconId => 0;
        MpParameterHostBaseFormat MpIParameterHostViewModel.BackupComponentFormat =>
            null;
        string MpIParameterHostViewModel.PluginGuid => null;
        MpIPluginComponentBase MpIParameterHostViewModel.PluginComponent =>
            null;
        public MpPluginFormat PluginFormat { get; set; }
        public MpParameterHostBaseFormat ComponentFormat =>
            PluginFormat == null ?
                null :
                PluginFormat.headless;

        #endregion


        #region MpAvIParameterCollectionViewModel Implementation
        public IEnumerable<MpAvParameterViewModelBase> Items { get; set; }
        public MpAvParameterViewModelBase SelectedItem { get; set; }
        public ICommand SaveCommand =>
            MpAvSettingsWindowViewModel.Instance.SaveSettingsCommand;
        public ICommand CancelCommand =>
            MpAvSettingsWindowViewModel.Instance.CancelSettingsCommand;
        public bool CanSaveOrCancel =>
            Items == null ? false : Items.Any(x => x.HasModelChanged);
        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            false;
        #endregion
        #endregion

        #region Properties
        #region Model

        #endregion
        #endregion

        #region Constructors
        public MpAvPreferenceFrameViewModel(MpAvPreferencesMenuViewModel parent) : base(parent) {
            PropertyChanged += MpAvPreferenceFrameViewModel_PropertyChanged;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvPreferenceFrameViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CanSaveOrCancel):
                    if (!CanSaveOrCancel || IsBusy) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        IsBusy = true;
                        while (MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                            // when slider scrubbing get random errors writing to file
                            await Task.Delay(100);
                        }
                        var to_save = Items.Where(x => x.HasModelChanged).ToList();
                        foreach (var pvm in to_save) {
                            //while (MpPrefViewModel.Instance.IsSaving) {
                            //    await Task.Delay(100);
                            //}
                            MpPrefViewModel.Instance.SetPropertyValue(pvm.ParamId.ToString(), pvm.CurrentTypedValue);
                        }
                        IsBusy = false;
                    });
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
