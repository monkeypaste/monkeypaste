using MonkeyPaste;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public abstract class MpTextElement : FrameworkContentElement, MpIUserControl {
        public MpTextElement() : base() { }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
    }

    //public class MpTextElement<TElementType> : TElementType where TElementType : TextElement {

    //}
}
