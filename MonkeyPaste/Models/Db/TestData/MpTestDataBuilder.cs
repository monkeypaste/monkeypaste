using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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
                   content_count: 100_000,//1_000_000,
                   big_count: 25,
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
                MpFileIo.WriteTextToFile(table_import_file_path, it.OutputCsv.Replace(Environment.NewLine, eol));
                string batch_stmt = $".import {table_import_file_name} {table_name}";
                imports_sb.AppendLine(batch_stmt);
            }
            string imports_file_name = "imports.sql";
            string imports_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, imports_file_name), imports_sb.ToString());

            var batch_script_sb = new StringBuilder();
            batch_script_sb.AppendLine("@echo off");
            batch_script_sb.AppendLine($"sqlite3 ..\\{Path.GetFileName(db_path)} < {imports_file_name}");
            batch_script_sb.AppendLine("echo done");

            string batch_file_name = "batch_import.bat";
            string batch_file_path = MpFileIo.WriteTextToFile(Path.Combine(import_dir_path, batch_file_name), batch_script_sb.ToString());

            return batch_file_path;
        }

        private static string GetTestItemText(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                @"Long Content Test {0}\n{1}\nPellentesque habitant morbi tristiquesenectus et netus et malesuada fames ac turpis egestas. Vestibulum tortor quam, feugiat vitae, ultricies eget, tempor sit amet, ante. Donec eu libero sit amet quam egestas semper.Aenean ultricies mi vitae est.Mauris placerat eleifend leo. Quisque sit amet est et sapien ullamcorper pharetra. Vestibulum erat wisi, condimentum sed,commodo vitae, ornare sit amet, wisi. Aenean fermentum, elit eget tincidunt condimentum, eros ipsum rutrum orci, sagittis tempus lacus enim ac dui.Donec non enimin turpis pulvinar facilisis. Ut felis.\nHeader Level 2\nLorem ipsum dolor sit amet, consectetuer adipiscing elit.\nAliquam tincidunt mauris eu risus.\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus magna. Cras in mi at felis aliquet congue. Ut a est eget ligula molestie gravida. Curabitur massa. Donec eleifend, libero at sagittis mollis, tellus est malesuada tellus, at luctus turpis elit sit amet quam. Vivamus pretium ornare est.\nHeader Level 3\nLorem ipsum dolor sit amet, consectetuer adipiscing elit.\nAliquam tincidunt mauris eu risus.\n" :
                @"Short Content Test {0}\n{1}\nPellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.\n";

            return string.Format(template_html, i, contentGuid);
        }

        private static string GetTestItemHtml(int i, Guid contentGuid, bool isBig) {
            string template_html = isBig ?
                @"<h1><strong style=""color:#ffffff""><em>Long Con</em></strong><span class=""font-color-override-on"" style=""color:#effeb9"">tent</span><span style=""color:#ffffff"">&nbsp;Test&nbsp;{0}</span></h1><p><span style=""color:#ffffff"">{1}</span><br><strong class=""font-bg-color-override-on font-color-override-on"" style=""color:#44c721;background-color:#e167a4"">Pellentesque&nbsp;habitant</strong><strong style=""color:#ffffff"">&nbsp;morbi&nbsp;tristique</strong><span style=""color:#ffffff"">senectus&nbsp;et&nbsp;netus&nbsp;et&nbsp;malesuada&nbsp;fames&nbsp;ac&nbsp;turpis&nbsp;egestas.&nbsp;Vestibulum&nbsp;tortor&nbsp;quam,&nbsp;feugiat&nbsp;vitae,&nbsp;ultricies&nbsp;eget,&nbsp;tempor&nbsp;sit&nbsp;amet,&nbsp;ante.&nbsp;Donec&nbsp;eu&nbsp;libero&nbsp;sit&nbsp;amet&nbsp;quam&nbsp;egestas&nbsp;semper.</span><em>Aenean&nbsp;ultricies&nbsp;mi&nbsp;vitae&nbsp;est.</em><span style=""color:#ffffff"">Mauris&nbsp;placerat&nbsp;eleifend&nbsp;leo.&nbsp;Quisque&nbsp;sit&nbsp;amet&nbsp;est&nbsp;et&nbsp;sapien&nbsp;ullamcorper&nbsp;pharetra.&nbsp;Vestibulum&nbsp;erat&nbsp;wisi,&nbsp;condimentum&nbsp;sed,</span><code>commodo&nbsp;vitae</code><span style=""color:#ffffff"">,&nbsp;ornare&nbsp;sit&nbsp;amet,&nbsp;wisi.&nbsp;Aenean&nbsp;fermentum,&nbsp;elit&nbsp;eget&nbsp;tincidunt&nbsp;condimentum,&nbsp;eros&nbsp;ipsum&nbsp;rutrum&nbsp;orci,&nbsp;sagittis&nbsp;tempus&nbsp;lacus&nbsp;enim&nbsp;ac&nbsp;dui.</span><a href=""unsafe:javascript:;"" target=""_blank"">Donec&nbsp;non&nbsp;enim</a><span style=""color:#ffffff"">in&nbsp;turpis&nbsp;pulvinar&nbsp;facilisis.&nbsp;Ut&nbsp;felis.</span></p><h2><span class=""ql-font-harlow-solid ql-size-24px"" style=""color:#ffffff"">Header&nbsp;Level&nbsp;2</span></h2><ol><li data-list=""ordered""><span style=""color:#ffffff"">Lorem&nbsp;ipsum&nbsp;dolor&nbsp;sit&nbsp;amet,&nbsp;consectetuer&nbsp;adipiscing&nbsp;elit.</span></li><li data-list=""ordered""><span style=""color:#ffffff"">Aliquam&nbsp;tincidunt&nbsp;mauris&nbsp;eu&nbsp;risus.</span></li></ol><blockquote><span style=""color:#ffffff"">Lorem&nbsp;ipsum&nbsp;dolor&nbsp;sit&nbsp;amet,&nbsp;consectetur&nbsp;adipiscing&nbsp;elit.&nbsp;Vivamus&nbsp;magna.&nbsp;Cras&nbsp;in&nbsp;mi&nbsp;at&nbsp;felis&nbsp;aliquet&nbsp;congue.&nbsp;Ut&nbsp;a&nbsp;est&nbsp;eget&nbsp;ligula&nbsp;molestie&nbsp;gravida.&nbsp;Curabitur&nbsp;massa.&nbsp;Donec&nbsp;eleifend,&nbsp;libero&nbsp;at&nbsp;sagittis&nbsp;mollis,&nbsp;tellus&nbsp;est&nbsp;malesuada&nbsp;tellus,&nbsp;at&nbsp;luctus&nbsp;turpis&nbsp;elit&nbsp;sit&nbsp;amet&nbsp;quam.&nbsp;Vivamus&nbsp;pretium&nbsp;ornare&nbsp;est.</span></blockquote><h3><span style=""color:#ffffff"">Header&nbsp;Level&nbsp;3</span></h3><ul><li data-list=""bullet""><span style=""color:#ffffff"">Lorem&nbsp;ipsum&nbsp;dolor&nbsp;sit&nbsp;amet,&nbsp;consectetuer&nbsp;adipiscing&nbsp;elit.</span></li><li data-list=""bullet""><span style=""color:#ffffff"">Aliquam&nbsp;tincidunt&nbsp;mauris&nbsp;eu&nbsp;risus.</span></li></ul>" :
                @"<h1><span style=""color:#ffffff"">Short&nbsp;Content&nbsp;Test&nbsp;{0}</span></h1><p><span style=""color:#ffffff"">{1}</span><br><span style=""color:#ffffff"">Pellentesque&nbsp;habitant&nbsp;morbi&nbsp;tristique&nbsp;senectus&nbsp;et&nbsp;netus&nbsp;et&nbsp;malesuada&nbsp;fames&nbsp;ac&nbsp;turpis&nbsp;egestas.</span></p>";

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
