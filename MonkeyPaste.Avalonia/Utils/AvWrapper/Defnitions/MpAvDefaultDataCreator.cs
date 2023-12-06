using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDefaultDataCreator : MpIDefaultDataCreator {
        private static List<string[]> _defaultShortcutDefinitions;
        public static List<string[]> DefaultShortcutDefinitions {
            get {
                if (_defaultShortcutDefinitions == null) {

                    var ps = Mp.Services.PlatformShorcuts;
                    MpShortcutRoutingProfileType routingProfile = Mp.Services.WelcomeSetupInfo.DefaultRoutingProfileType;
                    MpRoutingType mw_routing = routingProfile.GetProfileBasedRoutingType(MpShortcutType.ToggleMainWindow);
                    MpRoutingType globalRouting = routingProfile.GetProfileBasedRoutingType(MpShortcutType.ToggleListenToClipboard);
                    _defaultShortcutDefinitions = new List<string[]>() {
                        // ORDER:
                        // guid,keyString,shortcutType,routeType, readOnly = false

                        // GLOBAL
                
                            new string[] {"5dff238e-770e-4665-93f5-419e48326f01","Caps Lock", MpShortcutType.ToggleMainWindow.ToString(), mw_routing.ToString(),"False","False"},
                            new string[] {"97e29b06-0ec4-4c55-a393-8442d7695038","Control+Shift+F1", MpShortcutType.ToggleListenToClipboard.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"892bf7d7-ba8e-4db1-b2ca-62b41ff6614c", "Control+Shift+F2", MpShortcutType.ToggleAutoCopyMode.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"a12c4211-ab1f-4b97-98ff-fbeb514e9a1c", "Control+Shift+F3", MpShortcutType.ToggleRightClickPasteMode.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"777367e6-c161-4e93-93e0-9bf12221f7ff", "Control+Shift+F5", MpShortcutType.ToggleAppendLineMode.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"a41aeed8-d4f3-47de-86c5-f9ca296fb103", "Control+Shift+F6", MpShortcutType.ToggleAppendInsertMode.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"9fa72a1b-2286-4907-bf70-37686aad009a", "Control+Shift+F7", MpShortcutType.ToggleAppendPreMode.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"9fa72a1b-2286-4907-bf70-37686aad009a", "Control+Shift+F8", MpShortcutType.ToggleAppendPaused.ToString(), globalRouting.ToString(),"False","False"},
                            new string[] {"746b41c6-4155-4281-808c-f8b8a278ce54", "Control+Shift+F9", MpShortcutType.ManuallyAddFromClipboard.ToString(), globalRouting.ToString(),"False","False"},

                            // APPLICATION
                            new string[] {"94e81589-fe2f-4e80-8940-ed066f0d9c27",ps.PasteKeys, MpShortcutType.PasteHere.ToString(), MpRoutingType.Internal.ToString(),"True"},
                            new string[] {"ee74dd92-d18b-46cf-91b7-3946ab55427c",ps.CopyKeys, MpShortcutType.CopySelection.ToString(), MpRoutingType.Internal.ToString(),"True"},
                            new string[] {"2acde1cc-c8e4-4675-8895-81712a6f0a36",ps.CutKeys, MpShortcutType.CutSelection.ToString(), MpRoutingType.Internal.ToString(),"True"},
                            new string[] {"cb807500-9121-4e41-80d3-8c3682ce90d9","Escape", MpShortcutType.HideMainWindow.ToString(), MpRoutingType.Internal.ToString(),"True"},
                            new string[] {"1d212ca5-fb2a-4962-8f58-24ed9a5d007d","Control+Enter", MpShortcutType.PasteSelectedItems.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"e94ca4f3-4c6e-40dc-8941-c476a81543c7","Delete", MpShortcutType.DeleteSelectedItems.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"7fe24929-6c9e-49c0-a880-2f49780dfb3a","Right", MpShortcutType.SelectNextColumnItem.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"ee657845-f1dc-40cf-848d-6768c0081670","Left", MpShortcutType.SelectPreviousColumnItem.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"674bae7f-0a60-4f17-ac2c-81d5c6c3d879","Down", MpShortcutType.SelectNextRowItem.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"b916ab44-d4bd-4d8b-ac4a-de947343bd5a","Up", MpShortcutType.SelectPreviousRowItem.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"6487f6ff-da0c-475b-a2ae-ef1484233de0","Control+I", MpShortcutType.AssignShortcut.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"118a2ca6-7021-47a0-8458-7ebc31094329","Control+Z", MpShortcutType.Undo.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"3980efcc-933b-423f-9cad-09e455c6824a","Control+Y", MpShortcutType.Redo.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"7a7580d1-4129-432d-a623-2fff0dc21408","Control+E", MpShortcutType.ToggleContentReadOnly.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"085338fb-f297-497a-abb7-eeb7310dc6f3","F2", MpShortcutType.Rename.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"e22faafd-4313-441a-b361-16910fc7e9d3","Control+D", MpShortcutType.Duplicate.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","Home", MpShortcutType.ScrollToHome.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","End", MpShortcutType.ScrollToEnd.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"9b0ca09a-5724-4004-98d2-f5ef8ae02055","Control+Up", MpShortcutType.WindowSizeUp.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"39a6194e-37e3-4d37-a9f4-254ed83157f2","Control+Down", MpShortcutType.WindowSizeDown.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"6cc03ef0-3b33-4b94-9191-0d751e6b7fb6","Control+Left", MpShortcutType.WindowSizeLeft.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"c4ac1629-cdf0-4075-94af-8f934b014452","Control+Right", MpShortcutType.WindowSizeRight.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"30c813a0-d466-4ae7-b75e-82680b4542fc","PageUp", MpShortcutType.PreviousPage.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"09df97ea-f786-48d9-9112-a60266df6586","PageDown", MpShortcutType.NextPage.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"a39ac0cb-41e4-47b5-b963-70e388dc156a","Control+H", MpShortcutType.FindAndReplaceSelectedItem.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"cb1ac03b-a20f-4911-bf4f-bc1a858590e3","Control+L", MpShortcutType.ToggleMainWindowLocked.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"d73204f5-fbed-4d87-9dca-6dfa8d8cba82","Control+K", MpShortcutType.ToggleFilterMenuVisible.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"49f44a89-e381-4d6a-bf8c-1090eb443f17","Control+Q", MpShortcutType.ExitApplication.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"31e0a078-e80b-4d31-b236-2a585d6352cf", "Control+,", MpShortcutType.ShowSettings.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"5143ed80-a50a-41b9-9979-5e00c084560d", "Control+P", MpShortcutType.TogglePinned.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"755a4d0e-d26a-42cf-89a3-6c5710bd2e4c", "Control+O", MpShortcutType.OpenInWindow.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"093d5f34-971c-4b87-b15b-aab682300900", "Control+Escape", MpShortcutType.ForceMinimizeMainWindow.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"728bfb06-1d96-441c-b710-efee383138be", "Control+G", MpShortcutType.ToggleAppendManualMode.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"2025c6eb-2e89-4fa7-a69b-37f0eb4c0281", "Control+Delete", MpShortcutType.PermanentlyDelete.ToString(), MpRoutingType.Internal.ToString()},
                            new string[] {"518a1cb0-ffc1-4c06-b2bc-30aa29237d67", "F1", MpShortcutType.OpenHelp.ToString(), MpRoutingType.Internal.ToString()},
                    };
                }
                return _defaultShortcutDefinitions;
            }
        }
        public async Task CreateDefaultDataAsync() {
            await InitDefaultTagsAsync();
            await InitDefaultShortcutsAsync();

            MpConsole.WriteLine(@"Created all default tables");
        }

        public async Task ResetShortcutsAsync() {
            var sl = await MpDataModelProvider.GetItemsAsync<MpShortcut>();
            await Task.WhenAll(sl.Select(x => x.DeleteFromDatabaseAsync()));
            await InitDefaultShortcutsAsync();
        }
        #region Private Methods

        private async Task InitDefaultTagsAsync() {
            bool tracked = true;
            bool synced = true;

            var default_tags = new object[] {
                // guid,name,color,treeIdx,pinIdx,track,sync,parentId,type
                new object[] { "df388ecd-f717-4905-a35c-a8491da9c0e3", MpReadOnlyTagType.Collections.ToString(), MpSystemColors.lemonchiffon2, 1,-1, tracked,synced, 0, MpTagType.Group},
                new object[] { "287140cc-2f9a-4bc6-a88d-c5b836f1a340", MpReadOnlyTagType.All.ToString(), MpSystemColors.blue1, 0,1, tracked,synced, MpTag.FiltersTagId, MpTagType.Link},
                new object[] { "54b61353-b031-4029-9bda-07f7ca55c123", MpReadOnlyTagType.Favorites.ToString(), MpSystemColors.yellow1, 1,-1,tracked,synced, MpTag.CollectionsTagId, MpTagType.Link},
                new object[] { "e62b8e5d-52a6-46f1-ac51-8f446916dd85", MpReadOnlyTagType.Filters.ToString(), MpSystemColors.forestgreen, 0,-1,tracked,synced, 0, MpTagType.Group},
                new object[] { "70db0f5c-a717-4bca-af2f-a7581aecc24d", MpReadOnlyTagType.Trash.ToString(), MpSystemColors.lightsalmon1, 2,-1,tracked,synced, 0, MpTagType.Link},
            };
            for (int i = 0; i < default_tags.Length; i++) {
                var t = (object[])default_tags[i];
                await MpTag.CreateAsync(
                    guid: t[0].ToString(),
                    tagName: t[1].ToString(),
                    hexColor: t[2].ToString(),
                    treeSortIdx: (int)t[3],
                    pinSortIdx: (int)t[4],
                    ignoreTracking: (bool)t[5],
                    ignoreSyncing: (bool)t[6],
                    parentTagId: (int)t[7],
                    tagType: (MpTagType)t[8]);
            }

            #region Recent

            var recent_tag = await MpTag.CreateAsync(
                    tagName: MpReadOnlyTagType.Today.ToString(),
                    hexColor: MpSystemColors.pink,
                    parentTagId: MpTag.FiltersTagId,
                    sortType: MpContentSortType.CopyDateTime,
                    pinSortIdx: 0,
                    isSortDescending: true,
                    tagType: MpTagType.Query);

            var recent_tag_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: recent_tag.Id,
                sortOrderIdx: 0,
                joinType: MpLogicalQueryType.And,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(
                    MpContentQueryBitFlags.Content |
                    MpContentQueryBitFlags.TextType |
                    MpContentQueryBitFlags.ImageType |
                    MpContentQueryBitFlags.FileType)).ToString());

            var recent_tag_created_datetime_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: recent_tag.Id,
                sortOrderIdx: 1,
                joinType: MpLogicalQueryType.And,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(
                        ",",
                        new[] {
                            (int)MpRootOptionType.History, //5
                            (int)MpTransactionType.Created, //1
                            (int)MpDateTimeOptionType.After, //3 
                            (int)MpDateAfterUnitType.Yesterday} //1
                        .Select(x => x.ToString())),
                matchValue: (0).ToString());

            var recent_tag_recreated_datetime_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: recent_tag.Id,
                sortOrderIdx: 2,
                joinType: MpLogicalQueryType.Or,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(
                        ",",
                        new[] {
                            (int)MpRootOptionType.History, //5
                            (int)MpTransactionType.Recreated, //1
                            (int)MpDateTimeOptionType.After, //3 
                            (int)MpDateAfterUnitType.Yesterday} //1
                        .Select(x => x.ToString())),
                matchValue: (0).ToString());

            #endregion

            #region Item Types

            var item_type_group_tag = await MpTag.CreateAsync(
                    tagName: MpReadOnlyTagType.Formats.ToString(),
                    hexColor: MpSystemColors.peachpuff4,
                    parentTagId: MpTag.FiltersTagId,
                    tagType: MpTagType.Group);

            #region Text

            var text_type_tag = await MpTag.CreateAsync(
                    tagName: MpReadOnlyTagType.Text.ToString(),
                    hexColor: MpSystemColors.darkgoldenrod3,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var text_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: text_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var text_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: text_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Text }));

            #endregion

            #region Image

            var image_type_tag = await MpTag.CreateAsync(
                    tagName: MpReadOnlyTagType.Images.ToString(),
                    hexColor: MpSystemColors.sienna2,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var image_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: image_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var image_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: image_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Image }));

            #endregion

            #region Files

            var file_type_tag = await MpTag.CreateAsync(
                    tagName: MpReadOnlyTagType.Files.ToString(),
                    hexColor: MpSystemColors.mediumorchid3,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var file_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: file_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var file_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: file_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Files }));

            #endregion

            #endregion
        }


        private async Task InitDefaultShortcutsAsync() {
            _defaultShortcutDefinitions = null;
            foreach (var defaultShortcut in DefaultShortcutDefinitions) {
                await MpShortcut.CreateAsync(
                    guid: defaultShortcut[0],
                    keyString: defaultShortcut[1],
                    shortcutType: defaultShortcut[2].ToEnum<MpShortcutType>(),
                    routeType: defaultShortcut[3].ToEnum<MpRoutingType>(),
                    isReadOnly: defaultShortcut.Length >= 5 ? bool.Parse(defaultShortcut[4]) : false,
                    isInternalOnly: defaultShortcut.Length >= 6 ? bool.Parse(defaultShortcut[5]) : true);
            }
        }

        #endregion
    }
}
