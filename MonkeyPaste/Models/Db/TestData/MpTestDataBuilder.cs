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

        public static async Task CreateImportsTestContentAsync(
            string db_path,
            string pwd,
            int content_count = 0,
            int big_count = 0,
            int link_count = 0,
            int parent_tag_count = 1,
            int child_tag_count = 2,
            int sub_child_tag_count = 0) {
            if (content_count == 0) {
                return;
            }
            MpDebug.Assert(big_count <= content_count, $"Big must be lte to content count");
            MpDebug.Assert(content_count >= link_count, $"Link Count must be lte to content count");
            if (link_count > 0) {
                MpDebug.Assert(parent_tag_count > 0, $"Must have at least 1 parent tag to add link to");
            }

            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);

            string eoc = '\t'.ToString();
            string eol = Environment.NewLine;

            // NOTE this assumes NO content exists so first ciid will be 1

            // Tables:
            // MpTag
            // MpCopyItemTag
            // MpCopyItem
            // MpCopyItemTransaction
            // MpTransactionSource
            // MpDataObject
            // MpDataObjectItem

            var t_sb = new StringBuilder();
            var cit_sb = new StringBuilder();
            var ci_sb = new StringBuilder();
            var citr_sb = new StringBuilder();
            var ts_sb = new StringBuilder();
            var do_sb = new StringBuilder();
            var doi_sb = new StringBuilder();

            int last_tid = await MpDataModelProvider.GetLastRowIdAsync<MpTag>();
            int cur_tag_id = last_tid;
            List<int> test_tag_ids = new List<int>();
            for (int i = 0; i < parent_tag_count; i++) {
                int ptid = ++cur_tag_id;
                t_sb.AppendLine(
                    string.Join(eoc,
                    ptid,
                    MpTag.CollectionsTagId,
                    Guid.NewGuid(),
                    i + 1, // sort after favorites
                    -1,
                    MpTagType.Link,
                    MpColorHelpers.GetRandomHexColor(),
                    $"TEST_{i}",
                    MpContentSortType.CopyDateTime,
                    true));
                for (int j = 0; j < child_tag_count; j++) {
                    int ctid = ++cur_tag_id;
                    t_sb.AppendLine(
                        string.Join(eoc,
                            ctid,
                            ptid,
                            Guid.NewGuid(),
                            j,
                            -1,
                            MpTagType.Link,
                            MpColorHelpers.GetRandomHexColor(),
                            $"TEST_{i}_{j}",
                            MpContentSortType.CopyDateTime,
                            true));
                    for (int k = 0; k < sub_child_tag_count; k++) {
                        int sctid = ++cur_tag_id;
                        t_sb.AppendLine(
                            string.Join(eoc,
                                sctid,
                                ctid,
                                Guid.NewGuid(),
                                k,
                                -1,
                                MpTagType.Link,
                                MpColorHelpers.GetRandomHexColor(),
                                $"TEST_{i}_{j}_{k}",
                                MpContentSortType.CopyDateTime,
                                true));
                        test_tag_ids.Add(sctid);
                    }
                    test_tag_ids.Add(ctid);
                }
                test_tag_ids.Add(ptid);
            }

            Dictionary<int, int> test_tag_count_lookup = test_tag_ids.ToDictionary(x => x, x => 0);
            int[] link_idxs = MpRandom.GetUniqueRandomInts(1, content_count, link_count);
            int[] big_idxs = MpRandom.GetUniqueRandomInts(1, content_count, big_count);

            int cit_count = 0;

            for (int i = 1; i <= content_count; i++) {
                if (link_idxs.Contains(i)) {
                    int tidx = MpRandom.Rand.Next(test_tag_ids.Count);
                    cit_sb.AppendLine(
                        string.Join(eoc,
                        ++cit_count,
                        Guid.NewGuid(),
                        test_tag_ids[tidx],
                        i,
                        test_tag_count_lookup[test_tag_ids[tidx]]++));
                }
                Guid contentGuid = Guid.NewGuid();
                bool is_big = big_idxs.Contains(i);
                string test_pt = GetTestItemText(i, contentGuid, is_big);

                ci_sb.AppendLine(
                    string.Join(eoc,
                        i,
                        Guid.NewGuid(),
                        $"Test{i.ToCommaSeperatedIntString()}",
                        MpCopyItemType.Text,
                        DateTime.Now.Ticks,
                        DateTime.Now.Ticks,
                        GetTestItemHtml(i, contentGuid, is_big),
                        MpDefaultDataModelTools.UnknownIconId,
                        i,
                        1,
                        0,
                        string.Empty,
                        string.Empty,
                        is_big ? 6168 : 54,
                        6,
                        MpCopyItem.GetContentCheckSum(test_pt)));

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
                        test_pt,
                        0));
            }

            var output_lookup = new List<Tuple<string, StringBuilder>>() {
                new Tuple<string, StringBuilder>(nameof(MpTag), t_sb),
                new Tuple<string, StringBuilder>(nameof(MpCopyItemTag),cit_sb),
                new Tuple<string, StringBuilder>(nameof(MpCopyItem),ci_sb),
                new Tuple<string, StringBuilder>(nameof(MpCopyItemTransaction),citr_sb),
                new Tuple<string, StringBuilder>(nameof(MpTransactionSource),ts_sb),
                new Tuple<string, StringBuilder>(nameof(MpDataObject),do_sb),
                new Tuple<string, StringBuilder>(nameof(MpDataObjectItem),doi_sb),
            };

            string import_dir_name = $"import_{content_count}";
            string import_dir_path = Path.Combine(Path.GetDirectoryName(db_path), import_dir_name);
            import_dir_path.ToDirectory(true, false);

            var imports_sb = new StringBuilder();
            if (pwd != null) {
                imports_sb.AppendLine($"PRAGMA key = '{pwd}';");
            }
            imports_sb.AppendLine($".explain off");
            imports_sb.AppendLine($".headers on");
            imports_sb.AppendLine(@$".separator {eoc} {eol}");
            foreach (var output_tup in output_lookup) {
                string table_name = output_tup.Item1;
                string table_import_file_name = $"{table_name}_{content_count}.csv";
                string table_import_file_path = Path.Combine(import_dir_path, table_import_file_name);
                MpFileIo.WriteTextToFile(table_import_file_path, output_tup.Item2.ToString().Replace(Environment.NewLine, eol), false);
                string batch_stmt = $".import {table_import_file_name} {table_name}";
                imports_sb.AppendLine(batch_stmt);
            }
            string imports_file_name = "imports.sql";
            string imports_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, imports_file_name), imports_sb.ToString(), false);

            string batch_script_text = $"sqlite3 '..\\{Path.GetFileName(db_path)}' < {imports_file_path}";
            string batch_file_name = "batch_import.bat";
            string batch_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, batch_file_name), batch_script_text, false);
            MpDebug.Break($"batch file for {content_count} items created at path '{batch_file_path}'");
        }


        public static async Task CreateTestContentAsync(
            int content_count = 0,
            int big_count = 0,
            int link_count = 0,
            int parent_tag_count = 1,
            int child_tag_count = 2,
            int sub_child_tag_count = 0) {
            if (content_count == 0) {
                return;
            }
            MpDebug.Assert(big_count <= content_count, $"Big must be lte to content count");
            MpDebug.Assert(content_count >= link_count, $"Link Count must be lte to content count");
            if (link_count > 0) {
                MpDebug.Assert(parent_tag_count > 0, $"Must have at least 1 parent tag to add link to");
            }

            Mp.Services.AccountTools.SetAccountType(MpUserAccountType.Unlimited);
            // create test link tags

            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);


            int[] test_tag_ids = await CreateTestLinkTagsAsync(parent_tag_count, child_tag_count, sub_child_tag_count);
            int[] link_idxs = MpRandom.GetUniqueRandomInts(0, content_count - 1, link_count);

            async Task CreateTestItemAsync(int i) {
                Guid contentGuid = Guid.NewGuid();
                string data = GetTestItemHtml(i, contentGuid, false);

                var mpdo = new MpPortableDataObject(MpPortableDataFormats.Text, GetTestItemText(i, contentGuid, false));
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

        private static string GetTestItemText(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                @"BIG BOY!! \nI am test {0} \nMy content guid is {1}\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\n" :
                @"Hey! \nThis is test #{0}\n\nUnique Id is:\n {1}\nSee ya :p\n";

            return string.Format(template_html, i, contentGuid);
        }

        private static string GetTestItemHtml(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                "<p><span style='color:#ffffff'>BIG&nbsp;BOY!!&nbsp;</span><br/><span style='color:#ffffff'>I&nbsp;am&nbsp;test&nbsp;{0}&nbsp;</span><br/><span style='color:#ffffff'>My&nbsp;content&nbsp;guid&nbsp;is&nbsp;{1}</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style='color:#bfbfbf'>This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style='color:#bfbfbf'>This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa'   target='_blank'>Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A'   target='_blank'>Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list='ordered'><a style='color:#a9c7d5'   href='https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B'   target='_blank'>Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style='color:#bfbfbf'>If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span></p>" :
                "<p><span style='color:#ffffff'>Hey!&nbsp;</span><br/><em class='font-bg-color-override-on' style='background-color:#f34544'>Th</em><em class='font-bg-color-override-on' style='background-color:#44c721'>is&nbsp;is</em><em class='font-bg-color-override-on' style='background-color:#f34544'>&nbsp;te</em><span class='font-bg-color-override-on' style='background-color:#f34544'>st</span><span style='color:#ffffff'>&nbsp;#99954</span><br/><br/><span class='font-color-override-on' style='color:#fc4ad2'>Unique&nbsp;Id&nbsp;is:</span><br/><span style='color:#ffffff'>&nbsp;797bbf8f-d409-4e0b-a9e8-e6362d8f51d1</span><br/><span style='color:#ffffff'>See&nbsp;ya&nbsp;:p</span></p>";

            return string.Format(template_html, i, contentGuid);
        }
    }
}
