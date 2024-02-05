using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContentQueryPropertyPathHelpers {
        public static MpAvMenuItemViewModel GetContentPropertyRootMenu(
            ICommand selectCmd,
            IEnumerable<MpContentQueryPropertyPathType> hiddenPaths = null) {
            // NOTE command must use path type as parameter or ignore

            hiddenPaths = hiddenPaths ?? new List<MpContentQueryPropertyPathType>();

            var adv_mivm = new MpAvMenuItemViewModel() {
                HasLeadingSeparator = true,
                Header = UiStrings.ContentQueryAdvancedHeader,
                IconResourceKey = "CogImage",
                SubItems =
                    typeof(MpContentQueryPropertyGroupType)
                        .EnumToUiStrings()
                        .Skip(1)
                        .Select((x, idx) => new MpAvMenuItemViewModel() {
                            Header = x,
                            IconResourceKey = GetGroupIconResourceKey((MpContentQueryPropertyGroupType)idx + 1),
                            SubItems = new List<MpAvMenuItemViewModel>()
                        }).ToList()
            };

            var root_mivml = new List<MpAvMenuItemViewModel>();
            for (int i = 0; i < typeof(MpContentQueryPropertyPathType).Length(); i++) {
                MpContentQueryPropertyPathType pt = (MpContentQueryPropertyPathType)i;
                if (pt == MpContentQueryPropertyPathType.None ||
                    hiddenPaths.Contains(pt)) {
                    continue;
                }
                var pt_mivm = new MpAvMenuItemViewModel() {
                    Header = pt.EnumToUiString(),
                    Command = selectCmd,
                    CommandParameter = (int)pt,
                    Identifier = pt
                };

                MpContentQueryPropertyGroupType gt = GetPropertyGroupType(pt);
                if (gt == MpContentQueryPropertyGroupType.Root) {
                    pt_mivm.IconResourceKey = GetGroupIconResourceKey(gt);
                    root_mivml.Add(pt_mivm);
                } else {
                    //adv_mivm.SubItems[(int)gt - 1].SubItems.Add(pt_mivm);
                    if (adv_mivm.SubItems is List<MpAvMenuItemViewModel> adv_sub_mivml &&
                        adv_sub_mivml[(int)gt - 1] is MpAvMenuItemViewModel gt_mivm &&
                        gt_mivm.SubItems is List<MpAvMenuItemViewModel> gt_sub_mivml) {
                        gt_sub_mivml.Add(pt_mivm);
                    }
                }
            }

            root_mivml.Add(adv_mivm);
            return new MpAvMenuItemViewModel() {
                SubItems = root_mivml
            };

        }

        private static MpContentQueryPropertyGroupType GetPropertyGroupType(MpContentQueryPropertyPathType cqppt) {
            switch (cqppt) {
                case MpContentQueryPropertyPathType.ClipText:
                case MpContentQueryPropertyPathType.RawClipData:
                case MpContentQueryPropertyPathType.LastOutput:
                    return MpContentQueryPropertyGroupType.Root;
                case MpContentQueryPropertyPathType.ClipType:
                case MpContentQueryPropertyPathType.Title:
                    return MpContentQueryPropertyGroupType.Meta;
                case MpContentQueryPropertyPathType.SourceAppName:
                case MpContentQueryPropertyPathType.SourceAppPath:
                    return MpContentQueryPropertyGroupType.App;
                case MpContentQueryPropertyPathType.SourceUrl:
                case MpContentQueryPropertyPathType.SourceUrlDomain:
                case MpContentQueryPropertyPathType.SourceUrlTitle:
                    return MpContentQueryPropertyGroupType.Url;
                case MpContentQueryPropertyPathType.CopyDateTime:
                case MpContentQueryPropertyPathType.CopyCount:
                case MpContentQueryPropertyPathType.PasteCount:
                    return MpContentQueryPropertyGroupType.Statistics;
                case MpContentQueryPropertyPathType.SourceDeviceName:
                case MpContentQueryPropertyPathType.SourceDeviceType:
                    return MpContentQueryPropertyGroupType.Device;
                default:
                    MpDebug.Break($"Unhandled property path '{cqppt}'");
                    break;
            }
            return MpContentQueryPropertyGroupType.Root;
        }

        private static string GetGroupIconResourceKey(MpContentQueryPropertyGroupType gt) {
            switch (gt) {
                case MpContentQueryPropertyGroupType.App:
                    return "AppStoreImage";
                case MpContentQueryPropertyGroupType.Device:
                    return "DeviceImage";
                case MpContentQueryPropertyGroupType.Statistics:
                    return "SpreadsheetImage";
                case MpContentQueryPropertyGroupType.Root:
                    return "StarYellowImage";
                case MpContentQueryPropertyGroupType.Url:
                    return "GlobalImage";
                case MpContentQueryPropertyGroupType.Meta:
                    return "InfoImage";
                default:
                    return string.Empty;
            }
        }

        public static string ToQueryFragmentString(this MpContentQueryPropertyPathType cppt) {
            return string.Format(@"{{{0}}}", cppt.ToString());
        }
    }
}
