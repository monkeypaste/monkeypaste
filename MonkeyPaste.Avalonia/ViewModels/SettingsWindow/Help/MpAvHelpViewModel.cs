using Avalonia.Controls;
using Avalonia.VisualTree;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
    public class MpAvHelpViewModel : MpAvViewModelBase {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvHelpViewModel _instance;
        public static MpAvHelpViewModel Instance => _instance ?? (_instance = new MpAvHelpViewModel());
        #endregion

        #region Properties

        #region State

        public bool IsHelpEnabled =>
            true;

        public string InitialUrl { get; set; }
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
        public MpAvHelpViewModel() : base(null) { }
        #endregion

        #region Public Methods
        #endregion

        #region Commands

        public MpIAsyncCommand<object> NavigateToHelpLinkCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpHelpLinkType hlt = MpHelpLinkType.None;
                if (args is MpHelpLinkType argLink) {
                    hlt = argLink;
                }
                // open/activate settings window and select help tab...
                await MpAvSettingsViewModel.Instance
                    .ShowSettingsWindowCommand.ExecuteAsync(MpSettingsTabType.Help);
                if (MpAvWindowManager.LocateVisual<MpAvHelpView>(this) is MpAvHelpView hv &&
                    hv.GetVisualDescendant<WebView>() is WebView hwv) {
                    hwv.Navigate(OnlineHelpUriLookup[hlt]);
                }
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
