using MonkeyPaste.Avalonia;
using System.Diagnostics;
using System.IO.Compression;

namespace MonkeyPaste.Common.Plugin.Ledgerizer {
    internal class Program {

        static string[] Plugins = [
            "ChatGpt",
            "ComputerVision",
            "FileConverter",
            "ImageAnnotator",
            "QrCoder",
            "WebSearch"
        ];
        static bool LEDGERIZE_LOCAL = true;

        static string LocalLedgerDir = MpLedgerConstants.LOCAL_LEDGER_DIR;

        static string LedgerPath =
            Path.Combine(
                LocalLedgerDir,
                LEDGERIZE_LOCAL ?
                    MpLedgerConstants.LOCAL_LEDGER_NAME :
                    MpLedgerConstants.REMOTE_LEDGER_NAME);

        static string LocalReleasesDir = MpLedgerConstants.LOCAL_RELEASE_DIR;

        static void Main(string[] args) {
            MpFileIo.DeleteDirectory(LocalReleasesDir);

            MpManifestLedger ledger = new MpManifestLedger();
            foreach (var plugin in Plugins) {
                string mfp = Path.Combine(
                                        MpCommonHelpers.GetSolutionDir(),
                                        "Plugins",
                                        plugin,
                                        "manifest.json");
                string mft = MpFileIo.ReadTextFromFile(mfp);
                var mf = mft.DeserializeObject<MpManifestFormat>();

                if (LEDGERIZE_LOCAL) {
                    mf.packageUrl = PublishPlugin(Path.GetDirectoryName(mfp));
                    if (mf.packageUrl == null) {
                        continue;
                    }
                }
                ledger.manifests.Add(mf);
            }
            string ledger_text = ledger.SerializeObjectOmitNulls().ToPrettyPrintJson();
            MpFileIo.WriteTextToFile(LedgerPath, ledger_text);
            Console.WriteLine($"Ledger written to: ");
            Console.WriteLine(LedgerPath);
        }

        static string PublishPlugin(string projDir) {
            if (!LocalReleasesDir.IsDirectory()) {
                MpFileIo.CreateDirectory(LocalReleasesDir);
            }
            MpFileIo.DeleteDirectory(Path.Combine(projDir, "bin"));
            MpFileIo.DeleteDirectory(Path.Combine(projDir, "obj"));
            string publish_dir = Path.Combine(LocalReleasesDir, Path.GetFileName(projDir));

            var proc = new Process();
            proc.StartInfo.FileName = @"C:\Program Files\dotnet\dotnet.exe";
            proc.StartInfo.Arguments = $"publish --configuration Release --output {publish_dir}";
            proc.StartInfo.WorkingDirectory = projDir;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            string proc_output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            int exit_code = proc.ExitCode;
            proc.Close();

            if (exit_code != 0) {
                Console.WriteLine("");
                Console.WriteLine($"Error from '{Path.GetFileName(projDir)}' exit code '{exit_code}'");
                Console.WriteLine(proc_output);
                Console.WriteLine("");
                return null;
            }
            if (!publish_dir.IsDirectory()) {
                return null;
            }
            string output_path = Path.Combine(LocalReleasesDir, $"{Path.GetFileName(projDir)}.zip");
            ZipFile.CreateFromDirectory(publish_dir, output_path, CompressionLevel.Fastest, true);
            MpFileIo.DeleteDirectory(publish_dir);
            Console.WriteLine(output_path + " DONE");

            return output_path.ToFileSystemUriFromPath();
        }
    }
}
