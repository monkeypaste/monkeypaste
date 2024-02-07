using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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
        Trash,
        Plugins,
        Account
    }

    public class MpAvHelpViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIActiveWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables

        #endregion

        #region Constants
        public const string HELP_ICON_KEY = "ReadImage";
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

        #region Appearance

        public string WindowTitle {
            get {
                return string.Format(UiStrings.HelpWindowTitle, WebViewTitle).ToWindowTitleText();
            }
        }

        public string WebViewTitle { get; set; }

        #endregion

        #region State


        public string CurrentUrl =>
            MpAvDocusaurusHelpers.GetCustomUrl(
                url: OnlineHelpUriLookup[LastLinkType],
                hideNav: true,
                hideSidebars: false,
                isDark: MpAvPrefViewModel.Instance.IsThemeDark);

        public MpHelpLinkType LastLinkType { get; private set; } = MpHelpLinkType.None;

        #endregion

        #region Model

        Dictionary<MpHelpLinkType, string> OnlineHelpUriLookup => new() {
            {MpHelpLinkType.None, $"{MpServerConstants.DOCS_BASE_URL}/welcome" },
            {MpHelpLinkType.Plugins, $"{MpServerConstants.DOCS_BASE_URL}/plugins" },
            {MpHelpLinkType.Account, $"{MpServerConstants.DOCS_BASE_URL}/account" },
            {MpHelpLinkType.ContentLimits, $"{MpServerConstants.DOCS_BASE_URL}/account/#limits" },
            {MpHelpLinkType.Collections, $"{MpServerConstants.DOCS_BASE_URL}/collections/" },
            {MpHelpLinkType.Tags, $"{MpServerConstants.DOCS_BASE_URL}/collections/tags" },
            {MpHelpLinkType.Groups, $"{MpServerConstants.DOCS_BASE_URL}/collections/groups" },
            {MpHelpLinkType.Filters, $"{MpServerConstants.DOCS_BASE_URL}/collections/filters" },
            {MpHelpLinkType.Trash, $"{MpServerConstants.DOCS_BASE_URL}/collections/trash" }
        };

        #endregion

        #endregion

        #region Constructors
        public MpAvHelpViewModel() : base(null) {
            PropertyChanged += MpAvHelpViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        private void MpAvHelpViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //}
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ThemeChanged:
                    OnPropertyChanged(nameof(CurrentUrl));
                    break;
            }
        }

        private MpAvWindow CreateHelpWindow() {
            var w = new MpAvWindow() {
                ShowInTaskbar = true,
                Width = 1000,
                Height = 620,
                ShowActivated = true,
                Title = UiStrings.SettingsHelpTabLabel.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert(HELP_ICON_KEY, typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = this,
                Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString()),
                Content = new MpAvWebPageView()
            };
            w.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowTitle)
                });

            if (w.Content is MpAvWebPageView wpv) {
                wpv.Bind(
                        MpAvWebPageView.AddressProperty,
                        new Binding() {
                            Source = this,
                            Path = nameof(CurrentUrl)
                        });
                if (wpv.FindControl<MpAvWebView>("WebView") is { } wv) {
                    wv.Bind(
                        MpAvWebView.DocumentTitleProperty,
                        new Binding() {
                            Source = this,
                            Path = nameof(WebViewTitle)
                        });
                }
            }

            w.Classes.Add("fadeIn");
            return w;
        }

        #endregion

        #region Commands

        public ICommand NavigateToHelpLinkCommand => new MpCommand<object>(
            (args) => {
                MpHelpLinkType hlt = MpHelpLinkType.None;
                if (args is MpHelpLinkType argLink) {
                    hlt = argLink;
                } else if (args is string argStr) {
                    hlt = argStr.ToEnum<MpHelpLinkType>();
                }

                LastLinkType = hlt;
                // open/activate settings window and select help tab...
                if (IsWindowOpen) {
                    IsWindowActive = true;
                } else if (Mp.Services.PlatformInfo.IsDesktop) {
                    //var sw = CreateHelpWindow();
                    //sw.Show();
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(CurrentUrl);
                    MpMessenger.SendGlobal(MpMessageType.HelpWindowOpened);
                }
                OnPropertyChanged(nameof(CurrentUrl));

                MpConsole.WriteLine($"Help navigating to type '{hlt}' at url '{CurrentUrl}'");
            });

        public ICommand NavigateToContextualHelpCommand => new MpCommand(
            () => {
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
                NavigateToHelpLinkCommand.Execute(anchor_help_type);
            },
            () => {
                return MpAvWindowManager.IsAnyActive;
            });

        #endregion
    }
}
