using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    internal class MpAvMenuItemDataTemplateSelector : IDataTemplate {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public string GetTemplateName(object param) {
            string keyStr = string.Empty;
            if (param is MpMenuItemViewModel cmivm) {
                if (cmivm.IsHeaderedSeparator) {
                    keyStr = "HeaderedSeperatorMenuItemTemplate";
                } else if (cmivm.IsSeparator) {
                    keyStr = "SeperatorMenuItemTemplate";
                } else if (cmivm.IsPasteToPathRuntimeItem) {
                    keyStr = "PasteToPathRuntimeMenuItemTemplate";
                } else if (cmivm.IsColorPallete) {
                    keyStr = "ColorPalleteMenuItemTemplate";
                } else if (cmivm.IsColorPalleteItem) {
                    keyStr = "ColorPalleteItemMenuItemTemplate";
                } else if (cmivm.IsNewTableSelector) {
                    keyStr = "NewTableSelectorMenuItem";
                } else if (!string.IsNullOrEmpty(cmivm.IconResourceKey)) {
                    keyStr = "DefaultMenuItemTemplate";
                } else if (!string.IsNullOrEmpty(cmivm.IconHexStr)) {
                    keyStr = "CheckableMenuItemTemplate";
                } else if (cmivm.IconId > 0) {
                    keyStr = "DefaultMenuItemTemplate";
                } else {
                    keyStr = "DefaultMenuItemTemplate";
                }
            }
            return keyStr;
        }
        public IControl Build(object param) {
            string keyStr = GetTemplateName(param);
            return AvailableTemplates[keyStr].Build(param); 
        }

        public bool Match(object data) {
            return data is MpMenuItemViewModel;
        }
    }
}
