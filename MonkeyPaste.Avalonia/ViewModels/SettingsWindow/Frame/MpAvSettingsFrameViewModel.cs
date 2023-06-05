using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsFrameViewModel :
        MpViewModelBase,
        MpIFilterMatch,
        MpILabelTextViewModel,
        MpIIconResource,
        MpIParameterHostViewModel,
        MpAvIParameterCollectionViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIIconResource Implementation
        public object IconResourceObj {
            get {
                switch (FrameType) {
                    case MpSettingsFrameType.None:
                    default:
                        return null;
                    case MpSettingsFrameType.LookAndFeel:
                        return "EyeImage";
                }
            }
        }
        #endregion

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
        IEnumerable<MpAvParameterViewModelBase> MpAvIParameterCollectionViewModel.Items =>
            Items;
        public MpAvParameterViewModelBase SelectedItem { get; set; }
        public ICommand SaveCommand =>
            MpAvSettingsViewModel.Instance.SaveSettingsCommand;
        public ICommand CancelCommand =>
            MpAvSettingsViewModel.Instance.CancelSettingsCommand;
        public bool CanSaveOrCancel =>
            Items == null ? false : Items.Any(x => x.HasModelChanged);
        bool MpISaveOrCancelableViewModel.IsSaveCancelEnabled =>
            false;
        #endregion
        #endregion

        #region Properties
        #region View Models

        public IList<MpAvParameterViewModelBase> Items { get; set; }
        #endregion

        #region State

        public MpSettingsTabType TabType { get; set; }
        public MpSettingsFrameType FrameType { get; set; } = MpSettingsFrameType.None;
        public int SortOrderIdx { get; set; }

        public bool IsVisible { get; set; } = true;
        #endregion
        #endregion

        #region Constructors
        public MpAvSettingsFrameViewModel() : base(null) {
            PropertyChanged += MpAvPreferenceFrameViewModel_PropertyChanged;
            OnPropertyChanged(nameof(IconResourceObj));
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
                        to_save.ForEach(x => x.SaveCurrentValueCommand.Execute("skip model save"));
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
