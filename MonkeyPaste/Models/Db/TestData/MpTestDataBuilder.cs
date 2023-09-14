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
        public static async Task CreateTestDataAsync() {
            string batch_file_path = await CreateImportsTestContentAsync(
                   db_path: Mp.Services.DbInfo.DbPath,
                   pwd: Mp.Services.DbInfo.DbPassword,
                   content_count: 1000,//1_000_000,
                   big_count: 5,
                   link_count: 50,
                   parent_tag_count: 3,
                   child_tag_count: 3,
                   sub_child_tag_count: 2);

            await ImportTestDataAsync(batch_file_path);
            MpDebug.Break($"batch file created at path '{batch_file_path}'");
        }

        private static async Task ImportTestDataAsync(string batch_file_path) {
            using var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "cmd",
                    Arguments = @"/c " + Path.GetFileName(batch_file_path),
                    WorkingDirectory = Path.GetDirectoryName(batch_file_path),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            string output_line = null;
            var sb = new StringBuilder();
            try {
                process.Start();
                while ((output_line = await process.StandardOutput.ReadLineAsync()) != null) {
                    sb.AppendLine(output_line);
                    if (output_line.StartsWith("done")) {
                        string all_output = sb.ToString();
                        return;
                    }
                }
            }
            catch (Exception) {
                string all_output = sb.ToString();
                return;
            }
        }
        private static async Task<string> CreateImportsTestContentAsync(
            string db_path,
            string pwd,
            int content_count = 0,
            int big_count = 0,
            int link_count = 0,
            int parent_tag_count = 1,
            int child_tag_count = 2,
            int sub_child_tag_count = 0) {
            MpDebug.Assert(big_count <= content_count, $"Big must be lte to content count");
            MpDebug.Assert(content_count >= link_count, $"Link Count must be lte to content count");
            if (link_count > 0) {
                MpDebug.Assert(parent_tag_count > 0, $"Must have at least 1 parent tag to add link to");
            }

            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);

            string eoc = "|";// '\t'.ToString();
            string eol = Environment.NewLine;

            var table_types = new Type[] {
                typeof(MpTag),
                typeof(MpCopyItemTag),
                typeof(MpCopyItem),
                typeof(MpCopyItemTransaction),
                typeof(MpTransactionSource),
                typeof(MpDataObject),
                typeof(MpDataObjectItem)
            };

            var tables = table_types.ToDictionary(x => x, x => new MpImportTable(x));
            var tt = tables[typeof(MpTag)];
            int first_tag_id = tt.LastRowId + 1;
            for (int i = 0; i < parent_tag_count; i++) {

                int ptid = tt.LastRowId + 1;
                tt.AddRow(
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
                    int ctid = tt.LastRowId + 1;
                    tt.AddRow(
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
                        int sctid = tt.LastRowId + 1;
                        tt.AddRow(
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
                    }
                }
            }
            var test_tag_ids = Enumerable.Range(first_tag_id, parent_tag_count * child_tag_count * sub_child_tag_count).ToList();
            Dictionary<int, int> test_tag_count_lookup = test_tag_ids.ToDictionary(x => x, x => 0);
            int[] link_idxs = MpRandom.GetUniqueRandomInts(1, content_count, link_count);
            int[] big_idxs = MpRandom.GetUniqueRandomInts(1, content_count, big_count);

            for (int i = 1; i <= content_count; i++) {
                int citid = tables[typeof(MpCopyItemTag)].LastRowId + 1;
                int ciid = tables[typeof(MpCopyItem)].LastRowId + 1;
                int citrid = tables[typeof(MpCopyItemTransaction)].LastRowId + 1;
                int doid = tables[typeof(MpDataObject)].LastRowId + 1;
                int doiid = tables[typeof(MpDataObjectItem)].LastRowId + 1;
                int tsid = tables[typeof(MpTransactionSource)].LastRowId + 1;

                if (link_idxs.Contains(i)) {
                    int tidx = MpRandom.Rand.Next(test_tag_ids.Count);
                    tables[typeof(MpCopyItemTag)].AddRow(
                        string.Join(eoc,
                            citid,
                            Guid.NewGuid(),
                            test_tag_ids[tidx],
                            ciid,
                            test_tag_count_lookup[test_tag_ids[tidx]]++,
                            DateTime.Now.Ticks));
                }
                Guid contentGuid = Guid.NewGuid();
                bool is_big = big_idxs.Contains(i);
                string test_pt = GetTestItemText(i, contentGuid, is_big);

                tables[typeof(MpCopyItem)].AddRow(
                    string.Join(eoc,
                        ciid,
                        Guid.NewGuid(),
                        $"Test_{ciid.ToCommaSeperatedIntString()}",
                        MpCopyItemType.Text,
                        DateTime.Now.Ticks,
                        DateTime.Now.Ticks,
                        GetTestItemHtml(ciid, contentGuid, is_big),
                        MpDefaultDataModelTools.UnknownIconId,
                        doid,
                        1,
                        0,
                        string.Empty,
                        string.Empty,
                        is_big ? 6168 : 54,
                        6,
                        MpCopyItem.GetContentCheckSum(test_pt)));

                tables[typeof(MpCopyItemTransaction)].AddRow(
                    string.Join(eoc,
                        citrid,
                        Guid.NewGuid(),
                        ciid,
                        MpTransactionType.Created,
                        MpJsonMessageFormatType.DataObject,
                        string.Empty,
                        MpJsonMessageFormatType.Delta,
                        string.Empty,
                        MpDefaultDataModelTools.ThisUserDeviceId,
                        DateTime.Now.Ticks,
                        DateTime.Now.Ticks));

                tables[typeof(MpTransactionSource)].AddRow(
                    string.Join(eoc,
                        tsid,
                        Guid.NewGuid(),
                        citrid,
                        MpTransactionSourceType.App,
                        MpDefaultDataModelTools.ThisAppId,
                        DateTime.Now.Ticks));

                tables[typeof(MpDataObject)].AddRow(
                    string.Join(eoc,
                        doid,
                        Guid.NewGuid()));

                tables[typeof(MpDataObjectItem)].AddRow(
                    string.Join(eoc,
                        doiid,
                        Guid.NewGuid(),
                        doid,
                        MpPortableDataFormats.Text,
                        test_pt,
                        0));
            }

            string import_dir_name = $"import_{content_count.ToCommaSeperatedIntString()}";
            string import_dir_path = Path.Combine(Path.GetDirectoryName(db_path), import_dir_name);
            import_dir_path.ToDirectory(true, false);

            var imports_sb = new StringBuilder();
            if (pwd != null) {
                imports_sb.AppendLine($"PRAGMA key = '{pwd}';");
            }
            imports_sb.AppendLine($".explain off");
            imports_sb.AppendLine($".bail on");
            //imports_sb.AppendLine($".progress 10000");
            imports_sb.AppendLine(@$".separator {eoc}");
            foreach (var it in tables.Select(x => x.Value)) {
                string table_name = it.TableType.Name;
                string table_import_file_name = $"{table_name}_{content_count.ToCommaSeperatedIntString()}.csv";
                string table_import_file_path = Path.Combine(import_dir_path, table_import_file_name);
                MpFileIo.WriteTextToFile(table_import_file_path, it.OutputCsv.Replace(Environment.NewLine, eol), false);
                string batch_stmt = $".import {table_import_file_name} {table_name}";
                imports_sb.AppendLine(batch_stmt);
            }
            string imports_file_name = "imports.sql";
            string imports_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, imports_file_name), imports_sb.ToString(), false);

            var batch_script_sb = new StringBuilder();
            batch_script_sb.AppendLine("@echo off");
            batch_script_sb.AppendLine($"sqlite3 ..\\{Path.GetFileName(db_path)} < {imports_file_name}");
            batch_script_sb.AppendLine("echo done");

            string batch_file_name = "batch_import.bat";
            string batch_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, batch_file_name), batch_script_sb.ToString(), false);

            return batch_file_path;
        }

        private static string GetTestItemText(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                @"BIG BOY!! \nI am test {0} \nMy content guid is {1}\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\nThis awesome!! article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nThis article can be considered as the fourth instalment in the following sequence of articles:\nMultiplatform UI Coding with AvaloniaUI in Easy Samples. Part 1 - AvaloniaUI Building Blocks\nBasics of XAML in Easy Samples for Multiplatform Avalonia .NET Framework\nMultiplatform Avalonia .NET Framework Programming Basic Concepts in Easy Samples\nIf you know WPF, you can read this article without reading the previous ones, otherwise, you should read the previous articles first.\n" :
                @"Hey! \nThis is test #{0}\n\nUnique Id is:\n {1}\nSee ya :p\n";

            return string.Format(template_html, i, contentGuid);
        }

        private static string GetTestItemHtml(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                @"<p><span style=""color:#ffffff"">BIG&nbsp;BOY!!&nbsp;</span><br/><span style=""color:#ffffff"">I&nbsp;am&nbsp;test&nbsp;{0}&nbsp;</span><br/><span style=""color:#ffffff"">My&nbsp;content&nbsp;guid&nbsp;is&nbsp;{1}</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span><br/><span style=""color:#bfbfbf"">This&nbsp;awesome!!&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span><br/><span style=""color:#bfbfbf"">This&nbsp;article&nbsp;can&nbsp;be&nbsp;considered&nbsp;as&nbsp;the&nbsp;fourth&nbsp;instalment&nbsp;in&nbsp;the&nbsp;following&nbsp;sequence&nbsp;of&nbsp;articles:</span></p><ol><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5308645/Multiplatform-UI-Coding-with-AvaloniaUI-in-Easy-Sa""   target=""_blank"">Multiplatform&nbsp;UI&nbsp;Coding&nbsp;with&nbsp;AvaloniaUI&nbsp;in&nbsp;Easy&nbsp;Samples.&nbsp;Part&nbsp;1&nbsp;-&nbsp;AvaloniaUI&nbsp;Building&nbsp;Blocks</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5314369/Basics-of-XAML-in-Easy-Samples-for-Multiplatform-A""   target=""_blank"">Basics&nbsp;of&nbsp;XAML&nbsp;in&nbsp;Easy&nbsp;Samples&nbsp;for&nbsp;Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework</a></li><li data-list=""ordered""><a style=""color:#a9c7d5""   href=""https://www.codeproject.com/Articles/5311995/Multiplatform-Avalonia-NET-Framework-Programming-B""   target=""_blank"">Multiplatform&nbsp;Avalonia&nbsp;.NET&nbsp;Framework&nbsp;Programming&nbsp;Basic&nbsp;Concepts&nbsp;in&nbsp;Easy&nbsp;Samples</a></li></ol><p><span style=""color:#bfbfbf"">If&nbsp;you&nbsp;know&nbsp;WPF,&nbsp;you&nbsp;can&nbsp;read&nbsp;this&nbsp;article&nbsp;without&nbsp;reading&nbsp;the&nbsp;previous&nbsp;ones,&nbsp;otherwise,&nbsp;you&nbsp;should&nbsp;read&nbsp;the&nbsp;previous&nbsp;articles&nbsp;first.</span></p>" :
                @"<p><span style=""color:#ffffff"">Hey!&nbsp;</span><br/><em class=""font-bg-color-override-on"" style=""background-color:#f34544"">Th</em><em class=""font-bg-color-override-on"" style=""background-color:#44c721"">is&nbsp;is</em><em class=""font-bg-color-override-on"" style=""background-color:#f34544"">&nbsp;te</em><span class=""font-bg-color-override-on"" style=""background-color:#f34544"">st</span><span style=""color:#ffffff"">&nbsp;#{0}</span><br/><br/><span class=""font-color-override-on"" style=""color:#fc4ad2"">Unique&nbsp;Id&nbsp;is:</span><br/><span style=""color:#ffffff"">{1}&nbsp;</span><br/><span style=""color:#ffffff"">See&nbsp;ya&nbsp;:p</span></p>";

            return string.Format(template_html, i, contentGuid);
        }
    }

    internal class MpImportTable {
        private StringBuilder sb = new StringBuilder();
        public Type TableType { get; }

        public int LastRowId { get; set; }
        public string OutputCsv =>
            sb.ToString();
        public MpImportTable(Type tableType) {
            TableType = tableType;
            var mi = typeof(MpDataModelProvider).GetMethods().FirstOrDefault(x => x.Name == nameof(MpDataModelProvider.GetLastRowId));
            if (mi.GetGenericMethodDefinition().MakeGenericMethod(TableType).Invoke(this, null) is int last_row_id) {
                LastRowId = last_row_id;
            }
        }

        public void AddRow(string row) {
            sb.AppendLine(row);
            LastRowId++;
        }

    }
}
