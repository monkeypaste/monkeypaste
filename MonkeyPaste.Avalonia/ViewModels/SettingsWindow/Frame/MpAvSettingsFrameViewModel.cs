using Avalonia.Threading;
using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsFrameViewModel :
        MpAvViewModelBase,
        MpILabelTextViewModel,
        MpIIconResource,
        MpISelectableViewModel,
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
                    case MpSettingsFrameType.Register:
                        return "GlobeImage";
                    case MpSettingsFrameType.Login:
                        return "WebImage";
                    case MpSettingsFrameType.Status:
                        return "UserImage";
                    case MpSettingsFrameType.Theme:
                        return "BrushImage";
                    case MpSettingsFrameType.Fonts:
                        return "FontImage";
                    case MpSettingsFrameType.Sound:
                        return "SpeakerImage";
                    case MpSettingsFrameType.Taskbar:
                        return "Dots2x2Image";
                    case MpSettingsFrameType.Window:
                        return "AppFrameImage";
                    case MpSettingsFrameType.Hints:
                        return "InfoBwImage";
                    case MpSettingsFrameType.International:
                        return "GlobeImage";
                    case MpSettingsFrameType.Limits:
                        return "SlidersImage";
                    case MpSettingsFrameType.Tracking:
                        return "ClipboardImage";
                    case MpSettingsFrameType.Startup:
                        return "ClockArrowImage";
                    case MpSettingsFrameType.Search:
                        return "SearchImage";
                    case MpSettingsFrameType.Content:
                        return "BananaBwImage";
                    case MpSettingsFrameType.DragAndDrop:
                        return "DragAndDropImage";
                    case MpSettingsFrameType.Shortcuts:
                        return "JoystickImage";
                    case MpSettingsFrameType.System:
                        return "AppShellImage";
                    case MpSettingsFrameType.Password:
                        return "LockImage";
                    default:
                        return string.Empty;
                }
            }
        }
        #endregion

        #region MpILabelTextViewModel Implementation
        public string LabelText =>
            FrameType.EnumToUiString();

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected {
            get => SelectedItem != null;
            set {
                if (IsSelected != value) {
                    if (value && SelectedItem == null &&
                        Items.Any()) {
                        Items.FirstOrDefault().IsSelected = true;
                    } else if (!value && SelectedItem != null) {
                        SelectedItem = null;
                    }
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public DateTime LastSelectedDateTime { get; set; }

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
            FilteredItems;
        //public MpAvParameterViewModelBase SelectedItem { get; set; }
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

        public IEnumerable<MpAvParameterViewModelBase> FilteredItems =>
            Items
            .Where(x =>
            (x as MpIFilterMatch).IsFilterMatch(MpAvSettingsViewModel.Instance.FilterText) &&
            !MpAvSettingsViewModel.Instance.HiddenParamIds.Contains(x.ParamId.ToStringOrEmpty()));

        public IList<MpAvParameterViewModelBase> Items { get; set; }

        public MpAvParameterViewModelBase SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsSelected = x == value);
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        #endregion

        #region State


        public MpSettingsFrameType FrameType { get; set; } = MpSettingsFrameType.None;


        public bool IsVisible { get; set; } = true;
        #endregion

        #region Layout


        #endregion

        #region Appearance
        public int SortOrderIdx =>
            (int)FrameType;

        #endregion
        #endregion

        #region Constructors
        public MpAvSettingsFrameViewModel() : this(MpSettingsFrameType.None) {
        }

        public MpAvSettingsFrameViewModel(MpSettingsFrameType sft) : base(null) {
            PropertyChanged += MpAvPreferenceFrameViewModel_PropertyChanged;
            FrameType = sft;
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
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
                case nameof(SelectedItem):

                    break;
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
                            MpAvPrefViewModel.Instance.SetPropertyValue(pvm.ParamId.ToString(), pvm.CurrentTypedValue);
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
