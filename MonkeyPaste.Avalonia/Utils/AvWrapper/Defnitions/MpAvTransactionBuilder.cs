using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionBuilder : MpITransactionBuilder {

        public async Task<MpCopyItemTransaction> PerformTransactionAsync(
            int copyItemId,
            MpJsonMessageFormatType reqType, 
            string req,
            MpJsonMessageFormatType respType, 
            string resp,
            IEnumerable<string> ref_urls) {

            if(copyItemId <= 0) {
                throw new Exception("CopyItemId required to create transaction");
            }
            if(ref_urls == null || ref_urls.Count() == 0) {
                throw new Exception("Must provide transaction references");
            }
            var cit = await MpCopyItemTransaction.CreateAsync(
                            copyItemId: copyItemId,
                            reqMsgType: reqType,
                            reqMsgJsonStr: req,
                            respMsgType: respType,
                            respMsgJsonStr: resp);
            var refs = await Task.WhenAll(
                    ref_urls.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(x)));

            if (refs == null) {
                throw new Exception($"Error, could not fetch or create all sources");
            }
            if(refs.Count() != ref_urls.Count()) {
                throw new Exception($"Error, not all source urls could be stored.");
            }
            
            await MpPlatformWrapper.Services.SourceRefBuilder.AddTransactionSourcesAsync(
                cit.Id,
                refs);

            return cit;
        }
    }
}
