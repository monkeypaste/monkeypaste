using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileContentDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpAvClipTileViewModel ctvm) {
                return null;
            }
            string key = "ContentWebViewTemplate";
            bool needs_fallback = !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled;

#if CEFNET_WV
            if(!MpAvCefNetApplication.IsCefNetLoaded) {
                needs_fallback = true;
            }
#elif SUGAR_WV
            needs_fallback = ctvm.IsContentReadOnly;
#endif
            if (needs_fallback) {
#if !SUGAR_WV
                ctvm.IsEditorLoaded = true; 
#endif
                switch (ctvm.CopyItemType) {
                    default:
                    case MpCopyItemType.Text:
#if SUGAR_WV
                        key = "CompositeWebViewTemplate";
#else
                        key = "PlainTextTemplate"; 
#endif
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
