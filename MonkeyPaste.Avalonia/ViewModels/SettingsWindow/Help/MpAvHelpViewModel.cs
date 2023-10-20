using Avalonia.Controls;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public enum MpHelpLinkType {
        None = 0,
        ContentLimits,
        Collections,
        Tags,
        Groups,
        Filters,
        Trash
    }
    public class MpAvHelpViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvHelpViewModel _instance;
        public static MpAvHelpViewModel Instance => _instance ?? (_instance = new MpAvHelpViewModel());
        #endregion

        #region Interfaces

        #region MpIWindowViewModel Implementatiosn
        public MpWindowType WindowType =>
            MpWindowType.Help;

        public bool IsWindowOpen { get; set; }

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation 
        public bool WantsTopmost =>
            true;

        #endregion

        #region MpIActiveWindowViewModel Implementation
        public bool IsWindowActive { get; set; }

        #endregion

        #endregion

        #region Properties

        #region State
        public string LoadErrorInfo { get; set; }

        public bool IsHelpSettingsTabVisible =>
            false;

        public string CurrentUrl { get; set; }

        public MpHelpLinkType LastLinkType { get; private set; }

        public bool IsOffline =>
            !string.IsNullOrEmpty(LoadErrorInfo);

        #endregion

        #region Model

        Dictionary<MpHelpLinkType, string> OnlineHelpUriLookup => new() {
            {MpHelpLinkType.None, @"https://www.monkeypaste.com/docs/welcome" },
            {MpHelpLinkType.ContentLimits, @"https://www.monkeypaste.com/docs/account/#content-limits" },
            {MpHelpLinkType.Collections, @"https://www.monkeypaste.com/docs/collections/" },
            {MpHelpLinkType.Tags, @"https://www.monkeypaste.com/docs/collections/tags" },
            {MpHelpLinkType.Groups, @"https://www.monkeypaste.com/docs/collections/groups" },
            {MpHelpLinkType.Filters, @"https://www.monkeypaste.com/docs/collections/filters" },
            {MpHelpLinkType.Trash, @"https://www.monkeypaste.com/docs/collections/trash" },
        };

        #endregion

        #endregion

        #region Constructors
        public MpAvHelpViewModel() : base(null) {
            PropertyChanged += MpAvHelpViewModel_PropertyChanged;
            CurrentUrl = OnlineHelpUriLookup[MpHelpLinkType.None];
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void MpAvHelpViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(LoadErrorInfo):
                    OnPropertyChanged(nameof(IsOffline));
                    break;
            }
        }

        private MpAvWindow CreateHelpWindow() {
            var w = new MpAvWindow() {
                ShowInTaskbar = true,
                Width = 1000,
                Height = 620,
                Title = UiStrings.SettingsHelpTabLabel.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("QuestionMarkImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = this,
                Content = new MpAvHelpView()
            };
            w.Classes.Add("fadeIn");

            void W_Opened(object sender, EventArgs e) {
                w.Activate();
            }
            void W_Closed(object sender, EventArgs e) {
                w.Opened -= W_Opened;
                w.Closed -= W_Closed;
            }

            w.Opened += W_Opened;
            w.Closed += W_Closed;
            return w;
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> NavigateToHelpLinkCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpHelpLinkType hlt = MpHelpLinkType.None;
                if (args is MpHelpLinkType argLink) {
                    hlt = argLink;
                }
                LastLinkType = hlt;
                // open/activate settings window and select help tab...
                if (IsHelpSettingsTabVisible) {
                    await MpAvSettingsViewModel.Instance
                        .ShowSettingsWindowCommand.ExecuteAsync(MpSettingsTabType.Help);
                } else {
                    if (IsWindowOpen) {
                        IsWindowActive = true;
                    } else if (Mp.Services.PlatformInfo.IsDesktop) {
                        var sw = CreateHelpWindow();
                        sw.ShowChild();
                        MpMessenger.SendGlobal(MpMessageType.HelpWindowOpened);
                    }
                }

                MpConsole.WriteLine($"Help navigating to type '{hlt}' at url '{OnlineHelpUriLookup[hlt]}'");

                CurrentUrl = OnlineHelpUriLookup[hlt];

                //if (MpAvWindowManager.LocateVisual<MpAvHelpView>(this) is MpAvHelpView hv &&
                //    hv.GetVisualDescendant<WebView>() is WebView hwv) {
                //    hwv.Navigate(OnlineHelpUriLookup[hlt]);
                //}
            });

        public MpIAsyncCommand NavigateToContextualHelpCommand => new MpAsyncCommand(
            async () => {
                MpHelpLinkType anchor_help_type = MpHelpLinkType.None;

                if (MpAvFocusManager.Instance.FocusElement is Control fc) {
                    // this may need manually tweaking but trying to prefer
                    // descendants to deal w/ focus being within control templates (like ListBoxItem)
                    // so check down then up for help anchors
                    if (fc.GetSelfAndVisualDescendants().FirstOrDefault(x => MpAvHelpAnchorExtension.GetIsEnabled(x)) is Control desc_anchor_c) {
                        anchor_help_type = MpAvHelpAnchorExtension.GetLinkType(desc_anchor_c);
                    } else if (fc.GetSelfAndVisualAncestors().FirstOrDefault(x => MpAvHelpAnchorExtension.GetIsEnabled(x)) is Control anc_anchor_c) {
                        anchor_help_type = MpAvHelpAnchorExtension.GetLinkType(anc_anchor_c);
                    }

                }
                await NavigateToHelpLinkCommand.ExecuteAsync(anchor_help_type);
            },
            () => {
                return MpAvWindowManager.IsAnyActive;
            });

        #endregion
    }
}
