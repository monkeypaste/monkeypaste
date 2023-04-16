using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionReporter : MpITransactionReporter {

        public async Task<MpCopyItemTransaction> ReportTransactionAsync(
            int copyItemId,
            MpJsonMessageFormatType reqType = MpJsonMessageFormatType.None,
            string req = "",
            MpJsonMessageFormatType respType = MpJsonMessageFormatType.None,
            string resp = "",
            IEnumerable<string> source_ref_uris = null,
            MpTransactionType transactionType = MpTransactionType.None) {

            if (copyItemId <= 0) {
                throw new Exception("CopyItemId required to create transaction");
            }
            if (source_ref_uris == null || source_ref_uris.Count() == 0) {
                throw new Exception("Must provide transaction references");
            }
            if (transactionType != MpTransactionType.Analyzed) {
                req = null;
                resp = null;
            }
            var cit = await MpCopyItemTransaction.CreateAsync(
                            copyItemId: copyItemId,
                            reqMsgType: reqType,
                            reqMsgJsonStr: req,
                            respMsgType: respType,
                            respMsgJsonStr: resp,
                            transactionType: transactionType);

            //List<Tuple<MpISourceRef, string>> source_ref_arg_tuples = new List<Tuple<MpISourceRef, string>>();
            //foreach (var url_arg_tuple in source_refs) {
            //    MpISourceRef sr = await Mp.Services.SourceRefTools.FetchOrCreateSourceAsync(url_arg_tuple);
            //    // check provided url to see if query arg is already embedded
            //    string url_query_param = Mp.Services.SourceRefTools.ParseRefArgs(url_arg_tuple);
            //    source_ref_arg_tuples.Add(new Tuple<MpISourceRef, string>(sr, url_query_param));
            //}

            //if (source_ref_arg_tuples.Count() != source_refs.Count()) {
            //    throw new Exception($"Error, not all source urls could be stored.");
            //}

            var source_refs = await Task.WhenAll(
                source_ref_uris.Select(x => Mp.Services.SourceRefTools.FetchOrCreateSourceAsync(x)));
            await Mp.Services.SourceRefTools.AddTransactionSourcesAsync(
                cit.Id,
                source_refs);

            return cit;
        }
    }
}
