using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalysisDocumentSelector : DataTemplateSelector {
        private DataTemplate _textAnalysisTemplate;
        public DataTemplate TextAnalysisTemplate {
            get { return _textAnalysisTemplate; }
            set { _textAnalysisTemplate = value; }
        }

        private DataTemplate _imageAnalysisTemplate;
        public DataTemplate ImageAnalysisTemplate {
            get { return _imageAnalysisTemplate; }
            set { _imageAnalysisTemplate = value; }
        }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item == null || container == null) {
                return null;
            }

            var ci = (item as MpContentItemViewModel).CopyItem;
            if(ci == null) {
                // TODO need to trigger template selection after initial load since all models are null,
                // especially for other content types
                return TextAnalysisTemplate;
            }
            switch (ci.ItemType) {
                case MpCopyItemType.RichText:
                    return TextAnalysisTemplate;
                case MpCopyItemType.Image:
                    return ImageAnalysisTemplate;
            }

            throw new Exception("Uknown Item Type");
        }
    }
}
