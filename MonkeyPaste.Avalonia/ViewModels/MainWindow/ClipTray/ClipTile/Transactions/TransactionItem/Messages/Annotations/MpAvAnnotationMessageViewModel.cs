﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAnnotationMessageViewModel : MpAvTransactionMessageViewModelBase {

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
        public IEnumerable<MpAvAnnotationItemViewModel> RootItems =>
            new List<MpAvAnnotationItemViewModel>() { RootAnnotationViewModel };
        public MpAvAnnotationItemViewModel RootAnnotationViewModel { get; private set; }
        public MpAvAnnotationItemViewModel SelectedItem { get; set; }

        #endregion

        public override string LabelText => "Annotation";

        #region State
        public override bool IsAnyBusy =>
            base.IsAnyBusy || RootAnnotationViewModel.IsAnyBusy;

        public string SelectedItemGuid {
            get {
                if(SelectedItem == null) {
                    return null;
                }
                return SelectedItem.AnnotationGuid;
            }
        }

        #endregion

        #region Models

        #endregion

        #endregion

        #region Constructors
        public MpAvAnnotationMessageViewModel(MpAvTransactionItemViewModel parent) : base(parent) {
            PropertyChanged += MpAvAnnotationMessageViewModel_PropertyChanged;
        }



        #endregion

        #region Public Methods

        public override async Task InitializeAsync(object jsonOrParsedFragment, MpAvITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            ParentTreeItem = parentAnnotation;
            await Task.Delay(1);
            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;

            var rootAnnotation = MpAnnotationNodeFormat.Parse(Json);
            if(rootAnnotation is MpImageAnnotationNodeFormat) {
                RootAnnotationViewModel = new MpAvImageAnnotationItemViewModel(this);
            } else {
                RootAnnotationViewModel = new MpAvAnnotationItemViewModel(this);
            }
            await RootAnnotationViewModel.InitializeAsync(rootAnnotation,null);

            while (RootAnnotationViewModel.IsAnyBusy) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(RootItems));
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Body));
            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvAnnotationMessageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItem):
                    if(RootAnnotationViewModel != null) {
                        var all_anns = RootAnnotationViewModel
                            .SelfAndAllDescendants();
                        all_anns
                            .Cast<MpAvAnnotationItemViewModel>()
                            .ForEach(x => x.IsSelected = x.AnnotationGuid == SelectedItemGuid);
                    }

                    if(Parent == null || 
                        Parent.Parent == null ||
                        Parent.Parent.Parent == null) {
                        break;
                    }
                    var ctvm = Parent.Parent.Parent;
                    if(ctvm.GetDragSource() is MpAvCefNetWebView wv) {
                        var msg = new MpQuillAnnotationSelectedMessage() {
                            annotationGuid = SelectedItemGuid
                        };
                        wv.ExecuteJavascript($"annotationSelected_ext('{msg.SerializeJsonObjectToBase64()}')");
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}