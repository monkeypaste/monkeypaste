using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileContentDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            string key = "ContentWebViewTemplate";
            bool needs_fallback = !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled;

#if CEFNET_WV
            if(!MpAvCefNetApplication.IsCefNetLoaded) {
                needs_fallback = true;
            }
#endif
            if (needs_fallback &&
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
