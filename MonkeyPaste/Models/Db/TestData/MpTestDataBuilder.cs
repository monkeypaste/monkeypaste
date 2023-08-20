using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpTestDataBuilder {

        public static async Task CreateTestContentAsync(int content_count = 0, int link_count = 0, int parent_tag_count = 1, int child_tag_count = 2, int sub_child_tag_count = 0) {
            if (content_count == 0) {
                return;
            }

            // create test link tags

            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);


            int[] test_tag_ids = await CreateTestLinkTagsAsync(parent_tag_count, child_tag_count, sub_child_tag_count);
            int[] link_idxs = MpRandom.GetUniqueRandomInts(0, content_count - 1, link_count);

            async Task CreateTestItemAsync(int i) {
                Guid contentGuid = Guid.NewGuid();
                string data = GetTestItemHtml(i, contentGuid);

                var mpdo = new MpPortableDataObject(MpPortableDataFormats.Text, GetTestItemText(i, contentGuid));
                var dobj = await MpDataObject.CreateAsync(pdo: mpdo);
                var ci = await MpCopyItem.CreateAsync(
                    data: data,
                    itemType: MpCopyItemType.Text,
                    title: $"Test {i + 1}",
                    dataObjectId: dobj.Id);

                await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                    copyItemId: ci.Id,
                    reqType: MpJsonMessageFormatType.DataObject,
                    respType: MpJsonMessageFormatType.Delta,
                    transType: MpTransactionType.Created,
                    ref_uris: new[] { this_app_url });

                if (link_idxs.Contains(i)) {
                    await MpCopyItemTag.CreateAsync(
                        tagId: test_tag_ids[MpRandom.Rand.Next(test_tag_ids.Length)],
                        copyItemId: ci.Id);
                }
                if (i % 100 == 0) {
                    MpConsole.WriteLine($"{content_count - i} Test Items Remaining");
                }
            }
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < content_count; i++) {
                await CreateTestItemAsync(i);
            }
            //var tasks = Enumerable.Range(0, content_count).Select(x => CreateTestItemAsync(x));
            //await Task.WhenAll(tasks);
            sw.Stop();
            MpConsole.WriteLine($"Total ms: {sw.ElapsedMilliseconds} Time per item: {sw.ElapsedMilliseconds / content_count}");
        }

        public static async Task CreateImportsTestContentAsync(string db_path, int content_count = 0, int link_count = 0, int parent_tag_count = 1, int child_tag_count = 2, int sub_child_tag_count = 0) {
            // from https://stackoverflow.com/a/759866/105028

            if (content_count == 0) {
                return;
            }
            // create test link tags

            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);


            int[] test_tag_ids = new[] { 11, 12, 13 };// await CreateTestLinkTagsAsync(parent_tag_count, child_tag_count, sub_child_tag_count);
            int[] link_idxs = MpRandom.GetUniqueRandomInts(1, content_count, Math.Min(link_count, content_count - 1));

            // NOTE this assumes NO content exists so first ciid will be 1

            // Tables:
            // MpCopyItemTag
            // MpCopyItem
            // MpCopyItemTransaction
            // MpTransactionSource
            // MpDataObject
            // MpDataObjectItem

            var cit_sb = new StringBuilder();
            var ci_sb = new StringBuilder();
            var citr_sb = new StringBuilder();
            var ts_sb = new StringBuilder();
            var do_sb = new StringBuilder();
            var doi_sb = new StringBuilder();
            string eoc = '\t'.ToString();
            int cit_count = 0;

            for (int i = 1; i <= content_count; i++) {
                if (link_idxs.Contains(i)) {
                    cit_sb.AppendLine(
                        string.Join(eoc,
                        ++cit_count,
                        Guid.NewGuid(),
                        test_tag_ids[MpRandom.Rand.Next(test_tag_ids.Length)],
                        i));
                }
                Guid contentGuid = Guid.NewGuid();

                ci_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid(),
                        $"Test{i}",
                        MpCopyItemType.Text,
                        DateTime.Now.Ticks,
                        DateTime.Now.Ticks,
                        GetTestItemHtml(i, contentGuid),
                        MpDefaultDataModelTools.UnknownIconId,
                        i,
                        1,
                        0,
                        string.Empty,
                        string.Empty,
                        54,
                        6));

                citr_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid(),
                        i,
                        MpTransactionType.Created,
                        MpJsonMessageFormatType.DataObject,
                        string.Empty,
                        MpJsonMessageFormatType.Delta,
                        string.Empty,
                        MpDefaultDataModelTools.ThisUserDeviceId,
                        DateTime.Now.Ticks,
                        DateTime.Now.Ticks));

                ts_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid(),
                        i,
                        MpTransactionSourceType.App,
                        MpDefaultDataModelTools.ThisAppId,
                        DateTime.Now.Ticks));

                do_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid()));

                doi_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid(),
                        i,
                        MpPortableDataFormats.Text,
                        GetTestItemText(i, contentGuid),
                        0));
            }

            var output_lookup = new List<Tuple<string, StringBuilder>>() {
                new Tuple<string, StringBuilder>(nameof(MpCopyItemTag),cit_sb),
                new Tuple<string, StringBuilder>(nameof(MpCopyItem),ci_sb),
                new Tuple<string, StringBuilder>(nameof(MpCopyItemTransaction),citr_sb),
                new Tuple<string, StringBuilder>(nameof(MpTransactionSource),ts_sb),
                new Tuple<string, StringBuilder>(nameof(MpDataObject),do_sb),
                new Tuple<string, StringBuilder>(nameof(MpDataObjectItem),doi_sb),
            };

            // example:
            /*
            $ echo "create table mytable (col1 int, col2 int);" | sqlite3 foo.sqlite
            $ echo ".import demotab.txt mytable"  | sqlite3 foo.sqlite
            */
            string db_file_name = Path.GetFileName(db_path);
            string db_dir = Path.GetDirectoryName(db_path);
            var batch_sb = new StringBuilder();
            batch_sb.AppendLine("@echo off");
            foreach (var output_tup in output_lookup) {
                string table_name = output_tup.Item1;
                string import_file_name = $"{table_name}_{content_count}.csv";
                string import_file_path = Path.Combine(db_dir, import_file_name);
                MpFileIo.WriteTextToFile(import_file_path, output_tup.Item2.ToString(), false);
                string batch_stmt = $"echo \".import {import_file_name} {table_name}\"  | sqlite3 {db_file_name}";
                batch_sb.AppendLine(batch_stmt);
            }
            batch_sb.AppendLine("pause");
            string batch_file_path = Path.Combine(db_dir, "batch_import.bat");
            MpFileIo.WriteTextToFile(batch_file_path, batch_sb.ToString(), false);
            MpDebug.Break($"batch file for {content_count} items created at path '{batch_file_path}'");
        }

        private static async Task<int[]> CreateTestLinkTagsAsync(int parent_count, int child_count, int sub_child_count) {
            // NOTE Total tags will be parent * chlid * sub_child

            List<int> tag_ids = new List<int>();
            for (int i = 0; i < parent_count; i++) {
                var parent_t = await MpTag.CreateAsync(
                    tagName: $"TEST_{i}",
                    treeSortIdx: i,
                    parentTagId: MpTag.CollectionsTagId,
                    tagType: MpTagType.Link);
                for (int j = 0; j < child_count; j++) {
                    var child_t = await MpTag.CreateAsync(
                        tagName: $"TEST_{i}_{j}",
                        treeSortIdx: j,
                        parentTagId: parent_t.Id,
                        tagType: MpTagType.Link);
                    for (int k = 0; k < sub_child_count; k++) {
                        var sub_child_t = await MpTag.CreateAsync(
                            tagName: $"TEST_{i}_{j}_{k}",
                            treeSortIdx: k,
                            parentTagId: child_t.Id,
                            tagType: MpTagType.Link);
                        tag_ids.Add(sub_child_t.Id);
                    }
                    tag_ids.Add(child_t.Id);
                }
                tag_ids.Add(parent_t.Id);
            }
            return tag_ids.ToArray();
        }

        private static string GetTestItemText(int i, Guid contentGuid) {
            string template_html = @"Hey! \nThis is test #{0}\n\nUnique Id is:\n {1}\nSee ya :p\n";
            return string.Format(template_html, i, contentGuid);
        }

        private static string GetTestItemHtml(int i, Guid contentGuid) {
            //string data = $"<p>This is test {i + 1}.</p><p>{System.Guid.NewGuid()}</p>";
            //string data = $"<p><span style=\"color:#ffffff\">This&nbsp;is&nbsp;test&nbsp;{i + 1}.</span><br/><span style=\"color:#ffffff\">{System.Guid.NewGuid()}</span></p>";
            string template_html = @"<p>Hey!&nbsp;<br/><em class=""font-bg-color-override-on ql-size-24px"" style=""background-color:#f34544"">Th</em><em class=""font-bg-color-override-on ql-size-24px"" style=""background-color:#44c721"">is&nbsp;is</em><em class=""font-bg-color-override-on ql-size-24px"" style=""background-color:#f34544"">&nbsp;te</em><span class=""font-bg-color-override-on ql-size-24px"" style=""background-color:#f34544"">st</span>&nbsp;#{0}<br/><br/><span class=""font-color-override-on ql-font-impact ql-size-54px"" style=""color:#fc4ad2"">Unique&nbsp;Id&nbsp;is:</span><br/>&nbsp;{1}<br/>See&nbsp;ya&nbsp;:p</p>";
            return string.Format(template_html, i, contentGuid);
        }
    }
}
