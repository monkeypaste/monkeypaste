using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileCompatibilityViewSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        Control ITemplate<object, Control>.Build(object param) {
            if (param is not MpCopyItemType cit) {
                return null;
            }
            string key;
            switch (cit) {
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

            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data) {
            return data is MpCopyItemType;
        }

    }
}
