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
            if (!MpPrefViewModel.Instance.IsRichHtmlContentEnabled &&
                param is MpAvClipTileViewModel ctvm) {
                ctvm.IsEditorLoaded = true;
                switch (ctvm.CopyItemType) {
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
            }

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpAvClipTileViewModel;
        }

    }
}
