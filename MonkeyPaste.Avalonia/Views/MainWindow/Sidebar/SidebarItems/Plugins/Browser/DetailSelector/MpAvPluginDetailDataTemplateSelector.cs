using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvPluginDetailDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            //if (param is not object[] paramParts ||
            //    paramParts[0] is not MpCopyItemType cit ||
            //    paramParts[1] is not MpAvClipTileViewModel ctvm) {
            //    return null;
            //}
            string key = "ContentWebViewTemplate";
            if ((!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled ||
                 !MpAvCefNetApplication.IsCefNetLoaded) &&
                param is MpAvClipTileViewModel ctvm) {
                ctvm.IsEditorLoaded = true;
                switch (ctvm.CopyItemType) {
                    default:
                    case MpCopyItemType.Text:
                        key = "PlainTextTemplate";
                        break;
                    case MpCopyItemType.Image:
                        key = "ImageTemplate";
                        break;
                    case MpCopyItemType.FileList:
                        key = "FileListTemplate";
                        break;
                }
            } else {

            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvClipTileViewModel;
        }

    }
}
