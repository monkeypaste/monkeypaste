using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvImageAnnotationMessageViewModelBase : MpAvTransactionMessageViewModelBase {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public MpAnnotationNodeFormat RootAnnotation { get; private set; }
        #endregion

        public override string LabelText => "Annotation";
        #endregion

        #region Constructors
        public MpAvImageAnnotationMessageViewModelBase(MpAvTransactionItemViewModelBase parent) : base(parent) {
        }


        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            await Task.Delay(1);
            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;

            if (MpJsonConverter.DeserializeObject<MpAnnotationNodeFormat>(Json) is MpAnnotationNodeFormat root_annotation) { 
                RootAnnotation = root_annotation;
            }
            ParentTreeItem = parentAnnotation;

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Body));
            IsBusy = false;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
