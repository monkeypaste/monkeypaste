﻿using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvTransactionReporter : MpITransactionReporter {

        public async Task<MpCopyItemTransaction> ReportTransactionAsync(
            int copyItemId,
            MpJsonMessageFormatType reqType,
            string req,
            MpJsonMessageFormatType respType,
            string resp,
            IEnumerable<string> ref_url_arg_tuples,
            MpTransactionType transactionType) {

            if (copyItemId <= 0) {
                throw new Exception("CopyItemId required to create transaction");
            }
            if (ref_url_arg_tuples == null || ref_url_arg_tuples.Count() == 0) {
                throw new Exception("Must provide transaction references");
            }
            var cit = await MpCopyItemTransaction.CreateAsync(
                            copyItemId: copyItemId,
                            reqMsgType: reqType,
                            reqMsgJsonStr: req,
                            respMsgType: respType,
                            respMsgJsonStr: resp,
                            transactionType: transactionType);

            List<Tuple<MpISourceRef, string>> source_ref_arg_tuples = new List<Tuple<MpISourceRef, string>>();
            foreach (var url_arg_tuple in ref_url_arg_tuples) {
                MpISourceRef sr = await Mp.Services.SourceRefBuilder.FetchOrCreateSourceAsync(url_arg_tuple);
                // check provided url to see if query arg is already embedded
                string url_query_param = Mp.Services.SourceRefBuilder.ParseRefArgs(url_arg_tuple);
                source_ref_arg_tuples.Add(new Tuple<MpISourceRef, string>(sr, url_query_param));
            }

            if (source_ref_arg_tuples.Count() != ref_url_arg_tuples.Count()) {
                throw new Exception($"Error, not all source urls could be stored.");
            }

            await Mp.Services.SourceRefBuilder.AddTransactionSourcesAsync(
                cit.Id,
                source_ref_arg_tuples);

            return cit;
        }
    }
}