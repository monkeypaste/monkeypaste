﻿using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvWelcomeOptionGroupViewModel : MpAvViewModelBase<MpAvWelcomeNotificationViewModel> {
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
        public IList<MpAvWelcomeOptionItemViewModel> Items { get; set; }

        public MpAvWelcomeOptionItemViewModel SelectedItem {
            get => Items.FirstOrDefault(x => x.IsChecked);
            set {
                if (SelectedItem != value) {
                    Items.ForEach(x => x.IsChecked = x == value);
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        #endregion

        #region State
        public bool NeedsSkip { get; set; }
        public MpWelcomePageType WelcomePageType { get; private set; }
        public bool WasVisited { get; set; }

        public bool IsSelected =>
            Parent != null &&
            Parent.CurPageType == WelcomePageType;

        public bool IsGestureGroup =>
            WelcomePageType == MpWelcomePageType.ScrollWheel ||
            WelcomePageType == MpWelcomePageType.DragToOpen;

        #endregion

        #region Appearance

        public string Title { get; set; }
        public string Caption { get; set; }
        public object SplashIconSourceObj { get; set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvWelcomeOptionGroupViewModel() : this(null) { }
        public MpAvWelcomeOptionGroupViewModel(MpAvWelcomeNotificationViewModel parent) : base(parent) { }
        public MpAvWelcomeOptionGroupViewModel(MpAvWelcomeNotificationViewModel parent, MpWelcomePageType pageType) : this(parent) {
            PropertyChanged += MpAvWelcomeOptionGroupViewModel_PropertyChanged;
            WelcomePageType = pageType;
        }

        private void MpAvWelcomeOptionGroupViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsChecked)));
                    break;
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        public ICommand ProgressMarkerClickCommand => new MpCommand(
            () => {
                if (Parent == null) {
                    return;
                }
                Parent.SelectPageByMarkerCommand.Execute(WelcomePageType);
            });
        #endregion


    }
}
