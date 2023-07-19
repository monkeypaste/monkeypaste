using MonkeyPaste.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using MpResources = MonkeyPaste.Avalonia.Resources.Locales.Resources;

namespace MonkeyPaste.Avalonia {
    public class MpAvGestureProfileCollectionViewModel : MpViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvGestureProfileItemViewModel> Items { get; set; }

        public MpAvGestureProfileItemViewModel HoverItem =>
            Items.FirstOrDefault(x => x.IsHovering);

        #endregion
        #region Appearance
        public string Caption =>
            HoverItem == null ?
            "Keyboard shortcuts can be changed at anytime from the 'Settings->Shortcuts' menu. " :
            HoverItem.DescriptionText;

        #endregion
        #endregion

        #region Constructors

        public MpAvGestureProfileCollectionViewModel() : base() {
            Items =
                new ObservableCollection<MpAvGestureProfileItemViewModel>(
                    Enum.GetNames(typeof(MpShortcutRoutingProfileType))
                        .Where(x => x != MpShortcutRoutingProfileType.None.ToString() && x != MpShortcutRoutingProfileType.Custom.ToString())
                        .Select(x =>
                            new MpAvGestureProfileItemViewModel(this, x.ToEnum<MpShortcutRoutingProfileType>())));

            OnPropertyChanged(nameof(Items));
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
