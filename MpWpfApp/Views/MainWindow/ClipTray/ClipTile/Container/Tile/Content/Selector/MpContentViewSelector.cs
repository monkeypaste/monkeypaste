using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpContentViewSelector : DataTemplateSelector {
        private DataTemplate _textContentTemplate;
        public DataTemplate TextContentTemplate {
            get { return _textContentTemplate; }
            set { _textContentTemplate = value; }
        }

        private DataTemplate _imageContentTemplate;
        public DataTemplate ImageContentTemplate {
            get { return _imageContentTemplate; }
            set { _imageContentTemplate = value; }
        }

        private DataTemplate _fileContentTemplate;
        public DataTemplate FileContentTemplate {
            get { return _fileContentTemplate; }
            set { _fileContentTemplate = value; }
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var ci = (item as MpContentItemViewModel).CopyItem;
            if(ci == null) {
                // TODO need to trigger template selection after initial load since all models are null,
                // especially for other content types
                return TextContentTemplate;
            }

            switch (ci.ItemType) {
                case MpCopyItemType.RichText:
                    return TextContentTemplate;
                case MpCopyItemType.Image:
                    return ImageContentTemplate;
                case MpCopyItemType.FileList:
                    return FileContentTemplate;
            }

            throw new Exception("Uknown Item Type");
        }
    }
}
