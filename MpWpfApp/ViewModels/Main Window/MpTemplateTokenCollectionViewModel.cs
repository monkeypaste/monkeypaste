using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpTemplateTokenCollectionViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel> {

        #region Properties
        //public Dictionary<string, string> TemplateTokenLookupDictionary {
        //    get {
        //        return _templateTokenLookupDictionary;
        //    }
        //    set {
        //        if (_templateTokenLookupDictionary != value) {
        //            _templateTokenLookupDictionary = value;
        //            OnPropertyChanged(nameof(TemplateTokenLookupDictionary));
        //            OnPropertyChanged(nameof(TemplateNavigationButtonStackVisibility));
        //        }
        //    }
        //}

        public bool IsTemplateEmpty {
            get {
                return this.Where(x => x.TemplateText.Length > 0).ToList().Count == 0;
            }
        }
        #endregion
        #region Public Methods

        #endregion
    }
}
