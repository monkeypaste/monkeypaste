﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Azure;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using Newtonsoft.Json;
using Windows.Globalization;
using Xamarin.Forms.Internals;

namespace MpWpfApp {
    public enum MpTranslatorParamType {
        None = 0,
        FromLang,
        ToLang
    }
    public class MpTranslatorViewModel : MpAnalyticItemViewModel {
        #region Properties
        #region State
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpTranslatorViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
         }

        #endregion

        #region Public Methods

        public override async Task<MpAnalyticItemParameterFormat> DeferredCreateParameterModel(MpAnalyticItemParameterFormat aip) {
            IsBusy = true;

            if (!MpLanguageTranslator.IsLoaded) {
                await MpLanguageTranslator.Init();
            }

            foreach(var lcat in MpLanguageTranslator.LanguageCodesAndTitles) {
                var laipv = new MpAnalyticItemParameterValue() {
                    label = string.Format(@"{0} - {1}",lcat.Value.LanguageName, lcat.Value.NativeName),
                    value = lcat.Key.ToString()
                };
                aip.values.Add(laipv);
            }

            IsBusy = false;
            return aip;
        }


        public override bool Validate() {
            if (IsBusy || Parent.IsBusy || !IsLoaded) {
                return true;
            }

            var paramLookup = SelectedPresetViewModel.ParamLookup;

            var fromParam = paramLookup[(int)MpTranslatorParamType.FromLang] as MpComboBoxParameterViewModel;
            var toParam = paramLookup[(int)MpTranslatorParamType.ToLang] as MpComboBoxParameterViewModel;

            if (fromParam.CurrentValue == toParam.CurrentValue) {
                toParam.ValidationMessage = "Cannot translate to same language";
            } else {
                toParam.ValidationMessage = string.Empty;
            }
            return string.IsNullOrEmpty(toParam.ValidationMessage);
        }

        #endregion

        #region Protected Methods


        protected override async Task<MpAnalyzerTransaction> ExecuteAnalysis(object obj) {
            IsBusy = true;
            var paramLookup = SelectedPresetViewModel.ParamLookup;

            string fromCode = paramLookup[(int)MpTranslatorParamType.FromLang].CurrentValue;
            string toCode = paramLookup[(int)MpTranslatorParamType.ToLang].CurrentValue;

            string translatedText = await MpLanguageTranslator.TranslateAsync(
                obj.ToString(),
                toCode,
                fromCode);

            IsBusy = false;

            return new MpAnalyzerTransaction() {
                Request = new MpAzureTranslateRequestFormat() {
                    FromCode = fromCode,
                    ToCode = toCode
                },
                Response = translatedText
            };
        }
        #endregion

        #region Private Methods
        
        #endregion
    }
}