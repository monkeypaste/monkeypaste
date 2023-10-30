using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WindowStartupLocation = Avalonia.Controls.WindowStartupLocation;

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
    public interface MpAvIWebPageViewModel : MpIViewModel, MpIPassiveAsyncObject {
        string CurrentUrl { get; }
        ICommand ReloadCommand { get; }
        object ReloadCommandParameter { get; }
    }
    public class MpAvHelpViewModel :
        MpAvViewModelBase,
        MpAvIWebPageViewModel,
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

        #region MpAvIWebPageViewModel Implementatiosn
        ICommand MpAvIWebPageViewModel.ReloadCommand =>
            NavigateToHelpLinkCommand;
        object MpAvIWebPageViewModel.ReloadCommandParameter =>
            LastLinkType;

        #endregion

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

        public bool IsHelpSettingsTabVisible =>
            false;

        public string CurrentUrl { get; set; }

        public MpHelpLinkType LastLinkType { get; private set; }

        #endregion

        #region Model

        Dictionary<MpHelpLinkType, string> OnlineHelpUriLookup => new() {
            {MpHelpLinkType.None, $"{MpServerConstants.DOCS_BASE_URL}/welcome" },
            {MpHelpLinkType.ContentLimits, $"{MpServerConstants.DOCS_BASE_URL}/account/#content-limits" },
            {MpHelpLinkType.Collections, $"{MpServerConstants.DOCS_BASE_URL}/collections/" },
            {MpHelpLinkType.Tags, $"{MpServerConstants.DOCS_BASE_URL}/collections/tags" },
            {MpHelpLinkType.Groups, $"{MpServerConstants.DOCS_BASE_URL}/collections/groups" },
            {MpHelpLinkType.Filters, $"{MpServerConstants.DOCS_BASE_URL}/collections/filters" },
            {MpHelpLinkType.Trash, $"{MpServerConstants.DOCS_BASE_URL}/collections/trash" },
        };

        #endregion

        #endregion

        #region Constructors
        public MpAvHelpViewModel() : base(null) {
            PropertyChanged += MpAvHelpViewModel_PropertyChanged;
            CurrentUrl = GetHelpUrl(MpHelpLinkType.None);
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void MpAvHelpViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //}
        }

        private MpAvWindow CreateHelpWindow() {
            var w = new MpAvWindow() {
                ShowInTaskbar = true,
                Width = 1000,
                Height = 620,
                ShowActivated = true,
                Title = UiStrings.SettingsHelpTabLabel.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("QuestionMarkImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = this,
                Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeWhiteColor.ToString()),
                Content = new MpAvWebPageView()
            };
            w.Classes.Add("fadeIn");
            return w;
        }

        private void InitHelpStyle() {
            if (MpAvWindowManager.LocateWindow(this) is not MpAvWindow w ||
                w.GetVisualDescendant<MpAvWebView>() is not MpAvWebView wv) {
                return;
            }
            MpAvDocusaurusHelpers.LoadMainOnlyAsync(wv).FireAndForgetSafeAsync();
        }

        private string GetHelpUrl(MpHelpLinkType hlt) {
            if (!OnlineHelpUriLookup.ContainsKey(hlt)) {
                hlt = MpHelpLinkType.None;
            }
            string url = OnlineHelpUriLookup[hlt];
            return url + MpAvDocusaurusHelpers.GetThemeUrlAttrb(MpAvPrefViewModel.Instance.IsThemeDark);
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

                //ensure reload
                CurrentUrl = MpUrlHelpers.BLANK_URL;
                CurrentUrl = GetHelpUrl(hlt);

                InitHelpStyle();
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
